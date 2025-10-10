using UnityEngine;
using UnityEngine.Rendering;

public class CameraChecker : MonoBehaviour{
    private enum Mode{
        None,
        Render,
        RenderOut,
    }
    private Mode mode;
    private Camera currentCamera;
    void Start(){
        mode = Mode.None;
    }
    private void Update(){
        Dead();
    }
    void OnEnable(){
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable(){
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera){
        currentCamera = camera;
    }

    private void OnWillRenderObject(){
        if ((currentCamera == null) || (currentCamera.cullingMask & (1 << gameObject.layer)) == 0){
            return;
        }

        if (currentCamera.name == "Main Camera"){
            mode = Mode.Render;
        }
    }

    private void Dead(){
        if(mode == Mode.RenderOut){
            Destroy(gameObject);
        }
        if (mode == Mode.Render){
            mode = Mode.RenderOut;
        }
    
    }
}
