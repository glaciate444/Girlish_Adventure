using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour {
    [SerializeField] private Image fadeImage; // 黒いImageをアタッチ
    [SerializeField] private float fadeDuration = 1.0f;

    private void Awake(){
        // 起動時にフェードイン開始
        if (fadeImage != null){
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeIn(){
        float t = fadeDuration;
        Color c = fadeImage.color;
        while (t > 0f){
            t -= Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false); // 完了後は非表示
    }

    public IEnumerator FadeOut(System.Action onComplete = null){
        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        onComplete?.Invoke();
    }
}
/*
 * 🔧 セットアップ手順
- Canvas を作成し、全画面サイズの 黒い Image を配置
- RectTransform を Stretch にして画面全体を覆う
- Color の Alpha を 1 にしておく
- 上記スクリプトを空の GameObject にアタッチ
- fadeImage に黒い Image をドラッグ＆ドロップ
-
💡 使い方
- シーン開始時に自動でフェードイン（Awake 内で呼んでいる）
- 他スクリプトから呼び出す場合：
- // フェードアウトしてからシーン遷移
- StartCoroutine(fadeController.FadeOut(() => SceneManager.LoadScene("NextScene")));
*/