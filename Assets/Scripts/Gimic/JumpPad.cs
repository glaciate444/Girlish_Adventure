/* =======================================
 * ファイル名 : JumpPad.cs
 * 概要 : ジャンプ台スクリプト
 * Created Date : 2025/10/31
 * Date : 2025/10/31
 * Version : 0.02
 * 更新内容 : 横移動を可能に
 * ======================================= */
using UnityEngine;
using System.Collections;

public class JumpPad : MonoBehaviour{
    [SerializeField] private float bounceHeight = 3f;
    [SerializeField] private float bounceDuration = 0.15f;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private float spriteResetTime = 0.2f;

    private SpriteRenderer sr;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    private void OnTriggerEnter2D(Collider2D other){
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        // プレイヤーコントローラへ通知（横移動はプレイヤー側で維持）
        var player = other.GetComponent<PlayerController>();
        if (player != null){
            // 連続発火の防止: バネ作用中は再度起動しない
            if (player.IsOnJumpPad) return;
            player.ActivateJumpPad(bounceHeight, bounceDuration);
        }

        if (sr != null && pressedSprite != null){
            sr.sprite = pressedSprite;
            CancelInvoke(nameof(ResetSprite));
            Invoke(nameof(ResetSprite), spriteResetTime);
        }

        // 位置を直接上書きしない（横移動を殺さないため）
    }


    private void ResetSprite(){
        if (sr != null && normalSprite != null)
            sr.sprite = normalSprite;
    }
}
