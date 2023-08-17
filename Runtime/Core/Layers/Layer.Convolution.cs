using System;

namespace Unity.Sentis.Layers
{
    /// <summary>
    /// Options for auto padding in image layers.
    /// </summary>
    public enum AutoPad
    {
        /// <summary>
        /// Use explicit padding.
        /// </summary>
        NotSet,
        /// <summary>
        /// Use no padding.
        /// </summary>
        Valid,
        /// <summary>
        /// Use equal or almost equal padding on both sides. When the padding is odd, add the extra value to the end.
        /// </summary>
        SameUpper,
        /// <summary>
        /// Use equal or almost equal padding on both sides. When the padding is odd, add the extra value to the start.
        /// </summary>
        SameLower,
    }

    /// <summary>
    /// Represents a `Conv` convolution layer, which applies a convolution filter to an input tensor.
    /// </summary>
    [Serializable]
    public class Conv : FusedActivation
    {
        static int[] s_DefaultStrides = { 1, 1, 1, 1, 1, 1 };
        static int[] s_DefaultPads = new int[12];
        static int[] s_DefaultDilations = { 1, 1, 1, 1, 1, 1 };

        /// <summary>
        /// The auto padding mode of the convolution as an `AutoPad`.
        /// </summary>
        public AutoPad autoPad;
        /// <summary>
        /// The dilation value of each spatial dimension of the filter.
        ///
        /// If this is `null` the layer uses a default of [1, 1, ..., 1].
        /// </summary>
        public int[] dilations;
        /// <summary>
        /// The number of groups that input channels and output channels are divided into.
        /// </summary>
        public int group;
        /// <summary>
        /// The lower and upper padding values for each spatial dimension of the filter, [pad_left, pad_right] for 1D, [pad_top, pad_bottom, pad_left, pad_right] for 2D, etc.
        ///
        /// If this is `null` the layer uses a default of [0, 0, ..., 0].
        /// </summary>
        public int[] pads;
        /// <summary>
        /// The stride value for each spatial dimension of the filter.
        ///
        /// If this is `null` the layer uses a default of [1, 1, ..., 1].
        /// </summary>
        public int[] strides;
        public int[] kernelShape;

        /// <summary>
        /// Initializes and returns an instance of `Conv` convolution layer.
        /// </summary>
        /// <param name="name">The name to use for the output tensor of the layer.</param>
        /// <param name="X">The name to use for the input tensor of the layer.</param>
        /// <param name="W">The name to use for the filter tensor of the layer.</param>
        /// <param name="B">The name to use for the optional bias tensor of the layer.</param>
        /// <param name="group">The number of groups that input channels and output channels are divided into.</param>
        /// <param name="strides">The optional stride value for each spatial dimension of the filter.</param>
        /// <param name="pads">The optional lower and upper padding values for each spatial dimension of the filter.</param>
        /// <param name="dilations">The optional dilation value of each spatial dimension of the filter.</param>
        /// <param name="autoPad">The auto padding mode of the convolution as an `AutoPad`.</param>
        /// <param name="fusedActivation">The fused activation type to apply after the convolution. The default value is `None`.</param>
        public Conv(string name, string X, string W, string B, int group, int[] strides, int[] pads, int[] dilations, AutoPad autoPad = AutoPad.NotSet, int[] kernelShape = null, FusableActivation fusedActivation = FusableActivation.None)
        {
            this.name = name;
            inputs = new[] { X, W, B };
            this.autoPad = autoPad;
            this.dilations = dilations;
            this.group = group;
            this.pads = pads;
            this.strides = strides;
            this.kernelShape = kernelShape;
            this.fusedActivation = fusedActivation;
        }

