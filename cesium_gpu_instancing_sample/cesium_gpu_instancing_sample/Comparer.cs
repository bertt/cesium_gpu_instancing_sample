
namespace cesium_gpu_instancing_sample
{
    public class Comparer
    {
        public static bool IsSimilar(double first, double second)
        {
            var delta = 0.1;
            return (second > first - delta) && (second < first + delta);
        }

    }
}
