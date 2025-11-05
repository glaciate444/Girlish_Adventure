/* =======================================
 * ファイル名 : PauseManager.cs
 * 概要 : ポーズ管理スクリプト
 * Create Date : 2025/11/05
 * Date : 2025/11/05
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

public class PauseManager : MonoBehaviour {
    public static PauseManager Instance { get; private set; }

    private bool isPaused = false;

    private void Awake(){
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetPause(bool pause){
        if (isPaused == pause) return;
        isPaused = pause;

        // プレイヤー停止
        var player = FindObjectOfType<PlayerController>();
        if (player != null){
            player.enabled = !pause;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null){
                rb.linearVelocity = Vector2.zero;
                rb.constraints = pause
                    ? RigidbodyConstraints2D.FreezeAll
                    : RigidbodyConstraints2D.FreezeRotation;
            }
        }

        // 敵停止（BaseEnemy 継承クラスをすべて）
        var enemies = FindObjectsOfType<BaseEnemy>();
        foreach (var e in enemies){
            e.enabled = !pause;

            var rb = e.GetComponent<Rigidbody2D>();
            if (rb != null){
                rb.linearVelocity = Vector2.zero;
                rb.constraints = pause
                    ? RigidbodyConstraints2D.FreezeAll
                    : RigidbodyConstraints2D.FreezeRotation;
            }
        }

        // 入力システム停止（例：PlayerInputなど）
        var inputs = FindObjectsOfType<UnityEngine.InputSystem.PlayerInput>();
        foreach (var input in inputs){
            input.enabled = !pause;
        }
    }

    public bool IsPaused => isPaused;
}
