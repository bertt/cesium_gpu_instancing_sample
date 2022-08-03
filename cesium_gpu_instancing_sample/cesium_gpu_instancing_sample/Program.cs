// See https://aka.ms/new-console-template for more information
using cesium_gpu_instancing_sample;
using cesium_gpu_instancing_sample.CesiumKit;
using Dapper;
using Npgsql;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using subtree;
using System.Numerics;
using Wkx;

Console.WriteLine("Hello, World!");
var conn = new NpgsqlConnection("Host=localhost;Username=postgres;password=postgres;Port=5432");
conn.Open();
var cmd = conn.CreateCommand();

var table = "traffic_signs_instances";
var geom_column = "geom";

var bbox3d = GetBoundingBox(conn, cmd, table, geom_column);
SqlMapper.AddTypeHandler(new GeometryTypeHandler());
var translation = bbox3d.GetCenter().ToVector();
var sr = 4978;

var subtreebytes = GetSubtreeBytes("1");
File.WriteAllBytes($"subtrees/0_0_0.subtree", subtreebytes);

var sql = $"SELECT ST_AsBinary(ST_RotateX(ST_Translate(st_transform(st_force3d({geom_column}), {sr}), {translation[0]} * -1, {translation[1]} * -1, {translation[2]} * -1), -pi() / 2)) as geom FROM {table} where {geom_column} is not null";

var points_4978 = conn.Query<Geometry>(sql).ToList();

conn.Close();

Console.WriteLine("Points: " + points_4978.Count);

var m = ModelRoot.Load("tree.glb");
var meshBuilder = m.LogicalMeshes.First().ToMeshBuilder();
var sceneBuilder = new SceneBuilder();

var rnd = new Random();
foreach (var point in points_4978)
{
    if (point is not null)
    {
        var enu = Transforms.EastNorthUpToFixedFrame(new Vector3((float)translation[0], (float)translation[1], (float)translation[2]));
        var random = new Random();
        var rad = Radian.ToRadius(random.Next(0, 360));

        var quaternion = Transforms.GetQuaterion(enu, rad);
        var p = (Point)point;

        var scaleRandom = rnd.Next(1, 5);
        var scale = new Vector3(scaleRandom, scaleRandom, scaleRandom);
        var translate = new Vector3((float)p.X , (float)p.Y + 5f * scaleRandom , (float)p.Z );
        sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
            scale,
            quaternion,
            translate));

        Console.Write('.');
    }
}

// saving
var gltf = sceneBuilder.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);
gltf.SaveGLB("content/0_0_0.glb");

static BoundingBox3D GetBoundingBox(NpgsqlConnection conn, NpgsqlCommand cmd, string table, string geom_column)
{
    var sql = $"SELECT st_xmin(box), ST_Ymin(box), ST_Zmin(box), ST_Xmax(box), ST_Ymax(box), ST_Zmax(box) FROM(select ST_3DExtent(st_transform(ST_Force3D(geom), 4978)) AS box from {table} ) as total";

    cmd.CommandText = sql;
    var reader = cmd.ExecuteReader();

    reader.Read();
    var xmin = reader.GetDouble(0);
    var ymin = reader.GetDouble(1);
    var zmin = reader.GetDouble(2);
    var xmax = reader.GetDouble(3);
    var ymax = reader.GetDouble(4);
    var zmax = reader.GetDouble(5);

    reader.Close();
    conn.Close();
    var bbox = new BoundingBox3D(xmin, ymin, zmin, xmax, ymax, zmax);
    return bbox;
}


static byte[] GetSubtreeBytes(string contentAvailability, string subtreeAvailability = null)
{
    var subtree_root = new Subtree();
    subtree_root.TileAvailabiltyConstant = 1;

    var s0_root = BitArrayCreator.FromString(contentAvailability);
    subtree_root.ContentAvailability = s0_root;

    if (subtreeAvailability != null)
    {
        var c0_root = BitArrayCreator.FromString(subtreeAvailability);
        subtree_root.ChildSubtreeAvailability = c0_root;
    }

    var subtreebytes = SubtreeWriter.ToBytes(subtree_root);
    return subtreebytes;
}
