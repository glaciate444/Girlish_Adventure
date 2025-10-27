using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleMenuFadeController : MonoBehaviour{
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private CanvasGroup gameStartPanel;
    [SerializeField] private CanvasGroup optionPanel;
    [SerializeField] private Button firstMenuButton;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup currentPanel;

    private void Start(){
        mainPanel.alpha = 1;
        menuPanel.alpha = 0;
        gameStartPanel.alpha = 0;
        optionPanel.alpha = 0;

        currentPanel = mainPanel;

        menuPanel.gameObject.SetActive(false);
        gameStartPanel.gameObject.SetActive(false);
        optionPanel.gameObject.SetActive(false);
    }

    public void OpenMenu(){
        StartCoroutine(SwitchPanel(mainPanel, menuPanel, firstMenuButton));
    }

    public void OnGameStartSelected(){
        StartCoroutine(SwitchPanel(menuPanel, gameStartPanel));
    }

    public void OnOptionSelected(){
        StartCoroutine(SwitchPanel(menuPanel, optionPanel));
    }

    public void OnBackToMenu(){
        StartCoroutine(SwitchPanel(optionPanel, menuPanel, firstMenuButton));
    }

    private IEnumerator SwitchPanel(CanvasGroup from, CanvasGroup to, Button selectAfter = null){
        if (currentPanel == to) yield break;

        to.gameObject.SetActive(true);
        to.interactable = false;
        to.blocksRaycasts = false;

        // フェードアウト
        float t = 0f;
        while (t < fadeDuration){
            t += Time.deltaTime;
            float a = 1 - (t / fadeDuration);
            from.alpha = a;
            yield return null;
        }
        from.alpha = 0;
        from.gameObject.SetActive(false);

        // フェードイン
        t = 0f;
        while (t < fadeDuration){
            t += Time.deltaTime;
            float a = t / fadeDuration;
            to.alpha = a;
            yield return null;
        }
        to.alpha = 1;
        to.interactable = true;
        to.blocksRaycasts = true;

        currentPanel = to;

        if (selectAfter != null)
            EventSystem.current.SetSelectedGameObject(selectAfter.gameObject);
    }
}