using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour{
    private void Start(){
        var lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        Light2D persistentGlobal = null;
        Light2D stageGlobal = null;

        foreach (var l in lights){
            if (l.lightType == Light2D.LightType.Global){
                if (persistentGlobal == null) persistentGlobal = l;
                else stageGlobal = l;
            }
        }

        if (persistentGlobal != null && stageGlobal != null){
            // ステージ側Global Lightを優先
            Debug.Log("[LightManager] PersistentのGlobal Lightを無効化しました（ステージ優先）。");
            persistentGlobal.enabled = false;
            stageGlobal.enabled = true;
        }
    }
}
