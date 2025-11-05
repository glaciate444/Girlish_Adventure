/* =======================================
 * ファイル名 : UIManager.cs
 * 概要 : UIManagerスクリプト
 * Created Date : 2025/10/10
 * Date : 2025/10/31
 * Version : 0.03
 * 更新内容 : シンプル化（表示と破棄防止のみ）
 * ======================================= */
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private HPBar hpBar;
    [SerializeField] private SPBar spBar;
    [Header("メッセージウィンドウ")]
    [SerializeField] private GameObject messageWindow;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button nextButton;

    private void Awake(){
        // シングルトン処理
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else if (Instance != this){
            // 既存のUIManagerが存在する場合、このインスタンスは重複
            // 非アクティブ化も破棄もしない（シーンに直接配置された場合はそのまま維持）
            Debug.LogWarning($"[UIManager] 既にUIManagerが存在します。既存: {Instance.name}, 新規: {name}");
            // 何もしない（このインスタンスはそのまま維持）
            return;
        }else{
            DontDestroyOnLoad(gameObject);
        }

        // 参照の自動割り当て（nullの場合のみ）
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);
        if (coinText == null)
            coinText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (hpBar == null)
            hpBar = GetComponentInChildren<HPBar>(true);
        if (spBar == null)
            spBar = GetComponentInChildren<SPBar>(true);

        // シーンロードイベント登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start(){
        // 表示を確保
        gameObject.SetActive(true);
        if (canvas != null){
            canvas.gameObject.SetActive(true);
            canvas.enabled = true;
        }
        
        // GameManagerに登録
        if (GameManager.Instance != null){
            GameManager.Instance.RegisterUI(this);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        // 表示を確保
        gameObject.SetActive(true);
        if (canvas != null){
            canvas.gameObject.SetActive(true);
            canvas.enabled = true;
        }
        
        // カメラの割り当て
        AssignCamera();
    }

    private void OnEnable() => AssignCamera();

    public void AssignCamera(){
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera){
            var mainCam = PersistentCameraController.Instance?.GetCamera();
            if (mainCam != null){
                canvas.worldCamera = mainCam;
            }
        }
    }

    // ===== UI更新メソッド =====

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

    // ===== メッセージウィンドウ =====

    public void ShowMessage(string text){
        if (messageWindow == null || messageText == null) return;

        messageWindow.SetActive(true);
        messageText.text = text;

        if (nextButton != null){
            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => HideMessage());
        }
    }

    public void HideMessage(){
        if (messageWindow == null) return;
        messageWindow.SetActive(false);
    }

    public bool IsMessageOpen => messageWindow != null && messageWindow.activeSelf;

    // ===== 破棄防止 =====

    public void PreventDestroy(){
        if (Instance == this && gameObject != null){
            gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy(){
        if (Instance == this){
            Debug.LogError("[UIManager] ⚠️ UIManager.Instanceが破棄されようとしています！");
        }else{
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
