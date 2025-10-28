using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class OptionPanelController : MonoBehaviour{
    [Header("Groups")]
    [SerializeField] private CanvasGroup optionGroup;  // このパネル自体
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button firstOptionButton; // 最初に選択されるボタン（例：音量設定など）

    [Header("Controller Link")]
    [SerializeField] private TitleMenuTweenController menuController;

    private void Start(){
        // 起動時は確実にフェードアウト状態
        InitGroup(optionGroup, true);
    }

    private void OnEnable(){
        // BackButton を TitleMenuTweenController に登録
        if (backButton != null && menuController != null){
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(menuController.SwitchToMenu);
            Debug.Log("[OptionPanelController] BackButton listener set (OnEnable)");
        }else{
            Debug.LogWarning("[OptionPanelController] BackButton or MenuController not assigned!");
        }

        // 初期選択ボタンを指定（UIナビゲーション対策）
        if (firstOptionButton != null)
            EventSystem.current.SetSelectedGameObject(firstOptionButton.gameObject);
    }

    private void InitGroup(CanvasGroup group, bool active){
        group.alpha = active ? 1 : 0;
        group.interactable = active;
        group.blocksRaycasts = active;
        group.gameObject.SetActive(active);
    }

    // 🔽 フェード演出を入れたい場合（任意）
    public void FadeIn(){
        optionGroup.gameObject.SetActive(true);
        optionGroup.DOFade(1f, fadeDuration);
    }

    public void FadeOut(){
        optionGroup.DOFade(0f, fadeDuration).OnComplete(() => optionGroup.gameObject.SetActive(false));
    }
}
/*
 * 🎮 Unityでの設定手順
 * Hierarchyで OptionPanel に OptionPanelController をアタッチ。
 * Inspectorで以下を割り当て：
 * OptionGroup → OptionPanel の CanvasGroup
 * BackButton → 「戻る」ボタン（作ったら）
 * FirstOptionButton → 最初にフォーカスしたいボタン（例：音量設定など）
 * MenuController → TitleMenuTweenController をドラッグアンドドロップ
 * 
 * 💡 補足
 * この設計により、
 * GameStartPanelController → 「Back → SwitchToMenu()」
 * OptionPanelController → 「Back → SwitchToMenu()」
 * という共通挙動が完全に一貫します。
 * TitleMenuTweenController側は一切修正不要です。
 * （BackButtonのリスナー管理が各Panelに委譲されているため競合もしません。）
 * 
 */