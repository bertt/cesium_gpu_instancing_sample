// See https://aka.ms/new-console-template for more information
using cesium_gpu_instancing_sample;
using Dapper;
using Npgsql;
using SharpGLTF.Geometry.Parametric;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System.Numerics;
using Wkx;
using VPOSNRM = SharpGLTF.Geometry.VertexBuilder<SharpGLTF.Geometry.VertexTypes.VertexPositionNormal, SharpGLTF.Geometry.VertexTypes.VertexEmpty, SharpGLTF.Geometry.VertexTypes.VertexEmpty>;

Console.WriteLine("Hello, World!");
var conn = new NpgsqlConnection("Host=localhost;Username=postgres;password=postgres;Port=5432");
conn.Open();
var cmd = conn.CreateCommand();
var glb = "Box.glb";

var table = "traffic_signs_instances";
var geom_column = "geom";

BoundingBox bbox = GetBoundingBox(conn, cmd, table, geom_column);
var center = new Point(bbox.XMin + (bbox.XMax - bbox.XMin) / 2, bbox.YMin + (bbox.YMax - bbox.YMin) / 2);
// center: 4.8795101545124115, 52.35431072951356
Console.WriteLine(center.X + ", " + center.Y);
SqlMapper.AddTypeHandler(new GeometryTypeHandler());

var center_3857 = (Point)conn.Query<Geometry>($"select ST_AsBinary(ST_Transform('SRID=4326;POINT({center.X} {center.Y})'::geometry, 3857)) as geom").First();
Console.WriteLine("as 3857: " + center_3857.X + ", " + center_3857.Y);

var points_3857 = conn.Query<Geometry>($"select ST_AsBinary(st_transform({geom_column}, 3857)) as geom from {table} where {geom_column} is not null").ToList();

Console.WriteLine("Points: " + points_3857.Count);

var m = ModelRoot.Load("Box.glb");
var mesh = VPOSNRM.CreateCompatibleMesh("shape");

var r = new Quaternion(0.00001f, 0.00001f, 0.000001f, 0.00001f);
// var r = new Quaternion(0.0f, -0.0f, 0.00f, 0.00f);

var material = MaterialBuilder.CreateDefault();

var material1 = new MaterialBuilder()
          .WithDoubleSide(true)
          .WithMetallicRoughnessShader()
          .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 0, 1));

mesh.AddCube(material1, Matrix4x4.Identity);

var sceneBuilder = new SceneBuilder();

var t = new Vector3(0, 0.5f,  0);
sceneBuilder.AddRigidMesh(mesh, (r, t));
sceneBuilder.AddRigidMesh(mesh, (r, new Vector3(10, 0.5f, 0)));
sceneBuilder.AddRigidMesh(mesh, (r, new Vector3(0, 0.5f, 10)));


//foreach (var point in points_3857){

//    if(point is not null)
//    {
//        var p = (Point)point;
//        var t = new Vector3((float)p.X - (float)center_3857.X, 1f, (float)p.Y - (float)center_3857.Y);
//        //var t = new Vector3( (float)center_3857.X - (float)p.X, 0.5f,  (float)center_3857.Y - (float)p.Y);
//        if (t.Length() < 5000)
//        {
//            sceneBuilder.AddRigidMesh(mesh, (r, t));
//            Console.Write('.');
//        }
//    }
//}

// saving
var gltf = sceneBuilder.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);
gltf.SaveGLTF("Box_with_instances.gltf");

static BoundingBox GetBoundingBox(NpgsqlConnection conn, NpgsqlCommand cmd, string table, string geom_column)
{
    var sql = $"SELECT st_xmin(box), st_ymin(box), st_xmax(box), st_ymax(box) FROM (select st_extent({geom_column}) as box from {table}) as total";
    cmd.CommandText = sql;
    var reader = cmd.ExecuteReader();

    reader.Read();
    var xmin = reader.GetDouble(0);
    var ymin = reader.GetDouble(1);
    var xmax = reader.GetDouble(2);
    var ymax = reader.GetDouble(3);
    reader.Close();
    conn.Close();
    var bbox = new BoundingBox(xmin, ymin, xmax, ymax);
    return bbox;
}