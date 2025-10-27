using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleMenuTweenController : MonoBehaviour
{
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

    [Header("Buttons")]
    [SerializeField] private Button firstMenuButton;
    [SerializeField] private Button firstOptionButton;
    [SerializeField] private Button firstGameButton;
    [SerializeField] private Button menuGameStartButton; // メニュー上の「GameStart」ボタン
    [SerializeField] private Button menuOptionButton;    // メニュー上の「Option」ボタン

    private RectTransform currentPanel;
    private CanvasGroup currentGroup;

    private void Start(){
        // タイトル起動直後はメニューを表示しない（AnyKey後に表示）
        InitPanel(menuPanel, menuGroup, new Vector2(slideDistance, 0), 0f);
        InitPanel(gameStartPanel, gameStartGroup, new Vector2(slideDistance, 0), 0f);
        InitPanel(optionPanel, optionGroup, new Vector2(slideDistance, 0), 0f);

        currentPanel = null;
        currentGroup = null;

        // クリック配線をコードで担保
        if (menuGameStartButton != null){
            menuGameStartButton.onClick.RemoveListener(SwitchToGameStart);
            menuGameStartButton.onClick.AddListener(SwitchToGameStart);
        }
        if (menuOptionButton != null){
            menuOptionButton.onClick.RemoveListener(SwitchToOption);
            menuOptionButton.onClick.AddListener(SwitchToOption);
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
        seq.OnComplete(() => {
            if (currentPanel != null) currentPanel.gameObject.SetActive(false);
            currentPanel = nextPanel;
            currentGroup = nextGroup;
            currentGroup.interactable = true;
            currentGroup.blocksRaycasts = true;
            EventSystem.current.SetSelectedGameObject(firstSelect.gameObject);
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

        EventSystem.current.SetSelectedGameObject(firstMenuButton.gameObject);
    }
}
