/* =======================================
 * ファイル名 : TitleManager.cs
 * 概要 : タイトル画面スクリプト
 * Created Date : 2025/10/24
 * Date : 2025/10/28
 * Version : 0.02
 * ======================================= */
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class TitleManager : MonoBehaviour{
    [SerializeField] private TitleMenuTweenController menuController;
    [SerializeField] private InputActionReference anyKeyAction;
    [SerializeField] private CanvasGroup titleLogoPanel; // ロゴ（メイン）パネル
    [SerializeField] private float fadeDuration = 0.5f;

    private bool menuOpened = false;

    private void OnEnable(){
        if (anyKeyAction != null){
            anyKeyAction.action.performed += OnAnyKey;
            anyKeyAction.action.Enable();
        }
    }

    private void OnDisable(){
        if (anyKeyAction != null){
            anyKeyAction.action.performed -= OnAnyKey;
            anyKeyAction.action.Disable();
        }
    }

    private void OnAnyKey(InputAction.CallbackContext ctx){
        Debug.Log("AnyKey pressed!");
        if (menuOpened) return;
        menuOpened = true;

        // タイトル（ロゴ）をフェードアウトしてからメニューを初回表示
        if (titleLogoPanel != null){
            DOTween.Kill(titleLogoPanel);
            titleLogoPanel.interactable = false;
            titleLogoPanel.blocksRaycasts = false;
            titleLogoPanel.DOFade(0f, fadeDuration).OnComplete(() => {
                titleLogoPanel.gameObject.SetActive(false);
                menuController.OpenMenuFirstTime();
            });
        } else {
            menuController.OpenMenuFirstTime();
        }
    }
}
/*-------------------------------
 * 2025/10/28 メモ
 * 原因は割り当てミスです。今「Panels → Game Start Panel」にGameStart(Button)のRectTransformが入っています。
 * Start時にここを「スライド距離＋alpha=0、SetActive(false)」にするため、メニュー内のボタン自体が起動直後に非表示化されていました。
 * その結果、メニューを出してもOptionだけが残ります。
 * 
 * やること
 * InspectorのTitleMenuTweenControllerで次のように割り当て直してください。
 * Panels
 * ・Menu Panel: GameMenuPanel（2つのボタンを内包する親）
 * ・Game Start Panel: GameStartPanel（ボタンではなく、別画面用のパネル）
 * ・Option Panel: OptionPanel
 * ・CanvasGroups
 * ・Menu Group: GameMenuPanelのCanvasGroup
 * ・Game Start Group: GameStartPanelのCanvasGroup
 * ・Option Group: OptionPanelのCanvasGroup
 *
 * Buttons
 * ・First Menu Button: GameStart(Button)（メニュー初期選択）
 * ・First Game Button: GameStartPanel側の最初に選択させたいボタン（あるなら）
 * ・First Option Button: OptionPanel側の最初のボタン
 * ・Menu Game Start Button: GameStart(Button)
 * ・Menu Option Button: Option(Button)
 * 
 * 確認
 * ・再生→任意キー後に「GameStart」「Option」が両方見えます。
 * 「GameStart」を押すとGameStartPanelへ切り替わります。
 * ・同様の症状が再発する場合は、Game Start Panelに「ボタンではない別のパネル」を入れているかもう一度だけ確認してください。
---------------------------*/
