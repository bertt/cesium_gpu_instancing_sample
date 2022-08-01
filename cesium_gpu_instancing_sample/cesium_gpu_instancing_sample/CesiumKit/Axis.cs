﻿using System.Numerics;

namespace cesium_gpu_instancing_sample.CesiumKit
{
    public static class Axis
    {
        public static Matrix4x4 Z_UP_TO_Y_UP()
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, -1, 0, 0,
                0, 0, 0, 1);
        }

        public static Matrix4x4 Y_UP_TO_Z_UP()
        {
            return new Matrix4x4(
                1, 0, 0, 0,
                0, 0, -1, 0,
                0, 1, 0, 0,
                0, 0, 0, 1);
        }
    }
}
