using UnityEngine;
using System.Collections;

public class EnemyThwomp : BaseEnemy {
    [Header("ドッスン設定")]
    [SerializeField] private float triggerRange = 3f;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float riseSpeed = 2f;
    [SerializeField] private float groundWaitTime = 1f;

    [Header("地面検知")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Vector2 startPosition;
    private bool isFalling;
    private bool isRising;

    protected override void Start(){
        base.Start();
        rb.isKinematic = true;
        startPosition = transform.position;
    }

    protected override void Update(){
        base.Update(); // （shootBehaviorがある場合のみ必要）

        if (Player == null) return;
        float distX = Mathf.Abs(Player.position.x - transform.position.x);

        if (!isFalling && !isRising && distX <= triggerRange){
            StartCoroutine(FallRoutine());
        }
    }

    private IEnumerator FallRoutine(){
        isFalling = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector2.down * fallSpeed;

        // 地面到達待機
        yield return new WaitUntil(() => IsGrounded());

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        yield return new WaitForSeconds(groundWaitTime);

        isFalling = false;
        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine(){
        isRising = true;

        while (transform.position.y < startPosition.y){
            transform.position = Vector2.MoveTowards(
                transform.position,
                startPosition,
                riseSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = startPosition;
        isRising = false;
    }

    private bool IsGrounded(){
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected(){
        if (groundCheck){
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * triggerRange);
        Gizmos.DrawLine(transform.position, transform.position - Vector3.right * triggerRange);
    }
}
