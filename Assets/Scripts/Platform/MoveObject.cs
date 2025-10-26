using UnityEngine;

public class MoveObject : MonoBehaviour{
    [Header("移動経路")] public GameObject[] movePoint;
    [Header("速さ")] public float speed = 1.0f;

    private Rigidbody2D rb;
    private int nowPoint = 0;
    private bool returnPoint = false;
    private Vector2 oldPos = Vector2.zero;
    private Vector2 myVelocity = Vector2.zero;

    private void Start(){
        rb = GetComponent<Rigidbody2D>();
        if (movePoint != null && movePoint.Length > 0 && rb != null){
            rb.position = movePoint[0].transform.position;
        }
    }
    public Vector2 GetVelocity(){
        return myVelocity;
    }

    private void FixedUpdate(){
        if (movePoint == null || movePoint.Length <= 1) return;

        Vector2 targetPos = returnPoint
            ? movePoint[nowPoint - 1].transform.position
            : movePoint[nowPoint + 1].transform.position;

        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // 到達判定
        if (Vector2.Distance(transform.position, targetPos) <= 0.05f){
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

        // 速度計算（オプション）
        Vector2 newPos = transform.position;
        myVelocity = (newPos - oldPos) / Time.deltaTime;
        oldPos = newPos;
    }
    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            collision.transform.SetParent(transform);
            Debug.Log($"Check 201 - [MoveObject] Player entered lift: {name}");
        }
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            collision.transform.SetParent(null);
            Debug.Log($"Check 202 - [MoveObject] Player exited lift: {name}");
        }
    }
}