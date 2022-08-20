using System.Runtime.InteropServices;
using GlmNet;
using KarafutoCoreCSharp.Source.Misc;

namespace KarafutoCoreCSharp.Source.Bindings;

public class KarafutoLayerLibrary
{
    // layer
    [DllImport("libkcore")]
    public static extern IntPtr CreateTileLayer(float latitude, float longitude);

    // layer
    [DllImport("libkcore")]
    public static extern IntPtr CreateTileLayerWithURL(float latitude, float longitude, string url);
    
    [DllImport("libkcore")]
    public static extern void UpdateProjectionMatrix(IntPtr layerPtr, IntPtr matrixPtr, bool transpose);

    [DllImport("libkcore")]
    public static extern void UpdateViewMatrix(IntPtr layerPtr, IntPtr matrixPtr, bool transpose);

    [DllImport("libkcore")]
    public static extern void Calculate(IntPtr layerPtr);

    public enum LayerEventType
    {
        InFrustum = 0,
        NotInFrustum = 1,
        ImageReady = 2
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LayerEvent
    {
        [MarshalAs(UnmanagedType.I4)] public LayerEventType Type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Quadcode;

        public IntPtr OptionalPayload;
    }

    public enum TileType
    {
        Root = 0,
        Separated = 1,
        Leaf = 2
    }

    public enum TileVisibility
    {
        Hide = 0,
        Visible = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TilePayloadEvent
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] Tilecode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] Center;

        [MarshalAs(UnmanagedType.R4)] public float Scale;

        [MarshalAs(UnmanagedType.I4)] public TileType Type;

        [MarshalAs(UnmanagedType.I4)] public TileVisibility Visible;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Quadcode;
    }

    public enum ImageFormat
    {
        RGB565 = 0,
        RGB888 = 1,
        RGBA8888 = 2
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ImagePayloadEvent
    {
        [MarshalAs(UnmanagedType.U4)] public uint Width;
        [MarshalAs(UnmanagedType.U4)] public uint Height;
        [MarshalAs(UnmanagedType.I4)] public ImageFormat Format;
        [MarshalAs(UnmanagedType.U8)] public long Size;
        public IntPtr Data;
    }

    // elevation

    public enum ElevationSourceType
    {
        File = 0,
        Url = 1
    }

    [DllImport("libkcore")]
    public static extern IntPtr CreateSRTMElevationSource();

    [DllImport("libkcore")]
    public static extern IntPtr AddSRTMPiece(IntPtr elevationPtr, string path, ElevationSourceType type);

    [DllImport("libkcore")]
    public static extern IntPtr GetCoreEventsVector(IntPtr elevationPtr);

    [DllImport("libkcore")]
    public static extern IntPtr GetImageEventsVector(IntPtr elevationPtr);

    [DllImport("libkcore")]
    public static extern IntPtr EjectEventsFromVector(IntPtr vectorPtr, ref int length);

    // mesh

    [DllImport("libkcore")]
    public static extern IntPtr CreateTileMeshQuadcode(IntPtr elevationPtr, string quadcode,
        uint segmentsX, uint segmentsY, bool flipUVsX, bool flipUVsY);

    [DllImport("libkcore")]
    public static extern IntPtr GetMeshVertices(IntPtr meshPtr, ref int length);

    [DllImport("libkcore")]
    public static extern IntPtr GetMeshNormals(IntPtr meshPtr, ref int length);

    [DllImport("libkcore")]
    public static extern IntPtr GetMeshUVs(IntPtr meshPtr, ref int length);

    [DllImport("libkcore")]
    public static extern IntPtr GetMeshIndices(IntPtr meshPtr, ref int length);
}

public class LibraryMesh
{
    private readonly IntPtr _meshPtr;

    public LibraryMesh(SRTMElevation elevationPtr, string quadcode, uint segmentsX = 16, uint segmentsY = 16)
    {
        _meshPtr = KarafutoLayerLibrary.CreateTileMeshQuadcode(
            elevationPtr.ElevationPtr, quadcode, segmentsX, segmentsY, false, false);
    }

