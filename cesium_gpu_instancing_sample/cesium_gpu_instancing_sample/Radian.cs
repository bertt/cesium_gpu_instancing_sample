using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cesium_gpu_instancing_sample
{
    public static class Radian
    {
        public static double ToRadius(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

    }
}
