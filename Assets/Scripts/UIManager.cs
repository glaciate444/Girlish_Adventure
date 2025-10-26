using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private HPBar hpBar;
    [SerializeField] private SPBar spBar;

    private void Awake(){
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject); // Canvas全体を保持
        }else if (Instance != this){
            Debug.LogWarning("重複したUIManagerが検出されたため破棄");
            Destroy(gameObject);
            return;
        }
        if(canvas == null)
            canvas = GetComponent<Canvas>();
    }

    private void Start(){
        // 起動時のUI初期化
        if (CoinManager.Instance != null)
            UpdateCoinUI(CoinManager.Instance.GetTotalCoins());
        
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            UpdateHP(player.hp, player.maxHP);
            UpdateSP(player.sp, player.maxSP);
        
    }

    private void OnEnable() => AssignCamera();

    private void OnLevelWasLoaded(int level) => AssignCamera();

    private void AssignCamera(){
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera){
            var mainCam = Camera.main;
            if (mainCam != null){
                canvas.worldCamera = mainCam;
                Debug.Log($"Canvas に {mainCam.name} を再設定しました");
            }
        }
    }
    public void ShowClearUI(){
        
    
    }


    // ======= 外部から呼び出すUI更新メソッド =======

    public void UpdateCoinUI(int total){
        if (coinText != null)
            coinText.text = $"× {total}";
        else
            Debug.LogWarning("CoinText が設定されていません！（Prefab内でドラッグしてください）");
    }

    public void UpdateHP(int currentHP, int maxHP){
        if (hpBar != null)
            hpBar.SetHP(currentHP, maxHP);
        else
            Debug.LogWarning("HPBar が設定されていません！（Prefab内でドラッグしてください）");
    }
    public void UpdateSP(int currentSP, int maxSP){
        if (hpBar != null)
            spBar.SetSP(currentSP, maxSP);
        else
            Debug.LogWarning("SPBar が設定されていません！（Prefab内でドラッグしてください）");
    }

}