    public unsafe float[] GetVertices()
    {
        var length = 0;
        var verticesPtr = KarafutoLayerLibrary.GetMeshVertices(_meshPtr, ref length);

        var vertices = new float[length];
        var gcHandler = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        var pV = gcHandler.AddrOfPinnedObject();

        var bytesCount = length * sizeof(float);
        Buffer.MemoryCopy(verticesPtr.ToPointer(), pV.ToPointer(), 
            bytesCount, bytesCount);

        return vertices;
    }
    
    public IntPtr GetVerticesPtr()
    {
        var length = 0;
        return KarafutoLayerLibrary.GetMeshVertices(_meshPtr, ref length);
    }
    
    public int GetVerticesLength()
    {
        var length = 0;
        KarafutoLayerLibrary.GetMeshVertices(_meshPtr, ref length);
        return length;
    }
    
    public unsafe float[] GetNormals()
    {
        var length = 0;
        var normalsPtr = KarafutoLayerLibrary.GetMeshNormals(_meshPtr, ref length);

        var normals = new float[length];
        var gcHandler = GCHandle.Alloc(normals, GCHandleType.Pinned);
        var pV = gcHandler.AddrOfPinnedObject();

        var bytesCount = length * sizeof(float);
        Buffer.MemoryCopy(normalsPtr.ToPointer(), pV.ToPointer(), 
            bytesCount, bytesCount);

        return normals;
    }
    
    public IntPtr GetNormalsPtr()
    {
        var length = 0;
        return KarafutoLayerLibrary.GetMeshNormals(_meshPtr, ref length);
    }
    
    public int GetNormalsLength()
    {
        var length = 0;
        KarafutoLayerLibrary.GetMeshNormals(_meshPtr, ref length);
        return length;
    }
    
    public unsafe float[] GetUVs()
    {
        var length = 0;
        var uvsPtr = KarafutoLayerLibrary.GetMeshUVs(_meshPtr, ref length);

        var uvs = new float[length];
        var gcHandler = GCHandle.Alloc(uvs, GCHandleType.Pinned);
        var pV = gcHandler.AddrOfPinnedObject();

        var bytesCount = length * sizeof(float);
        Buffer.MemoryCopy(uvsPtr.ToPointer(), pV.ToPointer(), 
            bytesCount, bytesCount);

        return uvs;
    }
    
    public IntPtr GetUVsPtr()
    {
        var length = 0;
        return KarafutoLayerLibrary.GetMeshUVs(_meshPtr, ref length);
    }
    
    public int GetUVsLength()
    {
        var length = 0;
        KarafutoLayerLibrary.GetMeshUVs(_meshPtr, ref length);
        return length;
    }
    
    public unsafe uint[] GetIndices()
    {
        var length = 0;
        var indicesPtr = KarafutoLayerLibrary.GetMeshIndices(_meshPtr, ref length);

        var indices = new uint[length];
        var gcHandler = GCHandle.Alloc(indices, GCHandleType.Pinned);
        var pV = gcHandler.AddrOfPinnedObject();

        var bytesCount = length * sizeof(uint);
        Buffer.MemoryCopy(indicesPtr.ToPointer(), pV.ToPointer(), 
            bytesCount, bytesCount);

        return indices;
    }
    
    public IntPtr GetIndicesPtr()
    {
        var length = 0;
        return KarafutoLayerLibrary.GetMeshIndices(_meshPtr, ref length);
    }
    
    public int GetIndicesLength()
    {
        var length = 0;
        KarafutoLayerLibrary.GetMeshIndices(_meshPtr, ref length);
        return length;
    }
}

public struct ImageEvent
{
    public KarafutoLayerLibrary.LayerEvent Event;
    public KarafutoLayerLibrary.ImagePayloadEvent Payload;
}

public struct TileEvent
{
    public KarafutoLayerLibrary.LayerEvent Event;
    public KarafutoLayerLibrary.TilePayloadEvent? Payload;
}

public class KarafutoLayer
{
    private readonly IntPtr _layerPtr;

