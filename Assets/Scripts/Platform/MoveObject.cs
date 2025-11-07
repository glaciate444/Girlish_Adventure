using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveObject : MonoBehaviour {
    [Header("移動経路")] public GameObject[] movePoint;
    [Header("速さ")] public float speed = 1.0f;

    private Rigidbody2D rb;
    private int nowPoint = 0;
    private bool returnPoint = false;
    private Vector2 oldPos = Vector2.zero;
    private Vector2 myVelocity = Vector2.zero;

    private void Start(){
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // 物理力を受けない
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (movePoint != null && movePoint.Length > 0)
            rb.position = movePoint[0].transform.position;

        oldPos = rb.position;
    }

    public Vector2 GetVelocity() => myVelocity;

    private void FixedUpdate(){
        if (movePoint == null || movePoint.Length <= 1) return;

        Vector2 targetPos = returnPoint
            ? movePoint[nowPoint - 1].transform.position
            : movePoint[nowPoint + 1].transform.position;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // 到達判定
        if (Vector2.Distance(newPos, targetPos) <= 0.05f){
            if (!returnPoint){
                nowPoint++;
                if (nowPoint + 1 >= movePoint.Length)
                    returnPoint = true;
            }else{
                nowPoint--;
                if (nowPoint <= 0)
                    returnPoint = false;
            }
        }

        // 速度計算
        myVelocity = (newPos - oldPos) / Time.fixedDeltaTime;
        oldPos = newPos;
    }

    // 接触時に velocity を渡す方法（親子化しない）
    private void OnCollisionStay2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            var rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null){
                // リフトの移動分をプレイヤーに足す
                rbPlayer.linearVelocity += myVelocity * Time.fixedDeltaTime;
            }
        }
    }
}
