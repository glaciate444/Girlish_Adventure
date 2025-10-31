/* =======================================
 * ファイル名 : UIManager.cs
 * 概要 : UIManagerスクリプト
 * Created Date : 2025/10/10
 * Date : 2025/10/31
 * Version : 0.02
 * 更新内容 : プレハブ生成時の不具合修正
 * ======================================= */
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private HPBar hpBar;
    [SerializeField] private SPBar spBar;

    private void Awake(){
        // まず親（自分）から取得し、無ければ子を探索する
        if (canvas == null)
            canvas = GetComponent<Canvas>() ?? GetComponentInChildren<Canvas>(true);

        if (coinText == null)
            coinText = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>(true);
        if (hpBar == null)
            hpBar = GetComponent<HPBar>() ?? GetComponentInChildren<HPBar>(true);
        if (spBar == null)
            spBar = GetComponent<SPBar>() ?? GetComponentInChildren<SPBar>(true);

        // シーンロードイベント登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start(){
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterUI(this);

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        AssignCamera();
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
