/* =======================================
 * ファイル名 : TitleMenuTweenController.cs
 * 概要 : タイトル画面DoTween連動スクリプト
 * Created Date : 2025/10/27
 * Date : 2025/10/28
 * Version : 0.01
 * 更新内容 : 
 * 備考：DoTween必須
 * ======================================= */
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleMenuTweenController : MonoBehaviour{

    [Header("Panels")]
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private RectTransform gameStartPanel;
    [SerializeField] private RectTransform optionPanel;

    [Header("CanvasGroups")]
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private CanvasGroup gameStartGroup;
    [SerializeField] private CanvasGroup optionGroup;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float slideDistance = 800f;
    [SerializeField] private bool forceHideAtStart = true; // 外部からの上書きを抑止

    [Header("Buttons")]
    [SerializeField] private Button firstMenuButton;
    [SerializeField] private Button firstOptionButton;
    [SerializeField] private Button firstGameButton;
    [SerializeField] private Button menuGameStartButton; // メニュー上の「GameStart」ボタン
    [SerializeField] private Button menuOptionButton;    // メニュー上の「Option」ボタン
    [SerializeField] private Button backButton; // 戻るボタン

    [Header("Cursor")]
    [SerializeField] private TitleCursorController cursorController;

    [Header("ScreenFade")]
    [SerializeField] private ScreenFadeController fadeController;
    [SerializeField] private string nextSceneName = "MainScene"; // 次のシーン名（仮）
    [SerializeField] private Button newGameButton;


    private RectTransform currentPanel;
    private CanvasGroup currentGroup;
    private bool menuOpenedOnce = false;

    private void Awake(){
        // タイトル起動直後はメニュー/各パネルを完全に非表示（初期フレームに写り込まないようAwakeで処理）
        InitPanel(menuPanel, menuGroup, new Vector2(slideDistance, 0), 0f);
        InitPanel(gameStartPanel, gameStartGroup, new Vector2(slideDistance, 0), 0f);
        InitPanel(optionPanel, optionGroup, new Vector2(slideDistance, 0), 0f);

        currentPanel = null;
        currentGroup = null;
    }

    private void OnEnable(){
        if (!forceHideAtStart) return;
        // 念のため再度非表示を強制（他スクリプトの初期化順で上書きされる場合に備える）
        HidePanelHard(menuPanel, menuGroup);
        HidePanelHard(gameStartPanel, gameStartGroup);
        HidePanelHard(optionPanel, optionGroup);
    }

    private void Start(){
        // クリック配線をコードで担保
        if (menuGameStartButton != null){
            menuGameStartButton.onClick.RemoveListener(SwitchToGameStart);
            menuGameStartButton.onClick.AddListener(SwitchToGameStart);
        }
        if (menuOptionButton != null){
            menuOptionButton.onClick.RemoveListener(SwitchToOption);
            menuOptionButton.onClick.AddListener(SwitchToOption);
        }
        if (newGameButton != null){
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGame);
        }
        // 既存設定の後に追加
        if (backButton != null){
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(SwitchToMenu);
        }
    }

    private void Update(){
        // メニュー未オープン中に外部から可視化された場合は即座に戻す保険
        if (!menuOpenedOnce && forceHideAtStart){
            if (gameStartPanel.gameObject.activeSelf || gameStartGroup.alpha > 0f)
                HidePanelHard(gameStartPanel, gameStartGroup);
            if (optionPanel.gameObject.activeSelf || optionGroup.alpha > 0f)
                HidePanelHard(optionPanel, optionGroup);
        }
    }

    private void InitPanel(RectTransform panel, CanvasGroup group, Vector2 pos, float alpha){
        panel.anchoredPosition = pos;
        group.alpha = alpha;
        panel.gameObject.SetActive(alpha > 0);
    }

    public void SwitchToGameStart(){
        SwitchPanel(gameStartPanel, gameStartGroup, firstGameButton);
    }

    public void SwitchToOption(){
        SwitchPanel(optionPanel, optionGroup, firstOptionButton);
    }

    public void SwitchToMenu(){
        SwitchPanel(menuPanel, menuGroup, firstMenuButton);
    }

    private void SwitchPanel(RectTransform nextPanel, CanvasGroup nextGroup, Button firstSelect){
        if (nextPanel == currentPanel) return;

        // 退場アニメ
        Sequence seq = DOTween.Sequence();
        if (currentPanel != null){
            currentGroup.interactable = false;
            currentGroup.blocksRaycasts = false;
            seq.Join(currentPanel.DOAnchorPos(new Vector2(-slideDistance, 0), duration).SetEase(Ease.InCubic));
            seq.Join(currentGroup.DOFade(0f, duration));
        }

        // 入場アニメ
        nextPanel.gameObject.SetActive(true);
        nextPanel.anchoredPosition = new Vector2(slideDistance, 0);
        nextGroup.alpha = 0;
        nextGroup.interactable = false;
        nextGroup.blocksRaycasts = false;

        seq.Append(nextPanel.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutCubic));
        seq.Join(nextGroup.DOFade(1f, duration));
        seq.OnComplete(() =>
        {
            if (currentPanel != null)
                currentPanel.gameObject.SetActive(false);

            currentPanel = nextPanel;
            currentGroup = nextGroup;

            // 必ずここでCanvasGroupを有効化
            currentGroup.interactable = true;
            currentGroup.blocksRaycasts = true;

            // EventSystemへ最初のボタンを設定（未設定・誤設定でもフォールバック）
            var targetFirst = ResolveFirstButton(nextPanel, firstSelect);
            if (targetFirst != null)
                EventSystem.current.SetSelectedGameObject(targetFirst.gameObject);

            // カーソルにアクティブグループを通知
            if (cursorController != null)
                cursorController.SetActiveGroup(currentGroup);
        });
    }
    public void OpenMenuFirstTime(){
        menuPanel.gameObject.SetActive(true);
        menuPanel.anchoredPosition = Vector2.zero;
        menuGroup.alpha = 1f;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;

        currentPanel = menuPanel;
        currentGroup = menuGroup;
        menuOpenedOnce = true;

        var targetFirst = ResolveFirstButton(menuPanel, firstMenuButton);
        if (targetFirst != null)
            EventSystem.current.SetSelectedGameObject(targetFirst.gameObject);

        if (cursorController != null)
            cursorController.SetActiveGroup(menuGroup);
    }

    private Button ResolveFirstButton(RectTransform root, Button assigned){
        if (assigned != null && assigned.gameObject.activeInHierarchy && assigned.interactable)
            return assigned;

        // 子階層から適当なボタンを探す（非アクティブを含む）
        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons){
            if (b.gameObject.activeInHierarchy && b.interactable)
                return b;
        }
        return assigned; // 見つからなければ元の値を返す（null可）
    }

    private void HidePanelHard(RectTransform panel, CanvasGroup group){
        panel.gameObject.SetActive(false);
        panel.anchoredPosition = new Vector2(slideDistance, 0);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
    private void OnNewGame(){
        if (fadeController == null){
            Debug.LogError("FadeController not assigned!");
            return;
        }

        fadeController.FadeOut().OnComplete(() => {
            Debug.Log("NewGame start fade complete!");
            // シーン遷移（仮） - ここでロード処理へ
            SceneManager.LoadScene(nextSceneName);
        });
    }
}
