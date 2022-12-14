using Dapper;
using System.Data;
using Wkx;

namespace cesium_gpu_instancing_sample
{
    public class GeometryTypeHandler : SqlMapper.TypeHandler<Geometry>
    {
        public override Geometry Parse(object value)
        {
            if (value == null)
                return null;

            var stream = (byte[])value;
            var g = Geometry.Deserialize<WkbSerializer>(stream);
            return g;
        }

        public override void SetValue(IDbDataParameter parameter, Geometry value)
        {
            parameter.Value = value;
        }
    }
}
