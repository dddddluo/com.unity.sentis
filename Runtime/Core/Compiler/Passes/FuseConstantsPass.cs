using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.Sentis.Compiler.Passes.Optimization
{
    class FuseConstantsPass : IModelPass
    {
        public void Run(ref Model model)
        {
            FuseConstants(ref model);
        }

        static void FuseConstants(ref Model model)
        {
            var ctx = new PartialInferenceContext();
            var backend = new CPUBackend();
            var vars = new ModelStorage();
            var executionContext = new ExecutionContext
            {
                backend = backend,
                storage = vars
            };

            var constantTensors = new Dictionary<string, Tensor>();
            var calculatedTensors = new Dictionary<string, Tensor>();

            // model constants
            foreach (var constant in model.constants)
            {
                var constantTensor = constant.WeightsToTensorWithSharedTensorData();
                constantTensors.Add(constant.index, constantTensor);
                ctx.AddPartialTensor(constant.index, PartialTensor.FromTensor(constantTensor));
            }

            // model inputs
            foreach (var input in model.inputs)
            {
                ctx.AddPartialTensor(input.index, new PartialTensor(input.dataType, input.shape));
            }

            // iterate through layers executing if layer inputs are all known
            foreach (var layer in model.layers)
            {
                var isDeterministic = !layer.GetType().IsDefined(typeof(NonDeterministicOutput));
                for (var i = 0; i < layer.inputs.Length && isDeterministic; i++)
                {
                    isDeterministic &= string.IsNullOrEmpty(layer.inputs[i]) || calculatedTensors.ContainsKey(layer.inputs[i]) || constantTensors.ContainsKey(layer.inputs[i]);
                }

                if (!isDeterministic)
                {
                    // partial tensor inference
                    layer.InferPartial(ctx);
                    for (var i = 0; i < layer.outputs.Length; i++)
                    {
                        var outputPartialTensor = ctx.GetPartialTensor(layer.outputs[i]);
                        if (outputPartialTensor.IsFullyKnown())
                            calculatedTensors.Add(layer.outputs[i], outputPartialTensor.ToTensor());
                    }
                    continue;
                }

                for (var i = 0; i < layer.inputs.Length; i++)
                {
                    Tensor tensor = null;
                    if (string.IsNullOrEmpty(layer.inputs[i]))
                        continue;
                    if (calculatedTensors.TryGetValue(layer.inputs[i], out var calculatedInputTensor))
                        tensor = calculatedInputTensor;
                    else
                        tensor = constantTensors[layer.inputs[i]];

                    executionContext.storage.SetInput(layer.inputs[i], tensor);
                }

                // full inference
                layer.Execute(executionContext);

                for (var i = 0; i < layer.outputs.Length; i++)
                {
                    var outputTensor = executionContext.storage.GetTensor(layer.outputs[i]);
                    calculatedTensors.Add(layer.outputs[i], outputTensor);
                    ctx.AddPartialTensor(layer.outputs[i], PartialTensor.FromTensor(outputTensor));
                }
            }

            // remove precalculated layers
            model.layers.RemoveAll(x =>
            {
                var isLayerCalculated = true;
                foreach (var output in x.outputs)
                {
                    if (string.IsNullOrEmpty(output))
                        continue;
                    isLayerCalculated &= calculatedTensors.ContainsKey(output);
                }
                return isLayerCalculated;
            });

            // add precalculated constants
            foreach (var kvp in calculatedTensors)
            {
                model.constants.Add(new Layers.Constant(kvp.Key, kvp.Value));
                kvp.Value.Dispose();
            }

            // remove unused constants
            var removeUnusedPass = new Cleanup.RemoveUnusedPass();
            removeUnusedPass.Run(ref model);

            foreach (var constantTensor in constantTensors.Values)
            {
                constantTensor.Dispose();
            }

            backend.Dispose();
            vars.Dispose();
        }
    }
}
