using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PersistentCameraController : MonoBehaviour{
    public static PersistentCameraController Instance { get; private set; }

    [SerializeField] private GameObject mainCameraPrefab; // MainCamera��Prefab
    private Camera mainCam;

    private void Awake(){
        if (Instance != null){
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupCamera();
    }

    private void SetupCamera(){
        mainCam = FindAnyObjectByType<Camera>();
        if (mainCam == null){
            if (mainCameraPrefab != null){
                var camObj = Instantiate(mainCameraPrefab);
                camObj.name = "Main Camera (Auto)";
                mainCam = camObj.GetComponent<Camera>();
                Debug.Log("[CameraController] MainCamera�������������܂����B");
            }else{
                Debug.LogError("[CameraController] MainCameraPrefab���ݒ肳��Ă��܂���I");
            }
        }
    }
    public Camera GetCamera() => mainCam;
}