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
        StartCoroutine(InitRoutine());
    }

    private System.Collections.IEnumerator InitRoutine(){
        if (uiManagerPrefab == null){
            Debug.LogError("UIManager Prefab が設定されていません。");
            yield break;
        }

        // 親指定なしで完全なルートとして生成
        GameObject instance = Instantiate(uiManagerPrefab);
        var ui = instance.GetComponent<UIManager>();
        if (ui == null){
            Debug.LogError("UIManager スクリプトがPrefabのルートにありません！");
            yield break;
        }

        // GameManager が用意できるまで待つ（シーン初期化順の差異に対応）
        while (GameManager.Instance == null)
            yield return null;

        GameManager.Instance.RegisterUI(ui);
        Debug.Log($"✅ {instance.name} 生成完了");

        // 既存の値をUIへ初期同期
        var coinMgr = CoinManager.Instance;
        if (coinMgr != null)
            ui.UpdateCoinUI(coinMgr.GetTotalCoins());

        // プレイヤーの HP/SP も初期同期
        var player = FindObjectOfType<PlayerController>();
        if (player != null){
            ui.UpdateHP(player.GetHP(), player.maxHP);
            ui.UpdateSP(player.sp, player.maxSP);
        }

        int id = bgmID >= 0 ? bgmID : GameManager.Instance.CurrentStage;
        GameManager.Instance.PlayBGM(id);
    }

    private void OnDestroy(){
        GameManager.Instance?.UnregisterUI();
    }
}