    public KarafutoLayer(float latitude, float longitude)
    {
        _layerPtr = KarafutoLayerLibrary.CreateTileLayer(latitude, longitude);
    }

    public KarafutoLayer(float latitude, float longitude, string url)
    {
        _layerPtr = KarafutoLayerLibrary.CreateTileLayerWithURL(latitude, longitude, url);
    }

    public void UpdateProjectionMatrix(mat4 projectionMatrix)
    {
        // allocate memory
        var data = projectionMatrix.to_array();

        var allocSize = data.Length * Marshal.SizeOf<float>();
        var pointerToAlloc = Marshal.AllocHGlobal(allocSize);

        Marshal.Copy(data, 0, pointerToAlloc, data.Length);

        KarafutoLayerLibrary.UpdateProjectionMatrix(_layerPtr, pointerToAlloc, false);

        // release allocated heap memory
        Marshal.FreeHGlobal(pointerToAlloc);
    }

    public void UpdateViewMatrix(mat4 viewMatrix)
    {
        // allocate memory
        var data = viewMatrix.to_array();

        var allocSize = viewMatrix.to_array().Length * Marshal.SizeOf<float>();
        var pointerToAlloc = Marshal.AllocHGlobal(allocSize);

        Marshal.Copy(data, 0, pointerToAlloc, data.Length);

        KarafutoLayerLibrary.UpdateViewMatrix(_layerPtr, pointerToAlloc, false);

        // release allocated heap memory
        Marshal.FreeHGlobal(pointerToAlloc);
    }

    public void Update()
    {
        KarafutoLayerLibrary.Calculate(_layerPtr);
    }

    public IEnumerable<TileEvent> GetCoreEvents()
    {
        var eventsVecPtr = KarafutoLayerLibrary.GetCoreEventsVector(_layerPtr);

        var length = 0;
        var eventsPtr = KarafutoLayerLibrary.EjectEventsFromVector(eventsVecPtr, ref length);
        MarshalUtils.UnmanagedArrayToStructArray(eventsPtr, length, out KarafutoLayerLibrary.LayerEvent[] events);

        var output = new TileEvent[length];
        for (var i = 0; i < events.Length; i++)
        {
            var item = events[i];

            KarafutoLayerLibrary.TilePayloadEvent? payload = null;
            if (item.Type == KarafutoLayerLibrary.LayerEventType.InFrustum)
            {
                payload = Marshal.PtrToStructure<KarafutoLayerLibrary.TilePayloadEvent>(item.OptionalPayload);
            }

            output[i] = new TileEvent
            {
                Payload = payload,
                Event = item
            };
        }

        return output;
    }

    public IEnumerable<ImageEvent> GetImageEvents()
    {
        var eventsVecPtr = KarafutoLayerLibrary.GetImageEventsVector(_layerPtr);

        var length = 0;
        var eventsPtr = KarafutoLayerLibrary.EjectEventsFromVector(eventsVecPtr, ref length);

        MarshalUtils.UnmanagedArrayToStructArray(eventsPtr, length, out KarafutoLayerLibrary.LayerEvent[] events);

        var output = new ImageEvent[length];
        for (var i = 0; i < events.Length; i++)
        {
            var item = events[i];
            var payload = Marshal.PtrToStructure<KarafutoLayerLibrary.ImagePayloadEvent>(item.OptionalPayload);
            output[i] = new ImageEvent
            {
                Payload = payload,
                Event = item
            };
        }

        return output;
    }
}

public class SRTMElevation
{
    public IntPtr ElevationPtr { get; }

    public SRTMElevation()
    {
        ElevationPtr = KarafutoLayerLibrary.CreateSRTMElevationSource();
    }

    public void AddPieceFromFileSystem(string path)
    {
        KarafutoLayerLibrary.AddSRTMPiece(ElevationPtr, path, KarafutoLayerLibrary.ElevationSourceType.File);
    }

    public void AddPieceFromNetwork(string path)
    {
        KarafutoLayerLibrary.AddSRTMPiece(ElevationPtr, path, KarafutoLayerLibrary.ElevationSourceType.Url);
    }
}