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
    public Vector2 GroundNormal { get; private set; } = Vector2.up; // 坂の法線向き
    // Ground状態が変化したときに通知するイベント
    public event Action<bool> OnGroundedChanged;

    private bool prevGrounded;
    private Collider2D currentGroundCollider;

    private void Update(){
        // 円判定
        Collider2D hit = Physics2D.OverlapCircle(checkPoint.position, checkRadius, groundLayer);
        IsGrounded = hit != null;

        if (hit != null){
            // 現在の接地オブジェクトを記録
            currentGroundCollider = hit;
            
            // 複数のRaycastでより安定した法線を取得
            Vector2 averageNormal = GetStableGroundNormal();
            GroundNormal = averageNormal;
        }else{
            currentGroundCollider = null;
            GroundNormal = Vector2.up;
        }

        // 接地変化イベントを通知
        if (IsGrounded != prevGrounded){
            OnGroundedChanged?.Invoke(IsGrounded);
            prevGrounded = IsGrounded;
        }
    }

    // 安定した地面の法線を取得する
    private Vector2 GetStableGroundNormal(){
        Vector2 centerPos = checkPoint.position;
        Vector2 totalNormal = Vector2.zero;
        int validHits = 0;
        
        // 複数の位置でRaycastを実行
        Vector2[] rayPositions = {
            centerPos,
            centerPos + Vector2.left * 0.1f,
            centerPos + Vector2.right * 0.1f,
            centerPos + Vector2.left * 0.2f,
            centerPos + Vector2.right * 0.2f
        };
        
        foreach (Vector2 pos in rayPositions){
            RaycastHit2D rayHit = Physics2D.Raycast(pos, Vector2.down, 0.4f, groundLayer);
            if (rayHit){
                totalNormal += rayHit.normal;
                validHits++;
            }
        }
        
        // 平均値を計算し、急激な変化を抑制
        if (validHits > 0){
            Vector2 newNormal = (totalNormal / validHits).normalized;
            
            // 前フレームの法線との差を制限（急激な変化を防ぐ）
            float angleDiff = Vector2.Angle(GroundNormal, newNormal);
            if (angleDiff > 30f){ // 30度以上の急激な変化は制限
                float lerpFactor = 30f / angleDiff;
                newNormal = Vector2.Lerp(GroundNormal, newNormal, lerpFactor).normalized;
            }
            
            return newNormal;
        }
        
        return Vector2.up;
    }

    // 足元の動く床の速度を取得（なければゼロ）
    public Vector2 GetGroundVelocity(){
        if (currentGroundCollider == null) return Vector2.zero;
        var moveObject = currentGroundCollider.GetComponent<MoveObject>();
        return moveObject != null ? moveObject.GetVelocity() : Vector2.zero;
    }

    void OnDrawGizmosSelected(){
        if (checkPoint == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPoint.position, checkRadius);
        Gizmos.DrawLine(checkPoint.position, checkPoint.position + (Vector3)(-GroundNormal * 0.5f));
    }
}
/* 旧処理
 *     void Update(){
        currentGroundCollider = Physics2D.OverlapCircle(checkPoint.position, checkRadius, groundLayer);
        IsGrounded = currentGroundCollider != null;

        if (currentGroundCollider != null)
            Debug.Log($"Ground hit: {currentGroundCollider.name}, Layer: {LayerMask.LayerToName(currentGroundCollider.gameObject.layer)}");

        // 前回と異なる場合にイベントを発火
        if (IsGrounded != prevGrounded){
            OnGroundedChanged?.Invoke(IsGrounded);
            prevGrounded = IsGrounded;
        }
    }
*/