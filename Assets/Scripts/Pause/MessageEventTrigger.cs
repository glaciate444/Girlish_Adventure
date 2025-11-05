/* =======================================
 * ファイル名 : MessageEventTrigger.cs
 * 概要 : メッセージイベントスクリプト
 * Create Date : 2025/11/05
 * Date : 2025/11/05
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;
using System.Collections;

public class MessageEventTrigger : MonoBehaviour {
    [TextArea]
    [SerializeField] private string messageText;
    [SerializeField] private bool destroyAfterRead = true;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other){
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence(){
        // ゲーム一時停止
        PauseManager.Instance.SetPause(true);

        // メッセージ表示
        UIManager.Instance.ShowMessage(messageText);

        // UIが閉じるまで待機
        yield return new WaitUntil(() => !UIManager.Instance.IsMessageOpen);

        // ゲーム再開
        PauseManager.Instance.SetPause(false);

        if (destroyAfterRead)
            Destroy(gameObject);
    }
}

