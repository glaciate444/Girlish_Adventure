/* =======================================
 * ファイル名 : ScreenFadeController.cs
 * 概要 : スクリーンフェードスクリプト
 * Created Date : 2025/10/28
 * Date : 2025/10/28
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using DG.Tweening;

public class ScreenFadeController : MonoBehaviour {
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    public Tween FadeOut(){
        fadeGroup.blocksRaycasts = true;
        return fadeGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
    }

    public Tween FadeIn(){
        fadeGroup.blocksRaycasts = false;
        return fadeGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad);
    }

    private void Awake(){
        if (fadeGroup != null){
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }
}