using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [Header("プレイヤーの移動の速さ")]
    public float moveSpeed = 5f;
    [Header("プレイヤーのジャンプの高さ")]
    public float jumpForce = 12f;
    [Header("プレイヤーのHP")]
    public int hp = 10;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpCutApplied;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    void Update(){
        // 地面判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void FixedUpdate(){
        // 横移動
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // ジャンプ開始
        if (jumpPressed && isGrounded){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
            jumpCutApplied = false; // 新しいジャンプなのでリセット
        }

        // 可変ジャンプ処理
        if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 0){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            jumpCutApplied = true; // 一度だけ適用
        }
    }

    private void OnCollisionEnter2D(Collision2D other){
        //地面の場合
        if(other.gameObject.tag == "Ground"){
            isGrounded = true;
        }
        //敵の場合
        if(other.gameObject.tag == "Enemy"){
            HitEnemy(other.gameObject);
        }
    }
    private void HitEnemy(GameObject enemy){
        float halfscaleY = transform.lossyScale.y / 2.0f;
        float enemyHalfScaleY = enemy.transform.lossyScale.y / 2.0f;
        if(transform.position.y - (halfscaleY - 0.1f) >= enemy.transform.position.y + (enemyHalfScaleY - 0.1f)){
            Destroy(enemy);
        }else{
            enemy.GetComponent<EnemyManager>().PlayerDamage(this);
        }
    }

    // Invoke Unity Events 用
    public void OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context){
        if (context.started){
            jumpPressed = true;
            jumpHeld = true;
        }
        else if (context.canceled){
            jumpHeld = false;
        }
    }
    public void Damage(int damage){
        hp = Mathf.Max(hp - damage, 0);
    }
}