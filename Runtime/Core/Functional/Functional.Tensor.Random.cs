using System;

namespace Unity.Sentis
{
    public static partial class Functional
    {
        /// <summary>
        /// Returns an output generated with values 0 and 1 from a Bernoulli distribution.
        /// </summary>
        /// <param name="input">The probabilities used for generating the output values.</param>
        /// <param name="dataType">The data type of the output.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor Bernoulli(FunctionalTensor input, DataType dataType = DataType.Int, int? seed = null)
        {
            return FunctionalTensor.FromLayer(new Layers.Bernoulli(null, null, dataType, seed), dataType, input);
        }

        /// <summary>
        /// Returns an output generated from the multinomial probability distribution in the corresponding row of the input.
        /// </summary>
        /// <param name="input">The probability distributions.</param>
        /// <param name="numSamples">The number of samples.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor Multinomial(FunctionalTensor input, int numSamples, int? seed = null)
        {
            // TODO add replacement arg
            input = input.Float();
            return FunctionalTensor.FromLayer(new Layers.Multinomial(null, null, numSamples, seed), DataType.Int, input);
        }

        /// <summary>
        /// Returns an output generated by sampling a normal distribution.
        /// </summary>
        /// <param name="mean">The mean of the normal distribution.</param>
        /// <param name="std">The standard deviation of the normal distribution.</param>
        /// <param name="size">The shape of the output tensor.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor Normal(float mean, float std, int[] size, int? seed = null)
        {
            return FunctionalTensor.FromLayer(new Layers.RandomNormal(null, size, mean, std, seed), DataType.Float, Array.Empty<FunctionalTensor>());
        }

        /// <summary>
        /// Returns an output generated by sampling a uniform distribution on the interval [0, 1).
        /// </summary>
        /// <param name="size">The shape of the output tensor.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor Rand(int[] size, int? seed = null)
        {
            return FunctionalTensor.FromLayer(new Layers.RandomUniform(null, size, 0, 1, seed), DataType.Float, Array.Empty<FunctionalTensor>());
        }

        /// <summary>
        /// Returns an output generated by sampling a uniform distribution on the interval [0, 1) with shape matching the input tensor.
        /// </summary>
        /// <param name="input">The input tensor.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor RandLike(FunctionalTensor input, int? seed = null)
        {
            return FunctionalTensor.FromLayer(new Layers.RandomUniformLike(null, null, 0, 1, seed), DataType.Float, input);
        }

        /// <summary>
        /// Returns an output generated by sampling a uniform distribution of integers on the interval [low, high).
        /// </summary>
        /// <param name="size">The shape of the output tensor.</param>
        /// <param name="low">The inclusive minimum value of the interval.</param>
        /// <param name="high">The exclusive maximum value of the interval.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor RandInt(int[] size, int low, int high, int? seed = null)
        {
            return Floor(FunctionalTensor.FromLayer(new Layers.RandomUniform(null, size, low, high, seed), DataType.Float, Array.Empty<FunctionalTensor>())).Int();
        }

        /// <summary>
        /// Returns an output generated by sampling a uniform distribution of integers on the interval [low, high) with shape matching the input tensor.
        /// </summary>
        /// <param name="input">The input tensor.</param>
        /// <param name="low">The inclusive minimum value of the interval.</param>
        /// <param name="high">The exclusive maximum value of the interval.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor RandIntLike(FunctionalTensor input, int low, int high, int? seed = null)
        {
            return Floor(FunctionalTensor.FromLayer(new Layers.RandomUniformLike(null, null, low, high, seed), DataType.Float, input)).Int();
        }

        /// <summary>
        /// Returns an output generated by sampling a standard normal distribution.
        /// </summary>
        /// <param name="size">The shape of the output tensor.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor RandN(int[] size, int? seed = null)
        {
            return FunctionalTensor.FromLayer(new Layers.RandomNormal(null, size, 0, 1, seed), DataType.Float, Array.Empty<FunctionalTensor>());
        }

        /// <summary>
        /// Returns an output generated by sampling a standard normal distribution with shape matching the input tensor.
        /// </summary>
        /// <param name="input">The input tensor.</param>
        /// <param name="seed">The optional seed value for the random number generator.</param>
        /// <returns>The output tensor.</returns>
        public static FunctionalTensor RandNLike(FunctionalTensor input, int? seed = null)
        {
            return FunctionalTensor.FromLayer(new Layers.RandomNormalLike(null, null, 0, 1, seed), DataType.Float, input);
        }
    }
}