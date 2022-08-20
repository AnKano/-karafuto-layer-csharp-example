using System.Numerics;
using KarafutoCoreCSharp.Source.Bindings;
using Raylib_CsLo;

namespace KarafutoCoreCSharp.Source;

public class Tile
{
    public Vector3 Position { get; set; }
    public float Scale { get; set; }
    public Color Color { get; set; }
    public Model? Model { get; set; }

    public Tile(KarafutoLayerLibrary.TilePayloadEvent payload)
    {
        Model = Raylib.LoadModelFromMesh(Raylib.GenMeshPlane(1.0f, 1.0f, 1, 1));

        Position = new Vector3(-payload.Center[0], 0.0f, payload.Center[1]);
        Scale = payload.Scale;

        var random = new Random();
        Color = new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255), 255);
    }

    private unsafe Mesh CreateMeshFromData(LibraryMesh libraryMesh)
    {
        var mesh = new Mesh
        {
            vertexCount = libraryMesh.GetVerticesLength() / 3,
            triangleCount = libraryMesh.GetIndicesLength() / 3
        };

        var vertices = libraryMesh.GetVertices();
        var normals = libraryMesh.GetNormals();

        // get UVs and reverse it horizontaly
        var uvs = libraryMesh.GetUVs();
        var convertedUVs = new float[uvs.Length];
        for (var i = 0; i < uvs.Length; i += 2)
        {
            convertedUVs[i] = uvs[i];
            convertedUVs[i + 1] = 1.0f - uvs[i + 1];
        }

        // all indices created as uint32 values
        // need to convert them to uint16 (or... 'ushort' in CSharp terms)
        var indices = libraryMesh.GetIndices();
        var convertedIndices = Array.ConvertAll(indices, Convert.ToUInt16);

        fixed (float* verticesPtr = vertices)
        fixed (float* uvsPtr = convertedUVs)
        fixed (ushort* indicesPtr = convertedIndices)
        fixed (float* normalsPtr = normals)
        {
            mesh.vertices = verticesPtr;
            mesh.normals = normalsPtr;
            mesh.texcoords = uvsPtr;
            mesh.indices = indicesPtr;
        }

        Raylib.UploadMesh(&mesh, false);
        return mesh;
    }

    public void SetModel(LibraryMesh libraryMesh)
    {
        var mesh = CreateMeshFromData(libraryMesh);
        Model = Raylib.LoadModelFromMesh(mesh);
    }

    public unsafe void SetTexture(KarafutoLayerLibrary.ImagePayloadEvent payload)
    {
        var format = payload.Format switch
        {
            KarafutoLayerLibrary.ImageFormat.RGB565 => PixelFormat.PIXELFORMAT_UNCOMPRESSED_R5G6B5,
            KarafutoLayerLibrary.ImageFormat.RGB888 => PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8,
            KarafutoLayerLibrary.ImageFormat.RGBA8888 => PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8,
            _ => throw new ArgumentOutOfRangeException()
        };

        var image = new Image
        {
            data = payload.Data.ToPointer(),
            width = (int) payload.Width,
            height = (int) payload.Height,
            mipmaps = 1,
            format = (int) format
        };

        var texture = Raylib.LoadTextureFromImage(image);

        if (Model != null)
            Model.Value.materials[0].maps[(int) Raylib.MATERIAL_MAP_DIFFUSE].texture = texture;
    }

    public void Draw()
    {
        if (Model != null)
        {
            Raylib.DrawModelEx(Model.Value, Position,
                new Vector3(0.0f, 1.0f, 0.0f), 180.0f,
                new Vector3(-Scale, 1.0f, Scale), Raylib.WHITE);
        }
    }
}