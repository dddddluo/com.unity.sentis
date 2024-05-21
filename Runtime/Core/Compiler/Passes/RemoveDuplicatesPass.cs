using System;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using System.Reflection;
using System.Collections;

namespace Unity.Sentis.Compiler.Passes.Optimization
{
    class RemoveDuplicateLayersPass : IModelPass
    {
        long GetHashCode(Layer layer)
        {
            long seed = 0;
            HashHelper.HashCombine(ref seed, layer.GetType());
            foreach (var input in layer.inputs)
                HashHelper.HashCombine(ref seed, input);

            return seed;
        }

        List<object> GetComparableFields(Layer layer)
        {
            var infos = new List<object>();
            var fields = layer.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                var name = field.Name;
                if (name == "index" || name == "outputs")
                    continue;
                infos.Add(field.GetValue(layer));
            }
            return infos;
        }

        bool AllEqual(List<object> l0, List<object> l1)
        {
            if (l0.Count != l1.Count)
                return false;

            for (int i = 0; i < l0.Count; i++)
            {
                var f0 = l0[i];
                var f1 = l1[i];

                if ((f0 is IList a0) && (f1 is IList a1))
                {
                    if (a0.Count != a1.Count)
                        return false;

                    for (int j = 0; j < a0.Count; j++)
                    {
                        var e0 = a0[j]; var e1 = a1[j];
                        if (!Equals(e0, e1))
                            return false;
                    }
                }
                else if (!Equals(f0, f1))
                    return false;
            }

            return true;
        }

        List<object> GetComparableFields(ref Dictionary<string, List<object>> comparableFieldsByLayer, Layer layer)
        {
            if (!comparableFieldsByLayer.TryGetValue(layer.outputs[0], out var layerFields))
            {
                layerFields = GetComparableFields(layer);
                comparableFieldsByLayer.Add(layer.outputs[0], layerFields);
            }

            return layerFields;
        }

        public void Run(ref Model model)
        {
            // Algorithm: remove same layers
            // a layer is the same if it has the same types and all fields and inputs are the same
            // foreach layer:
            //  compute soft hash on layer inputs + type
            //  foreach collision:
            //    remove layer if equal (full param check) to collision
            var remapRemovedIndexes = new Dictionary<string, string>();
            var layersToRemove = new HashSet<string>();
            var layerByInput = new Dictionary<long, List<Layer>>();
            var comparableFieldsByLayer = new Dictionary<string, List<object>>();
            foreach (var layer in model.layers)
            {
                // in place input rename, to propagate removal stat mid traversal
                for (int i = 0; i < layer.inputs.Length; i++)
                {
                    var input = layer.inputs[i];
                    if (remapRemovedIndexes.ContainsKey(input))
                        layer.inputs[i] = remapRemovedIndexes[input];
                }

                long hash = GetHashCode(layer);
                if (!layerByInput.TryGetValue(hash, out var collisionLayers))
                {
                    layerByInput.Add(hash, new List<Layer>() { layer });
                    continue;
                }

                var layerFields = GetComparableFields(ref comparableFieldsByLayer, layer);
                bool removed = false;
                foreach (var similarLayer in collisionLayers)
                {
                    List<object> fields = GetComparableFields(ref comparableFieldsByLayer, similarLayer);

                    if (!AllEqual(layerFields, fields))
                        continue;

                    remapRemovedIndexes.Add(layer.outputs[0], similarLayer.outputs[0]);

                    layersToRemove.Add(layer.outputs[0]);
                    removed = true;

                    if (layer.outputs.Length != similarLayer.outputs.Length)
                        break;

                    for (int i = 0; i < layer.outputs.Length; i++)
                    {
                        if (!remapRemovedIndexes.ContainsKey(layer.outputs[i]))
                            remapRemovedIndexes.Add(layer.outputs[i], similarLayer.outputs[i]);
                    }

                    break;
                }

                if (!removed)
                    collisionLayers.Add(layer);
            }

            model.layers.RemoveAll(l => layersToRemove.Contains(l.outputs[0]));

            // all inputs have been remapped in place, no need to update layers

            // if output was removed, insert copy op
            foreach (var output in model.outputs)
            {
                if (!layersToRemove.Contains(output.index))
                    continue;
                model.layers.Add(new Identity(output.index, remapRemovedIndexes[output.index]));
            }
        }
    }

    class RemoveDuplicateConstantPass : IModelPass
    {
        long GetHashCode(Constant constant)
        {
            long seed = 0;
            HashHelper.HashCombine(ref seed, constant.shape);

            if (constant.shape.HasZeroDims())
                return seed;

            for (int i = 0; i < constant.shape.length; i++)
                HashHelper.HashCombine(ref seed, constant.weights.Get<int>(i));

            return seed;
        }

        bool AreEqual(Constant c0, Constant c1)
        {
            if (c0.shape != c1.shape)
                return false;

            if (c0.shape.HasZeroDims() && c1.shape.HasZeroDims())
                return true;

            for (int i = 0; i < c0.shape.length; i++)
            {
                int v0 = c0.weights.Get<int>(i);
                int v1 = c1.weights.Get<int>(i);
                if (v0 != v1)
                    return false;
            }

            return true;
        }

        public void Run(ref Model model)
        {
            // Algorithm: remove same constant
            // a constant is the same if it's length/shape/weights are all identical
            // foreach constant:
            //  compute first soft hash on constant length
            //  if equal compute hash on constant weights
            //     check secondary hashmap on weight.hash
            //     if collision, hard comparison
            // N.B: no handling of potential wrong collision on weight.hash
            var constantsToRemove = new Dictionary<string, string>();
            var shapeToConstantsByHash = new Dictionary<TensorShape, Dictionary<long, List<Constant>>>();
            foreach (var constant in model.constants)
            {
                if (constant.dataType != DataType.Int)
                    continue;

                var shape = constant.shape;
                if (!shapeToConstantsByHash.TryGetValue(shape, out var constantsByHash))
                {
                    long softHash = GetHashCode(constant);
                    var entry = new Dictionary<long, List<Constant>>();
                    entry.Add(softHash, new List<Constant>() { constant });
                    shapeToConstantsByHash.Add(shape, entry);
                    continue;
                }

                // shape collision, hash on weights
                long hash = GetHashCode(constant);
                if(!constantsByHash.TryGetValue(hash, out var potentialSimilarConstants))
                    continue;

                bool removed = false;
                foreach (var similarConstant in potentialSimilarConstants)
                {
                    // collision, double check values
                    if (!AreEqual(constant, similarConstant))
                        continue;

                    removed = true;
                    constantsToRemove.Add(constant.index, similarConstant.index);
                    break;
                }

                if (!removed)
                    potentialSimilarConstants.Add(constant);
            }

            model.constants.RemoveAll(c => constantsToRemove.ContainsKey(c.index));
            foreach (var layer in model.layers)
            {
                for (int i = 0; i < layer.inputs.Length; i++)
                {
                    var input = layer.inputs[i];
                    if (constantsToRemove.ContainsKey(input))
                        layer.inputs[i] = constantsToRemove[input];
                }
            }
            foreach (var output in model.outputs)
            {
                if (constantsToRemove.ContainsKey(output.index))
                    model.AddLayer(new Identity(output.index, constantsToRemove[output.index]));
            }
        }
    }

    class RemoveDuplicatesPass : IModelPass
    {
        public void Run(ref Model model)
        {
            var removeConstants = new RemoveDuplicateConstantPass();
            var removeLayers = new RemoveDuplicateLayersPass();

            removeConstants.Run(ref model);
            removeLayers.Run(ref model);
        }
    }
}
