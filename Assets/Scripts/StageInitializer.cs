/* =======================================
 * ファイル名 : StageInitializer.cs
 * 概要 : ステージ初期設定スクリプト
 * Date : 2025/10/29
 * Version : 0.01
 * ======================================= */
using UnityEngine;

public class StageInitializer : MonoBehaviour{
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private int bgmID = -1;

    private void Start(){
        if (uiManagerPrefab != null){
            var uiInstance = Instantiate(uiManagerPrefab);
            var ui = uiInstance.GetComponent<UIManager>();
            if (ui != null){
                GameManager.Instance.RegisterUI(ui);
                ui.AssignCamera();
            }
        }

        int id = bgmID >= 0 ? bgmID : GameManager.Instance.CurrentStage;
        GameManager.Instance.PlayBGM(id);
    }

    private void OnDestroy(){
        GameManager.Instance?.UnregisterUI();
    }
}
