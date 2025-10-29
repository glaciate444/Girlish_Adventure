using UnityEngine;

public class GoalTrigger : MonoBehaviour {
    [SerializeField] private GameObject fadePrefab;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other){
        if (isTriggered) return;
        if (!other.CompareTag("Player")) return;

        isTriggered = true;
        StartCoroutine(GoalSequence());
    }

    private System.Collections.IEnumerator GoalSequence(){
        // フェードCanvas生成
        var fadeObj = Instantiate(fadePrefab);
        var fader = fadeObj.GetComponent<SceneFader>();

        // BGMフェードアウト開始
        StartCoroutine(FadeOutBGM(1.0f));

        // 画面フェードアウト
        yield return fader.FadeOut();

        // クリアUI表示
        GameManager.Instance.StopBGM();
        if (GameManager.Instance != null && GameManager.Instance.Sound != null)
            GameManager.Instance.PlaySE(0); // クリアSEなど

        // シーン遷移（WorldMapへ）
        GameManager.Instance.ReturnToWorldMap();
    }

    private System.Collections.IEnumerator FadeOutBGM(float duration){
        var sound = GameManager.Instance.Sound;
        var bgmSource = sound != null ? sound.GetComponentInChildren<AudioSource>() : null;
        if (bgmSource == null) yield break;

        float startVol = bgmSource.volume;
        float time = 0f;

        while (time < duration){
            bgmSource.volume = Mathf.Lerp(startVol, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        bgmSource.volume = 0f;
        sound.StopBGM();
        bgmSource.volume = startVol; // 戻す（次曲用）
    }
}
