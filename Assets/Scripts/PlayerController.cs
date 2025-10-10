using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [Header("プレイヤーの移動の速さ")]
    public float moveSpeed = 5f;
    [Header("プレイヤーのジャンプの高さ")]
    public float jumpForce = 12f;
    [Header("プレイヤーのHP")]
    public int hp = 10;
    [Header("無敵時間・点滅")]
    public float damageTime = 3f;
    public float flashTime = 0.34f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private Animator anim;
    private SpriteRenderer sr;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update(){
        // 地面判定
        CheckGround();
        
        // アニメーション更新
        anim.SetBool("Walk", moveInput.x != 0.0f);
        anim.SetBool("Jump", !isGrounded);
    }

    void FixedUpdate(){
        Move();
        LookMoveDirection();

        // ジャンプ開始
        if (jumpPressed && isGrounded){
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPressed = false;
            jumpCutApplied = false; // 新しいジャンプなのでリセット
        }
        // 可変ジャンプ処理
        if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 0){
            rb.AddForce(Vector2.down * rb.linearVelocity.y * 0.5f, ForceMode2D.Impulse);
            jumpCutApplied = true; // 一度だけ適用
        }
    }

    private void Move(){
        // 横移動
        rb.AddForce(Vector2.right * moveInput.x * moveSpeed * 10f, ForceMode2D.Force);
        
        // 速度制限
        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed){
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * moveSpeed, rb.linearVelocity.y);
        }
    }
    private void LookMoveDirection(){
        if(moveInput.x > 0.0f){
            transform.eulerAngles = Vector3.zero;
        }else if(moveInput.x < 0.0f){
            transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
    
    }

    private void CheckGround(){
        // 地面判定をPhysics2D.OverlapCircleで行う
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnCollisionEnter2D(Collision2D other){
        //敵の場合
        if(other.gameObject.tag == "Enemy"){
            HitEnemy(other.gameObject);
            gameObject.layer = LayerMask.NameToLayer("PlayerDamage");
            Debug.Log($"Check001 - Damage!!");
        }
    }
    private void HitEnemy(GameObject enemy){
        float halfscaleY = transform.lossyScale.y / 2.0f;
        float enemyHalfScaleY = enemy.transform.lossyScale.y / 2.0f;
        if(transform.position.y - (halfscaleY - 0.1f) >= enemy.transform.position.y + (enemyHalfScaleY - 0.1f)){
            Destroy(enemy);
            rb.AddForce(Vector2.up * jumpForce * 0.5f, ForceMode2D.Impulse);
        }else{
            enemy.GetComponent<EnemyManager>().PlayerDamage(this);
            StartCoroutine(Damage());
        }
    }
    //無敵時間
    IEnumerator Damage(){
        Debug.Log($"Check002 - Damage! 現在のgameObject.layer -> {gameObject.layer}");
        Color color = sr.color;
        for(int i = 0; i < damageTime; i++){
            yield return new WaitForSeconds(flashTime);
            sr.color = new Color(color.r, color.g, color.b, 0.0f);

            yield return new WaitForSeconds(flashTime);
            sr.color = new Color(color.r, color.g, color.b, 1.0f);
        }
        sr.color = color;
        gameObject.layer = LayerMask.NameToLayer("Default");
        Debug.Log($"Check003 - 無敵時間終了 現在のgameObject.layer -> {gameObject.layer}");
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
    public int GetHP(){
        return hp;
    }
}