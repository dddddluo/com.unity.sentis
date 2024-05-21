using System.Collections.Generic;
using System.Linq;

namespace Unity.Sentis.Compiler.Passes.Cleanup
{
    class RemoveUnusedPass : IModelPass
    {
        bool IsOutputUsed(Layers.Layer layer, HashSet<string> outputsUsed)
        {
            foreach (var lo in layer.outputs)
            {
                if (string.IsNullOrEmpty(lo))
                    continue;
                if (outputsUsed.Contains(lo))
                    return true;
            }

            return false;
        }

        public void Run(ref Model model)
        {
            // algorithm:
            // bottom up graph iteration
            //  layer l
            //  check if previous layers uses l or any of its outputs
            //  if keep and add l.inputs as used outputs
            //  else remove
            var layersToRemove = new HashSet<string>();
            var outputsUsed = new HashSet<string>();
            foreach (var o in model.outputs)
                outputsUsed.Add(o.index);
            for (var i = model.layers.Count - 1; i >= 0; i--)
            {
                var layer = model.layers[i];

                bool isOutputUsed = IsOutputUsed(layer, outputsUsed);

                if (isOutputUsed)
                {
                    foreach (var input in layer.inputs)
                    {
                        if (string.IsNullOrEmpty(input))
                            continue;
                        outputsUsed.Add(input);
                    }
                }
                else
                {
                    layersToRemove.Add(layer.outputs[0]);
                }
            }

            model.layers = model.layers.Where(l => !layersToRemove.Contains(l.outputs[0])).ToList();
            model.constants = model.constants.Where(c => outputsUsed.Contains(c.index)).ToList();
        }
    }
}
