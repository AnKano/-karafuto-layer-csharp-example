using System.Numerics;
using KarafutoCoreCSharp.Source.Bindings;
using Raylib_CsLo;

namespace KarafutoCoreCSharp.Source;

public class Scene
{
    private readonly KarafutoLayer _layer;
    private readonly SRTMElevation _elevation;
    private readonly Camera _camera;

    private readonly Dictionary<string, Tile> _tiles;

    private readonly Model _model;

    public Scene()
    {
        _model = Raylib.LoadModelFromMesh(Raylib.GenMeshPlane(1.0f, 1.0f, 1, 1));

        _camera = new Camera(
            new Vector3(1.0f, 5.0f, 10.0f),
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            60.0f);
        
        _layer = new KarafutoLayer(46.9181f, 142.7189f);
        _elevation = new SRTMElevation();

        _tiles = new Dictionary<string, Tile>();

        // fill elevation source with srtm-files
        {
            _elevation.AddPieceFromFileSystem("Assets/elevation/N45E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N45E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N46E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N46E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N46E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N47E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N47E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N47E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N48E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N48E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N48E144.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N49E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N49E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N49E144.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N50E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N50E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N51E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N51E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N51E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N52E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N52E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N52E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N53E141.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N53E142.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N53E143.hgt");
            _elevation.AddPieceFromFileSystem("Assets/elevation/N54E142.hgt");
        }
    }

    private void UpdateMatrices()
    {
        var prj = _camera.GetProjectionMatrix(1920.0f / 1200.0f);
        _layer.UpdateProjectionMatrix(prj);

        var view = _camera.GetViewMatrix();
        _layer.UpdateViewMatrix(view);

        _layer.Update();
    }

    private void UpdateScene()
    {
        var cores = _layer.GetCoreEvents();
        foreach (var @event in cores)
        {
            var quadcode = @event.Event.Quadcode;

            if (@event.Event.Type == KarafutoLayerLibrary.LayerEventType.InFrustum)
            {
                if (@event.Payload != null)
                {
                    _tiles[quadcode] = new Tile(@event.Payload.Value);

                    // here call the mesh
                    var mesh = new LibraryMesh(_elevation, quadcode);
                    _tiles[quadcode].SetModel(mesh);
                }
            }

            if (@event.Event.Type == KarafutoLayerLibrary.LayerEventType.NotInFrustum)
            {
                if (_tiles.ContainsKey(quadcode))
                    _tiles.Remove(quadcode);
            }
        }

        var images = _layer.GetImageEvents();
        foreach (var @event in images)
        {
            var quadcode = @event.Event.Quadcode;

            if (@event.Event.Type == KarafutoLayerLibrary.LayerEventType.ImageReady)
            {
                if (_tiles.ContainsKey(quadcode))
                    _tiles[quadcode].SetTexture(@event.Payload);
            }
        }
    }

    public void Update()
    {
        _camera.Update();

        UpdateMatrices();
        UpdateScene();
    }

    public void Draw()
    {
        Raylib.BeginMode3D(_camera.RaylibCamera);

        RlGl.rlDisableBackfaceCulling();
        
        foreach (var pair in _tiles)
        {
            pair.Value.Draw();
        }

        Raylib.EndMode3D();
    }
}