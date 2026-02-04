namespace Aer.QdrantClient.Tests.Helpers;

internal static class DoubleExtensions
{
    extension(double[] arrayOfDoubles)
    {
        public float[] ToFloat()
        {
            float[] ret = new float[arrayOfDoubles.Length];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = checked((float)arrayOfDoubles[i]);
            }

            return ret;
        }
    }
}