        /// <inheritdoc/>
        internal override PartialTensor InferPartialTensor(PartialTensor[] inputTensors, PartialInferenceContext ctx)
        {
            var shapeX = inputTensors[0].shape;
            var shapeKernel = inputTensors[1].shape;
            for (var i = 0; kernelShape != null && i < kernelShape.Length; i++)
            {
                shapeKernel[i + 2] = SymbolicTensorDim.MaxDefinedDim(shapeKernel[i + 2], new SymbolicTensorDim(kernelShape[i]));
            }
            var shapeBias = inputTensors[2]?.shape ?? SymbolicTensorShape.UnknownShape;
            if (!shapeX.hasRank)
                return new PartialTensor(DataType.Float);

            shapeKernel.DeclareRank(shapeX.rank);
            shapeBias.DeclareRank(1);

            Logger.AssertIsTrue(strides == null || shapeX.rank - 2 == strides.Length, "Conv.InputError: strides must be the same length as the spatial dimensions or be null");
            Logger.AssertIsTrue(pads == null || 2 * (shapeX.rank - 2) == pads.Length, "Conv.InputError: padding must have two values per spatial dimension or be null");
            Logger.AssertIsTrue(dilations == null || shapeX.rank - 2 == dilations.Length, "Conv.InputError: dilations must be the same length as the spatial dimensions or be null");

            var shapeOut = SymbolicTensorShape.UnknownOfRank(shapeX.rank);

            shapeOut[0] = shapeX[0];
            shapeOut[1] = SymbolicTensorDim.MaxDefinedDim(shapeKernel[0], shapeBias[0]);

            for (var i = 2; i < shapeOut.rank; i++)
            {
                var stride = strides == null ? 1 : strides[i - 2];
                var pad = pads == null || autoPad != AutoPad.NotSet ? 0 : pads[i - 2] + pads[i - 2 + (shapeX.rank - 2)];
                var dilation = dilations == null ? 1 : dilations[i - 2];
                var dimX = shapeX[i];
                var dimKernel = shapeKernel[i];
                if (dimKernel.isValue)
                    shapeOut[i] = dimX.Pool(dimKernel.value, stride, pad, dilation, false, autoPad);
                else if (dimKernel.isParam && (autoPad is AutoPad.SameLower || autoPad is AutoPad.SameUpper))
                    shapeOut[i] = dimX.Pool(0, stride, pad, dilation, false, autoPad);
                else
                    shapeOut[i] = SymbolicTensorDim.Unknown;
            }

            return new PartialTensor(DataType.Float, shapeOut);
        }

        /// <inheritdoc/>
        public override Tensor Execute(Tensor[] inputTensors, ExecutionContext ctx)
        {
            var numSpatialDims = inputTensors[0].shape.rank - 2;
            var stridesSpan = strides ?? s_DefaultStrides.AsSpan(0, numSpatialDims);
            var padsSpan = pads ?? s_DefaultPads.AsSpan(0, 2 * numSpatialDims);
            var dilationsSpan = dilations ?? s_DefaultDilations.AsSpan(0, numSpatialDims);
            ShapeInference.UpdatePadForConvAutoPadding(inputTensors[0].shape, inputTensors[1].shape, stridesSpan, dilationsSpan, autoPad, padsSpan);
            return ctx.backend.Conv(inputTensors[0] as TensorFloat, inputTensors[1] as TensorFloat, inputTensors.Length > 2 ? inputTensors[2] as TensorFloat : null, group, stridesSpan, padsSpan, dilationsSpan, fusedActivation);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}, group: {group}, strides: [{(strides == null ? "null" : string.Join(", ", strides))}], pads: [{(pads == null ? "null" : string.Join(", ", pads))}], dilations: [{(dilations == null ? "null" : string.Join(", ", dilations))}], autoPad: {autoPad}, kernelShape: [{(kernelShape == null ? "null" : string.Join(", ", kernelShape))}], fusedActivation: {fusedActivation}";
        }

