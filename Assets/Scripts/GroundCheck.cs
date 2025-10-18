using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour{
    [Header("地面判定レイヤー")]
    [SerializeField] private LayerMask groundLayer;
    [Header("接地判定用トランスフォーム")]
    [SerializeField] private Transform checkPoint;
    [Header("判定半径")]
    [SerializeField] private float checkRadius = 0.1f;

    public bool IsGrounded { get; private set; }
    // Ground状態が変化したときに通知するイベント
    public event Action<bool> OnGroundedChanged;

    private bool prevGrounded;
    private Collider2D currentGroundCollider;

    void Update(){
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
    }
}
