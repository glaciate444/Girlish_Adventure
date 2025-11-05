/* =======================================
 * ファイル名 : GoalTrigger.cs
 * 概要 : ゴール処理スクリプト
 * Create Date : 2025/10/24
 * Date : 2025/11/05
 * Version : 0.02
 * 更新内容 : リニューアル
 * ======================================= */
using UnityEngine;
using System.Collections;

public class GoalTrigger : MonoBehaviour {
    [Header("ゴールファンファーレ（BGM扱い）")]
    [SerializeField] private int goalFanfareID = 99; // bgmDBに登録済みのID
    [Header("リザルトシーン名")]
    [SerializeField] private string resultSceneName = "Result";

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other){
        if (isTriggered) return;
        if (!other.CompareTag("Player")) return;

        isTriggered = true;
        StartCoroutine(GoalSequence());
    }

    private IEnumerator GoalSequence(){
        PauseManager.Instance.SetPause(true); // ← 一時停止

        GameManager.Instance.StopBGM();
        GameManager.Instance.PlayBGM(goalFanfareID);

        yield return new WaitForSeconds(3.5f);

        // リザルト画面へ
        GameManager.Instance.GoResult(resultSceneName);
    }
}
