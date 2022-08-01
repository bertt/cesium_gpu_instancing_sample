using System.Numerics;

namespace cesium_gpu_instancing_sample.CesiumKit
{
    public class Ellipsoid
    {
        public Ellipsoid()
        {
            SemiMajorAxis = 6378137;
            SemiMinorAxis = 6356752.3142478326;
            Eccentricity = 0.081819190837553915;
        }
        public double SemiMajorAxis { get; }
        public double SemiMinorAxis { get; }

        public double Eccentricity { get; }

        public Vector3 GeodeticSurfaceNormal(Vector3 cartesian)
        {
            return Vector3.Normalize(cartesian*OneOverRadiiSquared(cartesian));
        }

        public Vector3 OneOverRadiiSquared(Vector3 cartesian) {
            var x = cartesian.X == 0 ? 0.0 : 1.0 / (cartesian.X * cartesian.X);
            var y = cartesian.Y == 0 ? 0.0 : 1.0 / (cartesian.Y * cartesian.Y);
            var z = cartesian.Y == 0 ? 0.0 : 1.0 / (cartesian.Z * cartesian.Z);

            var res = new Vector3((float)x, (float)y, (float)z);
            return res;

        }

    }
}
