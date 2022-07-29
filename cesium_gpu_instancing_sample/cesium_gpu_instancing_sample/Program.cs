// See https://aka.ms/new-console-template for more information
using cesium_gpu_instancing_sample;
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
var center = bbox3d.GetCenter();
Console.WriteLine(center.X + ", " + center.Y + "," + center.Z);
SqlMapper.AddTypeHandler(new GeometryTypeHandler());
var translation = bbox3d.GetCenter().ToVector();
var sr = 4978;

var subtreebytes = GetSubtreeBytes("1");
File.WriteAllBytes($"subtrees/0_0_0.subtree", subtreebytes);


var boundingboxAllFeatures = BoundingBoxCalculator.TranslateRotateX(bbox3d, Reverse(translation), Math.PI / 2);
var box = boundingboxAllFeatures.GetBox();

//3889489.6901732744,
//      332141.2123117483,
//      5026974.8389739739,

var sql = $"SELECT ST_AsBinary(ST_RotateX(ST_Translate(st_transform(st_force3d({geom_column}), 4978), 3889489.6901732744 * -1, 332141.2123117483 * -1, 5026974.8389739739 * -1), -pi() / 2)) as geom FROM {table} where {geom_column} is not null";
// var sql = $"SELECT ST_AsBinary(ST_Translate(st_transform(st_force3d({geom_column}), 4978), 3889489.6901732744 * -1, 332141.2123117483 * -1, 5026974.8389739739 * -1)) as geom FROM {table} where {geom_column} is not null";
// "select ST_AsBinary(st_transform({geom_column}, 4978)) as geom from {table} where {geom_column} is not null"
var points_3857 = conn.Query<Geometry>(sql).ToList();

Console.WriteLine("Points: " + points_3857.Count);

var m = ModelRoot.Load("tree.glb");
var meshBuilder = m.LogicalMeshes.First().ToMeshBuilder();
var transform = m.DefaultScene.VisualChildren.ToArray()[4].LocalTransform;

var rnd = new Random(177);

var sceneBuilder = new SceneBuilder();

foreach (var point in points_3857)
{

    if (point is not null)
    {
        var p = (Point)point;
        var t = new Vector3((float)p.X , (float)p.Y + 10 , (float)p.Z );
        //var t = new Vector3( (float)center_3857.X - (float)p.X, 0.5f,  (float)center_3857.Y - (float)p.Y);
        sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
            new Vector3(5, 5, 5),
            transform.Rotation,
            t));
        Console.Write('.');
    }
}


// AddAxis(meshBuilder, transform, sceneBuilder);

// saving
var gltf = sceneBuilder.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);
gltf.SaveGLB("content/0_0_0.glb");


static double[] Reverse(double[] translation)
{
    var res = new double[] { translation[0] * -1, translation[1] * -1, translation[2] * -1 };
    return res;
}

// saving
//var gltf = sceneBuilder.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);
//gltf.SaveGLTF("Box_with_instances.gltf");

static BoundingBox3D GetBoundingBox(NpgsqlConnection conn, NpgsqlCommand cmd, string table, string geom_column)
{
    //var sql = $"SELECT st_xmin(box), st_ymin(box), st_xmax(box), st_ymax(box) FROM (select st_extent({geom_column}) as box from {table}) as total";
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

static void AddAxis(SharpGLTF.Geometry.IMeshBuilder<SharpGLTF.Materials.MaterialBuilder> meshBuilder, AffineTransform transform, SceneBuilder sceneBuilder)
{
    // center tree
    sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
        new Vector3(1, 1, 1),
        transform.Rotation,
        new Vector3(0, 5, 0)));

    sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
        new Vector3(2, 2, 2),
        transform.Rotation,
        new Vector3(100, 5, 0)));

    sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
        new Vector3(3, 3, 3),
        transform.Rotation,
        new Vector3(0, 5, 100)));

    sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
        new Vector3(4, 4, 4),
        transform.Rotation,
        new Vector3(-100, 5, 0)));

    sceneBuilder.AddRigidMesh(meshBuilder, new AffineTransform(
        new Vector3(5, 5, 5),
        transform.Rotation,
        new Vector3(0, 5, -100)));
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
