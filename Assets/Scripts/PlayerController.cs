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
    public int maxHP = 10;
    [Header("無敵時間・点滅")]
    public float damageTime = 3f;
    public float flashTime = 0.34f;

    private bool facingRight = true;
    [SerializeField] private SwordFlipHandler swordHandler;
    [SerializeField] private WeaponManager weaponManager;

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
    private bool isAttack;
    private bool isAttacking;
    private bool isAirAttacking;

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
        
        // 空中攻撃中の状態管理
        if (isAirAttacking && isAttacking) {
            // 空中攻撃中はJumpステートを無効化
            anim.SetBool("Jump", false);
        }
    }

    void FixedUpdate(){
        Move();
        LookMoveDirection();
        Dead();
        // ジャンプ開始
        if (jumpPressed && isGrounded && !isAttacking){
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
        if (isAttack || isAttacking) return;
        // 横移動
        rb.AddForce(Vector2.right * moveInput.x * moveSpeed * 10f, ForceMode2D.Force);
        
        // 速度制限
        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed){
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * moveSpeed, rb.linearVelocity.y);
        }
    }
    private void LookMoveDirection(){
        if(moveInput.x > 0.0f){
            facingRight = true;
            if (sr != null) sr.flipX = false;
        }else if(moveInput.x < 0.0f){
            facingRight = false;
            if (sr != null) sr.flipX = true;
        }
        // 剣の向きと武器の左右反転を同期
        swordHandler?.UpdateSwordDirection(facingRight);
        weaponManager.Flip(facingRight);

        // 🔥 Animatorに状態を同期
        anim.SetBool("FacingRight", facingRight);
    }
    private void CheckGround(){
        // 地面判定をPhysics2D.OverlapCircleで行う
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool("IsGrounded", isGrounded);
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
            enemy.GetComponent<BaseEnemy>().Attack(this);
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
    //HPが0になった時の処理
    private void Dead(){
        if(hp <= 0){
            this.gameObject.SetActive(false); //Destroyでも良かったのですが安全性としてオブジェクトを残す
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
    public void OnAttack(InputAction.CallbackContext context){
        if (context.started && !isAttacking){
            isAttacking = true;
            isAirAttacking = !isGrounded;
            StartCoroutine(AttackRoutine());
            weaponManager.Attack(moveInput);
        }
    }

    private IEnumerator AttackRoutine(){
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.05f); // トリガー維持を短く
        
        // 攻撃アニメ再生中はジャンプ抑制
        // 空中攻撃の場合は少し長めに設定
        float attackDuration = isGrounded ? 0.3f : 0.6f;
        yield return new WaitForSeconds(attackDuration);
        
        // 攻撃終了時にトリガーをリセット
        anim.ResetTrigger("Attack");
        isAttacking = false;
        isAirAttacking = false;
    }
    //ダメージ処理
    public void TakeDamage(int damage){
        hp = Mathf.Clamp(hp - damage, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
    }
    //回復処理
    public void Heal(int healAmount){
        hp = Mathf.Clamp(hp + healAmount, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
    }

    public int GetHP(){
        return hp;
    }
    //攻撃の終了
    public void EndAttack(){
        isAttack = false;
        anim.ResetTrigger("Attack");
    }

}