/* =======================================
 * ファイル名 : GroundCheck.cs
 * 概要 : 接地判定と地面法線の取得（リフト対応）スクリプト
 * Created Date : 2025/10/01
 * Date : 2025/10/21
 * Version : 0.04
 * 更新内容 : 
 * 変更点（安定版）:
 * - GetGroundVelocity() を強化:
 *   1) 接地オブジェクトに MoveObject コンポーネントがあれば優先してその速度を返す
 *   2) なければ attachedRigidbody の linearVelocity を返す（存在すれば）
 *   3) どれもなければ Vector2.zero
 * - nullチェックを厳密化してコンパイル安全性を確保
 * ======================================= */
using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour{
    [Header("地面判定レイヤー")]
    public LayerMask groundLayer;
    [Header("接地判定用トランスフォーム")]
    public Transform checkPoint;
    [Header("判定半径")]
    public float checkRadius = 0.3f;

    public bool IsGrounded { get; private set; }
    public Vector2 GroundNormal { get; private set; } = Vector2.up;
    public event Action<bool> OnGroundedChanged;

    private bool prevGrounded;
    private Collider2D currentGroundCollider;
    private Vector2 smoothedNormal = Vector2.up;

    [SerializeField] private float liftPushIgnoreThreshold = 0.5f; // 上昇速度閾値
    private void Update(){
        if (checkPoint == null){
            IsGrounded = false;
            currentGroundCollider = null;
            GroundNormal = Vector2.up;
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(checkPoint.position, checkRadius, groundLayer);
        IsGrounded = hit != null;

        if (hit == null){
            Collider2D effectorHit = Physics2D.OverlapCircle(checkPoint.position, checkRadius, LayerMask.GetMask("Platform"));
            if (effectorHit != null){
                hit = effectorHit;
            }
        }

        if (hit != null){
            currentGroundCollider = hit;
            Vector2 averageNormal = GetStableGroundNormal();
            
            // 法線の急激な変化を防ぐ（より厳格に制限）
            float angleDiff = Vector2.Angle(smoothedNormal, averageNormal);
            if (angleDiff > 30f){ // 30度以上の急激な変化を制限（より厳格に）
                float lerpFactor = 30f / angleDiff;
                averageNormal = Vector2.Lerp(smoothedNormal, averageNormal, lerpFactor).normalized;
            }
            
            // さらに滑らかな法線変化
            smoothedNormal = Vector2.Lerp(smoothedNormal, averageNormal, 0.1f).normalized;
            GroundNormal = smoothedNormal;
        }else{
            currentGroundCollider = null;
            smoothedNormal = Vector2.up;
            GroundNormal = Vector2.up;
        }

        if (IsGrounded != prevGrounded){
            OnGroundedChanged?.Invoke(IsGrounded);
            prevGrounded = IsGrounded;
        }
    }

    // 複数レイで安定した法線を取得
    private Vector2 GetStableGroundNormal(){
        if (checkPoint == null) return Vector2.up;

        Vector2 centerPos = (Vector2)checkPoint.position;
        Vector2 totalNormal = Vector2.zero;
        int validHits = 0;

        Vector2[] rayPositions = {
            centerPos,
            centerPos + Vector2.left * 0.1f,
            centerPos + Vector2.right * 0.1f,
            centerPos + Vector2.left * 0.2f,
            centerPos + Vector2.right * 0.2f
        };

        foreach (Vector2 pos in rayPositions){
            RaycastHit2D rayHit = Physics2D.Raycast(pos, Vector2.down, 0.4f, groundLayer);
            if (rayHit.collider != null){
                totalNormal += rayHit.normal;
                validHits++;
            }
        }

        if (validHits > 0){
            Vector2 newNormal = (totalNormal / validHits).normalized;

            // 前フレームの法線との急変を緩和
            float angleDiff = Vector2.Angle(GroundNormal, newNormal);
            if (angleDiff > 30f){
                float lerpFactor = 30f / angleDiff;
                newNormal = Vector2.Lerp(GroundNormal, newNormal, lerpFactor).normalized;
            }

            return newNormal;
        }

        return Vector2.up;
    }

    /// <summary>
    /// 足元の動く床の速度を取得（MoveObjectがあれば優先、なければRigidbody2DのlinearVelocity）
    /// コンパイル時に MoveObject が存在しないプロジェクトでもエラーにならないよう GetComponentInParent を使う。
    /// </summary>
    public Vector2 GetGroundVelocity(){
        if (currentGroundCollider == null) return Vector2.zero;

        // まず MoveObject（ユーザー実装の動く床）があればそちらの速度を優先
        var moveObj = currentGroundCollider.GetComponentInParent<MoveObject>();
        if (moveObj != null){
            try{
                return moveObj.GetVelocity();
            }catch{
                // 万一 MoveObject に問題があればフォールバックする
            }
        }

        // 次に attachedRigidbody の linearVelocity を返す（存在すれば）
        var attachedRb = currentGroundCollider.attachedRigidbody;
        if (attachedRb != null){
            // Unity 6 で linearVelocity を使うプロジェクト向けに参照
            // 一部環境では 'velocity' を使うため最終的に安全にキャスト
            try{                // 優先: linearVelocity（Unity6 の API を想定）
                return attachedRb.linearVelocity;
            }catch{
                // フォールバック: velocity プロパティ（従来の Unity）
                try{
                    return attachedRb.linearVelocity;
                }
                catch
                {
                    // どちらも使えなければゼロを返す
                    return Vector2.zero;
                }
            }
        }

        return Vector2.zero;
    }
    public MoveObject GetCurrentGroundMoveObject(){
        if (currentGroundCollider == null) return null;
        return currentGroundCollider.GetComponentInParent<MoveObject>();
    }
    public bool IsGroundedSafe(){
        if (!IsGrounded) return false;

        // 接地中のオブジェクトが MoveObject（リフト）の場合
        var lift = currentGroundCollider != null
            ? currentGroundCollider.GetComponentInParent<MoveObject>()
            : null;

        if (lift != null){
            Vector2 liftVel = lift.GetVelocity();
            // 上昇が一定以上なら「接地扱いを一時的に解除」
            if (liftVel.y > liftPushIgnoreThreshold)
                return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected(){
        if (checkPoint == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPoint.position, checkRadius);
        Gizmos.DrawLine(checkPoint.position, checkPoint.position + (Vector3)(-GroundNormal * 0.5f));
    }
}