        internal override string profilerTag => "Conv";
    }

    /// <summary>
    /// Represents a `ConvTranspose` transpose convolution layer, which applies a convolution filter to an input tensor.
    /// </summary>
    [Serializable]
    public class ConvTranspose : FusedActivation
    {
        static int[] s_DefaultStrides = { 1, 1, 1, 1, 1, 1 };
        static int[] s_DefaultPads = new int[12];
        static int[] s_DefaultOutputPadding = new int[6];

        /// <summary>
        /// The auto padding mode of the transpose convolution.
        /// </summary>
        public AutoPad autoPad;
        /// <summary>
        /// The output padding value for each spatial dimension in the filter.
        ///
        /// The layer adds the output padding to the side with higher coordinate indices in the output tensor.
        ///
        /// If this is `null` the layer uses a default of [0, 0, ..., 0].
        /// </summary>
        public int[] outputPadding;
        /// <summary>
        /// The lower and upper padding values for each spatial dimension of the filter. For example [pad_left, pad_right] for 1D, or [pad_top, pad_bottom, pad_left, pad_right] for 2D.
        ///
        /// If this is `null` the layer uses a default of [0, 0, ..., 0].
        /// </summary>
        public int[] pads;
        /// <summary>
        /// The stride value for each spatial dimension of the filter.
        ///
        /// If this is `null` the layer uses a default of [1, 1, ..., 1].
        /// </summary>
        public int[] strides;
        public int[] kernelShape;

        /// <summary>
        /// Initializes and returns an instance of `ConvTranspose` convolution layer.
        /// </summary>
        /// <param name="name">The name to use for the output tensor of the layer.</param>
        /// <param name="input">The name to use for the input tensor of the layer.</param>
        /// <param name="kernel">The name to use for the filter tensor of the layer.</param>
        /// <param name="bias">The name to use for the optional bias tensor of the layer.</param>
        /// <param name="strides">The optional stride value for each spatial dimension of the filter.</param>
        /// <param name="pads">The optional lower and upper padding values for each spatial dimension of the filter.</param>
        /// <param name="autoPad">The auto padding mode of the convolution.</param>
        /// <param name="outputPadding">The output padding value for each spatial dimension in the filter.</param>
        /// <param name="fusedActivation">The fused activation type to apply after the convolution. The default value is `None`.</param>
        public ConvTranspose(string name, string input, string kernel, string bias, int[] strides, int[] pads, AutoPad autoPad, int[] outputPadding, int[] kernelShape = null, FusableActivation fusedActivation = FusableActivation.None)
        {
            this.name = name;
            inputs = new[] { input, kernel, bias };
            this.autoPad = autoPad;
            this.outputPadding = outputPadding;
            this.pads = pads;
            this.strides = strides;
            this.kernelShape = kernelShape;
            this.fusedActivation = fusedActivation;
        }

        /// <inheritdoc/>
        internal override PartialTensor InferPartialTensor(PartialTensor[] inputTensors, PartialInferenceContext ctx)
        {
            var shapeX = inputTensors[0].shape;
            var shapeKernel = inputTensors[1].shape;
            for (var i = 0; kernelShape != null && i < kernelShape.Length; i++)
            {
                shapeKernel[i + 2] = SymbolicTensorDim.MaxDefinedDim(shapeKernel[i + 2], new SymbolicTensorDim(kernelShape[i]));
            }
            var shapeBias = inputTensors[2]?.shape ?? SymbolicTensorShape.UnknownShape;
            if (!shapeX.hasRank)
                return new PartialTensor(DataType.Float);

            shapeKernel.DeclareRank(shapeX.rank);

            Logger.AssertIsTrue(strides == null || shapeX.rank - 2 == strides.Length, "ConvTranspose.InputError: strides must have two less values than the rank of input or be null");
            Logger.AssertIsTrue(pads == null || 2 * (shapeX.rank - 2) == pads.Length, "ConvTranspose.InputError: padding must have two values per spatial dimension or be null");
            Logger.AssertIsTrue(outputPadding == null || shapeX.rank - 2 == outputPadding.Length, "ConvTranspose.InputError: outputPadding must have two values per spatial dimension or be null");

            var shapeOut = SymbolicTensorShape.Ones(shapeX.rank);

            shapeOut[0] = shapeX[0];
            shapeOut[1] = SymbolicTensorDim.MaxDefinedDim(shapeKernel[1], shapeBias[0]);

            for (var i = 2; i < shapeOut.rank; i++)
            {
                var stride = strides == null ? 1 : strides[i - 2];
                var pad = pads == null || autoPad != AutoPad.NotSet ? 0 : pads[i - 2] + pads[i - 2 + (shapeX.rank - 2)];
                var dilation = 1;
                var outputPad = outputPadding == null ? 0 : outputPadding[i - 2];
                var dimX = shapeX[i];
                var dimKernel = shapeKernel[i];
                if (autoPad == AutoPad.NotSet)
                    shapeOut[i] = stride * (dimX - 1) + outputPad + (dimKernel - 1) * dilation + 1 - pad;
                else
                    shapeOut[i] = dimX * stride;
            }

            return new PartialTensor(DataType.Float, shapeOut);
        }

        /// <inheritdoc/>
        public override Tensor Execute(Tensor[] inputTensors, ExecutionContext ctx)
        {
            var numSpatialDims = inputTensors[0].shape.rank - 2;
            var stridesSpan = strides ?? s_DefaultStrides.AsSpan(0, numSpatialDims);
            var padsSpan = pads ?? s_DefaultPads.AsSpan(0, 2 * numSpatialDims);
            var outputPaddingSpan = outputPadding ?? s_DefaultOutputPadding.AsSpan(0, numSpatialDims);
            ShapeInference.UpdatePadForConvTransAutoPadding(inputTensors[0].shape, inputTensors[1].shape, stridesSpan, autoPad, outputPaddingSpan, padsSpan);
            return ctx.backend.ConvTranspose(inputTensors[0] as TensorFloat, inputTensors[1] as TensorFloat, inputTensors.Length > 2 ? inputTensors[2] as TensorFloat : null, stridesSpan, padsSpan, outputPaddingSpan, fusedActivation);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}, strides: [{(strides == null ? "null" : string.Join(", ", strides))}], pads: [{(pads == null ? "null" : string.Join(", ", pads))}], outputPadding: [{(outputPadding == null ? "null" : string.Join(", ", outputPadding))}], autoPad, {autoPad}, kernelShape: [{(kernelShape == null ? "null" : string.Join(", ", kernelShape))}], fusedActivation: {fusedActivation}";
        }

        internal override string profilerTag => "ConvTranspose";
    }
}
