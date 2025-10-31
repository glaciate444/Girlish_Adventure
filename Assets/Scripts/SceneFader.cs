/* =======================================
 * �t�@�C���� : SceneFader.cs
 * �T�v : �V�[���̃t�F�[�h�X�N���v�g
 * Created Date : 2025/10/29
 * Date : 2025/10/29
 * Version : 0.01
 * �X�V���e : �V�K�쐬
 * ======================================= */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour {
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;

    private void Awake(){
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();
    }

    public IEnumerator FadeOut(){
        float time = 0f;
        Color color = fadeImage.color;
        while (time < fadeDuration)
        {
            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            fadeImage.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;
    }

    public IEnumerator FadeIn(){
        float time = 0f;
        Color color = fadeImage.color;
        while (time < fadeDuration)
        {
            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            fadeImage.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = 0f;
        fadeImage.color = color;
    }
}
