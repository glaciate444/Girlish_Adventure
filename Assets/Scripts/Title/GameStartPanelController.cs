using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GameStartPanelController : MonoBehaviour {
    [Header("Groups")]
    [SerializeField] private CanvasGroup slotGroup;      // セーブスロット一覧
    [SerializeField] private CanvasGroup confirmGroup;   // 確認ウィンドウ
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Buttons")]
    [SerializeField] private Button firstSlotButton;
    [SerializeField] private Button firstConfirmButton;
    [SerializeField] private Button cancelConfirmButton;

    private CanvasGroup currentGroup;

    private void Start(){
        InitGroup(slotGroup, true);
        InitGroup(confirmGroup, false);

        currentGroup = slotGroup;

        // 確認ウィンドウのキャンセルボタン
        if (cancelConfirmButton != null){
            cancelConfirmButton.onClick.RemoveAllListeners();
            cancelConfirmButton.onClick.AddListener(CloseConfirm);
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