using cesium_gpu_instancing_sample;
using cesium_gpu_instancing_sample.CesiumKit;
using System.Numerics;

namespace cesium_instancing_gpu_tests
{
    public class TransformTests
    {
       [Test]
        public void EastNorthUpToFixedFrameTest()
        {
            // arrange
            var cartesian = new Vector3(3888175.3441949254f, 333252.36478888814f, 5028049.702918064f);
            
            // act
            var enu = Transforms.EastNorthUpToFixedFrame(cartesian);

            // assert
            Assert.IsTrue(enu.M11 == -0.08539611f);
            Assert.IsTrue(enu.M12 == -0.78907776f);
            Assert.IsTrue(enu.M13 == 0.6083287f);
            Assert.IsTrue(enu.M22 == -0.067631215f);
            Assert.IsTrue(enu.M23 == 0.052139364f);
            Assert.IsTrue(enu.M31 == 0);
            Assert.IsTrue(enu.M32 == 0.610559f);
            Assert.IsTrue(enu.M32 == 0.610559f);
            Assert.IsTrue(enu.M33 == 0.79197073f);
        }

        [Test]
        public void GetQuaternionTests()
        {
            // arrange
            var cartesian = new Vector3(3888175.3441949254f, 333252.36478888814f, 5028049.702918064f);
            var enu = Transforms.EastNorthUpToFixedFrame(cartesian);

            // act
            var random = new Random();
            var rad = Radian.ToRadius(random.Next(0, 360));

            var quaternion = Transforms.GetQuaterion(enu, 0);
            Assert.IsTrue(quaternion.X == 0.6068409f);
            Assert.IsTrue(quaternion.Y == 0.325076f);
            Assert.IsTrue(quaternion.Z == -0.6610776236386398f);
            Assert.IsTrue(quaternion.W == 0.2984059f);
        }
    }
}