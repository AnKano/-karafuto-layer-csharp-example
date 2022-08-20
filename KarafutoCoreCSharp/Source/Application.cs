using Raylib_CsLo;

namespace KarafutoCoreCSharp.Source;

public class Application
{
    private const int Width = 1920;
    private const int Height = 1200;
    
    private Scene _scene;
    
    public Application()
    {
        Raylib.InitWindow(Width, Height, "KarafutoLayer C# Example");
        Raylib.SetTargetFPS(60);
        Raylib.SetTraceLogLevel((int) TraceLogLevel.LOG_NONE);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

        _scene = new Scene();
    }

    public void Start()
    {
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            
            Raylib.ClearBackground(Raylib.BLACK);

            _scene.Update();
            _scene.Draw();
            
            Raylib.EndDrawing();
        }
        
        Raylib.CloseWindow();
    }
}