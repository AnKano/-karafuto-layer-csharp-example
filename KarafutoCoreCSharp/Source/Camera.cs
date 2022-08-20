using System.Numerics;
using GlmNet;
using Raylib_CsLo;

namespace KarafutoCoreCSharp.Source;

public class Camera
{
    private const float CameraNear = 0.1f;
    private const float CameraFar = 100.0f;

    public Camera3D RaylibCamera;

    public Camera(Vector3 position, Vector3 target, Vector3 up, float fovy)
    {
        RaylibCamera = new Camera3D(position, target, up, fovy, CameraProjection.CAMERA_PERSPECTIVE);
        Raylib.SetCameraMode(RaylibCamera, CameraMode.CAMERA_FREE);
    }

    public void Update()
    {
        Raylib.UpdateCamera(ref RaylibCamera);
    }

    public mat4 GetProjectionMatrix(float aspectRatio)
    {
        return glm.perspective(glm.radians(RaylibCamera.fovy), aspectRatio, CameraNear, CameraFar);
    }

    public mat4 GetViewMatrix()
    {
        var camPos = new vec3(-RaylibCamera.position.X, RaylibCamera.position.Y, RaylibCamera.position.Z);
        var camTarget = new vec3(-RaylibCamera.target.X, RaylibCamera.target.Y, RaylibCamera.target.Z);
        var camUp = new vec3(-RaylibCamera.up.X, RaylibCamera.up.Y, RaylibCamera.up.Z);

        return glm.lookAt(camPos, camTarget, camUp);
    }
}