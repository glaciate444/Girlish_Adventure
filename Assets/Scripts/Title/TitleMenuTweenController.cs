/* =======================================
 * ファイル名 : TitleMenuTweenController.cs
 * 概要 : タイトル画面DoTween連動スクリプト
 * Created Date : 2025/10/27
 * Date : 2025/10/28
 * Version : 0.01
 * 更新内容 : 
 * 備考：DoTween必須
 * TitleMenuTweenController = 遷移司令塔
 * GameStartPanelController / OptionPanelController = 部隊リーダー（各UI制御）
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
    [SerializeField] private Button newGameButton;       // 「NewGame」ボタン（フェード→シーン切替）

    [Header("Cursor")]
    [SerializeField] private TitleCursorController cursorController;

    [Header("ScreenFade")]
    [SerializeField] private ScreenFadeController fadeController;
    [SerializeField] private string nextSceneName = "MainScene"; // 次のシーン名（仮）

    private RectTransform currentPanel;
    private CanvasGroup currentGroup;
    private bool menuOpenedOnce = false;

    private void Awake(){
        // 起動時は全パネル非表示
        HidePanelHard(menuPanel, menuGroup);
        HidePanelHard(gameStartPanel, gameStartGroup);
        HidePanelHard(optionPanel, optionGroup);
    }

    private void OnEnable(){
        // ボタン登録
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
    }

    private void Update(){
        // 保険：外部操作で誤って開かれたパネルを閉じる
        if (!menuOpenedOnce && forceHideAtStart){
            if (gameStartPanel.gameObject.activeSelf || gameStartGroup.alpha > 0f)
                HidePanelHard(gameStartPanel, gameStartGroup);
            if (optionPanel.gameObject.activeSelf || optionGroup.alpha > 0f)
                HidePanelHard(optionPanel, optionGroup);
        }
    }

    //============================
    // パネル切替処理
    //============================

    public void SwitchToGameStart(){
        SwitchPanel(gameStartPanel, gameStartGroup, firstGameButton);
    }

    public void SwitchToOption(){
        SwitchPanel(optionPanel, optionGroup, firstOptionButton);
    }

    public void SwitchToMenu(){
        Debug.Log("[TitleMenu] SwitchToMenu called");
        if (currentGroup != null){
            currentGroup.interactable = false;
            currentGroup.blocksRaycasts = false;
        }

        SwitchPanel(menuPanel, menuGroup, firstMenuButton);
    }

    private void SwitchPanel(RectTransform nextPanel, CanvasGroup nextGroup, Button firstSelect){
        if (nextPanel == currentPanel) return;

        Sequence seq = DOTween.Sequence();

        // 退場アニメ
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
        seq.OnComplete(() => {
            if (currentPanel != null)
                currentPanel.gameObject.SetActive(false);

            currentPanel = nextPanel;
            currentGroup = nextGroup;

            currentGroup.interactable = true;
            currentGroup.blocksRaycasts = true;

            var targetFirst = ResolveFirstButton(nextPanel, firstSelect);
            if (targetFirst != null)
                EventSystem.current.SetSelectedGameObject(targetFirst.gameObject);

            if (cursorController != null)
                cursorController.SetActiveGroup(currentGroup);
        });
    }

    //============================
    // 初回メニュー表示
    //============================

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

    //============================
    // ヘルパー関数
    //============================

    private Button ResolveFirstButton(RectTransform root, Button assigned){
        if (assigned != null && assigned.gameObject.activeInHierarchy && assigned.interactable)
            return assigned;

        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons){
            if (b.gameObject.activeInHierarchy && b.interactable)
                return b;
        }
        return assigned;
    }

    private void HidePanelHard(RectTransform panel, CanvasGroup group){
        panel.gameObject.SetActive(false);
        panel.anchoredPosition = new Vector2(slideDistance, 0);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    //============================
    // New Game フェード→シーン切替
    //============================

    private void OnNewGame(){
        if (fadeController == null){
            Debug.LogError("FadeController not assigned!");
            return;
        }

        fadeController.FadeOut().OnComplete(() => {
            Debug.Log("NewGame start fade complete!");
            SceneManager.LoadScene(nextSceneName);
        });
    }
}