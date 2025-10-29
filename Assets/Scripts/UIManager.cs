/* =======================================
 * ファイル名 : UIManager.cs
 * 概要 : UIManagerスクリプト
 * Date : 2025/10/10
 * Version : 0.02
 * ======================================= */
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private HPBar hpBar;
    [SerializeField] private SPBar spBar;

    private void Awake(){
        if (canvas == null)
            canvas = GetComponent<Canvas>();
    }

    private void Start(){
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterUI(this);

        InitializeUI();
    }

    private void InitializeUI(){
        if (CoinManager.Instance != null)
            UpdateCoinUI(CoinManager.Instance.GetTotalCoins());

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null){
            UpdateHP(player.hp, player.maxHP);
            UpdateSP(player.sp, player.maxSP);
        }
    }

    private void OnEnable() => AssignCamera();

    private void OnLevelWasLoaded(int level) => AssignCamera();

    public void AssignCamera(){
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera){
            var mainCam = PersistentCameraController.Instance?.GetCamera();
            if (mainCam != null){
                canvas.worldCamera = mainCam;
                Debug.Log($"[UIManager] Canvas に {mainCam.name} を再設定しました");
            }else{
                Debug.LogWarning("[UIManager] MainCameraが見つかりません。PersistentCameraControllerを確認してください。");
            }
        }
    }

    // ===== 外部更新メソッド =====

    public void UpdateCoinUI(int total){
        if (coinText != null)
            coinText.text = $"× {total}";
    }

    public void UpdateHP(int currentHP, int maxHP){
        hpBar?.SetHP(currentHP, maxHP);
    }

    public void UpdateSP(int currentSP, int maxSP){
        spBar?.SetSP(currentSP, maxSP);
    }

    public void ShowClearUI(){
        Debug.Log("ステージクリアUIを表示（後で演出追加）");
    }

    private void OnDestroy(){
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterUI();
    }
}
