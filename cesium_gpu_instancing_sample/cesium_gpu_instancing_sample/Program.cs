// See https://aka.ms/new-console-template for more information
using cesium_gpu_instancing_sample;
using Dapper;
using Npgsql;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using System.Numerics;
using Wkx;

Console.WriteLine("Hello, World!");
var conn = new NpgsqlConnection("Host=localhost;Username=postgres;password=postgres;Port=5432");
conn.Open();
var cmd = conn.CreateCommand();

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

var m = ModelRoot.Load("tree.glb");
var meshBuilder = m.LogicalMeshes.First().ToMeshBuilder();
var transform = m.DefaultScene.VisualChildren.ToArray()[4].LocalTransform;

var rnd = new Random(177);

var sceneBuilder = new SceneBuilder();
for (int i = 0; i < 100; i++)
{
    sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
        new Vector3(rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10)),
        transform.Rotation,
        new Vector3(rnd.Next(-50, 50), 20, rnd.Next(-50, 50))));
}

// saving
var gltf = sceneBuilder.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);
gltf.SaveGLB("trees.glb");


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
//var gltf = sceneBuilder.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);
//gltf.SaveGLTF("Box_with_instances.gltf");

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