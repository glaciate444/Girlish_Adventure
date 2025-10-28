using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GameStartPanelController : MonoBehaviour{
    [Header("Groups")]
    [SerializeField] private CanvasGroup slotGroup;      // セーブスロット一覧
    [SerializeField] private CanvasGroup confirmGroup;   // 確認ウィンドウ
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Buttons")]
    [SerializeField] private Button firstSlotButton;
    [SerializeField] private Button firstConfirmButton;
    [SerializeField] private Button cancelConfirmButton;

    // 🔽 ここを追加
    [Header("Back")]
    [SerializeField] private Button backButton;                          // ← GameStartPanel内のBackボタン
    [SerializeField] private TitleMenuTweenController menuController;    // ← TitleManagerをドラッグ設定

    private CanvasGroup currentGroup;

    private void Start(){
        InitGroup(slotGroup, true);
        InitGroup(confirmGroup, false);

        currentGroup = slotGroup;
    }

    // 🔽 ここを新しく追加
    private void OnEnable(){
        // CancelConfirm は必ず confirm の "No" ボタンを割り当てる（Back と混同しない）
        if (cancelConfirmButton != null){
            if (cancelConfirmButton == backButton){
                Debug.LogError("[GameStartPanelController] cancelConfirmButton is BackButton. Assign the NO button in confirm dialog.");
                // Back の動作は維持したいので、ここではリスナーを消さない
            }else{
                cancelConfirmButton.onClick.RemoveAllListeners();
                cancelConfirmButton.onClick.AddListener(CloseConfirm);
            }
        }

        // GameStartPanel がアクティブになった瞬間に BackButton 登録（最終的に必ず上書き保証）
        if (backButton != null && menuController != null){
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(menuController.SwitchToMenu);
            Debug.Log("[GameStartPanelController] BackButton listener set (OnEnable)");
        }else{
            Debug.LogWarning("[GameStartPanelController] BackButton or MenuController not assigned!");
        }
    }

    private void InitGroup(CanvasGroup group, bool active){
        group.alpha = active ? 1 : 0;
        group.interactable = active;
        group.blocksRaycasts = active;
        group.gameObject.SetActive(active);
    }

    public void OpenConfirmPanel(){
        if (confirmGroup == null || slotGroup == null) return;

        Sequence seq = DOTween.Sequence();
        seq.Join(slotGroup.DOFade(0f, fadeDuration))
           .Join(confirmGroup.DOFade(1f, fadeDuration))
           .OnStart(() => confirmGroup.gameObject.SetActive(true))
           .OnComplete(() =>
           {
               slotGroup.interactable = false;
               slotGroup.blocksRaycasts = false;

               confirmGroup.interactable = true;
               confirmGroup.blocksRaycasts = true;
               EventSystem.current.SetSelectedGameObject(firstConfirmButton.gameObject);
               currentGroup = confirmGroup;
           });
    }

    public void CloseConfirm(){
        if (confirmGroup == null || slotGroup == null) return;
        if (!confirmGroup.gameObject.activeSelf) return; // 非表示なら何もしない（Backと干渉防止）

        Sequence seq = DOTween.Sequence();
        seq.Join(confirmGroup.DOFade(0f, fadeDuration))
           .Join(slotGroup.DOFade(1f, fadeDuration))
           .OnStart(() => slotGroup.gameObject.SetActive(true))
           .OnComplete(() =>
           {
               confirmGroup.interactable = false;
               confirmGroup.blocksRaycasts = false;

               slotGroup.interactable = true;
               slotGroup.blocksRaycasts = true;
               confirmGroup.gameObject.SetActive(false);

               EventSystem.current.SetSelectedGameObject(firstSlotButton.gameObject);
               currentGroup = slotGroup;
           });
    }
}
/* ==============================
 * 💡 使い方
 * 
 * slotGroup に「SaveData01〜04 ボタンを持つパネル」を指定
 * confirmGroup に「ロード確認ウィンドウUI（Yes/No）」を指定
 * firstSlotButton は最初に選択されるセーブスロット
 * firstConfirmButton は確認画面の「Yes」ボタン
 * cancelConfirmButton は「No」ボタン
 * 
 * セーブスロットボタンにはクリックイベントとして
 * GameStartPanelController.OpenConfirmPanel()
 * をアサインしておけばOKです。
 * これで「スロットを選択 → 確認ウィンドウへ」アニメーションが動きます。
 * Yes 押下時に ScreenFadeController.FadeOut() → SceneManager.LoadScene() を呼べば
 * ロード確認 → ゲーム開始 の流れが完成します。
 ================================= */