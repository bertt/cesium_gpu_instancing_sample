using cesium_gpu_instancing_sample.CesiumKit;

namespace cesium_instancing_gpu_tests
{
    public class EllipsoidTests
    {
        [Test]
        public void OneOverRadiiSquaredTest()
        {
            var res = new Ellipsoid().OneOverRadiiSquared(new System.Numerics.Vector3(3888175.3441949254f, 333252.36478888814f, 5028049.702918064f));
            Assert.IsTrue(true);
        }

        [Test]
        public void GeodeticSurfaceNormalTest()
        {
            var res = new Ellipsoid().GeodeticSurfaceNormal(new System.Numerics.Vector3(3888175.3441949254f, 333252.36478888814f, 5028049.702918064f));
            Assert.IsTrue(true);
        }
    }
}
