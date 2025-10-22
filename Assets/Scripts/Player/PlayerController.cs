/* =======================================
 * ファイル名 : PlayerController.cs
 * 概要 : プレイヤースクリプト
 * Date : 2025/10/21
 * Version : 0.01
 * ======================================= */
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
    [Header("プレイヤーのSP")]
    public int sp = 6;
    public int maxSP = 6;
    [Header("無敵時間・点滅")]
    public float damageTime = 3f;
    public float flashTime = 0.34f;
    [Header("すり抜け時間")]
    [SerializeField] private float dropThroughTime = 0.3f; // すり抜け時間

    private bool facingRight = true;
    [Header("攻撃アクション反転など")]
    [SerializeField] private SwordFlipHandler swordHandler;
    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int specialCost = 1;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private Animator anim;
    private SpriteRenderer sr;
    // 動く床の水平速度を前フレームからの差分で補正するための記録
    private float appliedGroundVelocityX = 0f;

    [Header("Ground Check オブジェクト参照")]
    [SerializeField] private GroundCheck groundCheck; // ← 追加
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isDropping = false;
    private PlatformEffector2D currentEffector; // 足元の床Effector参照

    private bool isGrounded;
    private bool isAttack;
    private bool isAttacking;
    private bool isAirAttacking;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // GroundCheckイベント購読
        if (groundCheck != null){
            groundCheck.OnGroundedChanged += OnGroundedChanged;
        }
    }
    private void OnDestroy(){
        if (groundCheck != null){
            groundCheck.OnGroundedChanged -= OnGroundedChanged;
        }
    }

    void Update(){
        // アニメーション更新
        anim.SetBool("Walk", Mathf.Abs(moveInput.x) > 0.1f);
        anim.SetBool("Jump", !isGrounded);
        
        // 空中攻撃中の状態管理
        if (isAirAttacking && isAttacking) {
            // 空中攻撃中はJumpステートを無効化
            anim.SetBool("Jump", false);
        }
    }

    private void FixedUpdate(){
        // 動く床の速度補正を適用
        ApplyMovingPlatformVelocity();
        
        if (!isAttacking){
            MoveAlongSlope(); // ← 坂道も平地も兼ねる
        }
        LookMoveDirection();
        Dead();

        // 物理演算による意図しないジャンプを防止
        PreventUnintendedJump();

        // ジャンプ開始
        if (jumpPressed && isGrounded && !isAttacking){
            Debug.Log($"ジャンプ実行: isGrounded={isGrounded}, jumpPressed={jumpPressed}, isAttacking={isAttacking}");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPressed = false;
            jumpCutApplied = false;
        }

        // 可変ジャンプ
        if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 0){
            rb.AddForce(Vector2.down * rb.linearVelocity.y * 0.5f, ForceMode2D.Impulse);
            jumpCutApplied = true;
        }
    }

    // 物理演算による意図しないジャンプを防止
    private void PreventUnintendedJump(){
        if (!isGrounded) return;
        
        // 坂の頂上付近での物理的な跳ね返りを抑制
        if (groundCheck.IsGrounded && groundCheck.GroundNormal != Vector2.up){
            float slopeAngle = Vector2.Angle(groundCheck.GroundNormal, Vector2.up);
            
            // 平坦に近い部分（25度以下）で垂直速度を抑制
            if (slopeAngle < 25f && rb.linearVelocity.y > 0.5f){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
            
            // 坂道での過度な垂直速度を抑制
            if (slopeAngle >= 25f && slopeAngle <= 65f && Mathf.Abs(rb.linearVelocity.y) > 3f){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }
    private void MoveAlongSlope(){
        if (isAttack || isAttacking) return;

        // 基本的な水平移動
        Vector2 moveDir = new Vector2(moveInput.x, 0f);
        
        // 接地時のみ坂道処理を適用
        if (groundCheck.IsGrounded && groundCheck.GroundNormal != Vector2.up){
            float slopeAngle = Vector2.Angle(groundCheck.GroundNormal, Vector2.up);
            
            // 急すぎる坂（70度以上）では通常移動に切り替え
            if (slopeAngle > 70f){
                moveDir = new Vector2(moveInput.x, 0f);
                rb.AddForce(moveDir * moveSpeed * 10f, ForceMode2D.Force);
            }
            else {
                // 坂の接線を2方向取得
                Vector2 slopeDir1 = Vector2.Perpendicular(groundCheck.GroundNormal).normalized;
                Vector2 slopeDir2 = -slopeDir1;

                // 入力方向に近い方を選択
                Vector2 slopeDir = (Mathf.Sign(moveInput.x) == Mathf.Sign(slopeDir1.x)) ? slopeDir1 : slopeDir2;

                // 入力に応じた方向（加速補正なし、一定速度）
                moveDir = slopeDir * Mathf.Abs(moveInput.x);

                // 坂道でも一定速度で移動（加速補正を削除）
                rb.AddForce(moveDir * moveSpeed * 10f, ForceMode2D.Force);
                
                // 坂の頂上付近での物理演算によるジャンプを完全に抑制
                if (slopeAngle < 15f && Mathf.Abs(rb.linearVelocity.y) > 1f){
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }
            }
        }else{
            moveDir = new Vector2(moveInput.x, 0f);
            rb.AddForce(moveDir * moveSpeed * 10f, ForceMode2D.Force);
        }

        // 速度制限（坂道でも一定速度を維持）
        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed){
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * moveSpeed, rb.linearVelocity.y);
        }
    }
    private void SlideDownSlope(){
        // 坂でなければ無効
        float slopeAngle = Vector2.Angle(groundCheck.GroundNormal, Vector2.up);
        if (slopeAngle < 5f) return; // 平地は除外

        // 坂方向（重力に沿って）滑る
        Vector2 slopeDir = Vector2.Perpendicular(groundCheck.GroundNormal);
        if (slopeDir.y > 0)
            slopeDir *= -1f;

        rb.AddForce(slopeDir.normalized * moveSpeed * 8f, ForceMode2D.Force);
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
    private void OnGroundedChanged(bool grounded){
        Debug.Log($"接地状態変化: {grounded}");
        isGrounded = grounded;
        anim.SetBool("IsGrounded", grounded);
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
    //HPが0になった時の処理、Failureにする
    private void Dead(){
        if(hp <= 0){
            this.gameObject.SetActive(false); //Destroyでも良かったのですが安全性としてオブジェクトを残す
            //Failure処理へ
        }
    }
    //下に落下したらFailureにする
    private void OnBecameInvisible(){
        Camera camera = Camera.main;
        Debug.Log($"camera.name => {camera.name}");
        if(camera.name == "Main Camera" && camera.transform.position.y > transform.position.y){
            Destroy(gameObject);
            //Failure処理へ
        }
    }
    // Invoke Unity Events 用
    public void OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();

        // ↓キー押下中の処理チェック
        if (moveInput.y < -0.5f && !isDropping && groundCheck.IsGrounded){
            StartCoroutine(DropThroughPlatform());
        }
        if (moveInput.y < -0.5f && groundCheck.IsGrounded){
            SlideDownSlope();
        }
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
    public void OnSpecialA(InputAction.CallbackContext context){
        Debug.Log($"localScale.x = {transform.localScale.x}, spriteFlipX = {GetComponent<SpriteRenderer>()?.flipX}");
        if (!context.performed) return;
        if (sp < specialCost) return;

        UseSpecial(specialCost);

        var bulletObj = Instantiate(playerBulletPrefab, firePoint.position, Quaternion.identity);
        var bullet = bulletObj.GetComponent<PlayerBullet>();
        if (bullet != null){
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            bullet.Setup(dir);
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
    public void HealHP(int healAmount){
        hp = Mathf.Clamp(hp + healAmount, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
    }
    //SP消費処理
    public void UseSpecial(int useSP){
        sp = Mathf.Clamp(sp - useSP, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }
    //SP回復処理
    public void HealSP(int healSpAmount){
        sp = Mathf.Clamp(sp + healSpAmount, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    public int GetHP(){
        return hp;
    }
    public int GetSP(){
        return sp;
    }

    //攻撃の終了
    public void EndAttack(){
        isAttack = false;
        anim.ResetTrigger("Attack");
    }
    // 動く床の速度補正を適用
    private void ApplyMovingPlatformVelocity(){
        if (!isGrounded) return;
        
        // 足元の動く床の速度を取得
        Vector2 groundVelocity = groundCheck.GetGroundVelocity();
        
        // 前フレームで適用した速度を相殺
        rb.linearVelocity -= new Vector2(appliedGroundVelocityX, 0f);
        
        // 新しい床の速度を適用
        rb.linearVelocity += groundVelocity;
        
        // 次フレーム用に記録
        appliedGroundVelocityX = groundVelocity.x;
    }

    private IEnumerator DropThroughPlatform(){
        // すり抜け中フラグ
        isDropping = true;

        // GroundCheckの下にあるPlatformEffectorを検出
        Collider2D hit = Physics2D.OverlapCircle(
            groundCheck.transform.position,
            groundCheck.checkRadius,
            groundCheck.groundLayer
        );

        if (hit != null){
            // PlatformEffector と PlatformType を探す（親にある可能性があるので GetComponentInParent を使用）
            PlatformType platformType = hit.GetComponentInParent<PlatformType>();
            PlatformEffector2D eff = hit.GetComponentInParent<PlatformEffector2D>();

            // プラットフォームが存在し、かつ drop を許可している場合だけ落下処理を行う
            if (platformType != null && platformType.allowDropThrough && eff != null){
                // 回転させて一時的に衝突方向を反転（落下可能にする）
                float originalOffset = eff.rotationalOffset;
                eff.rotationalOffset = 180f;

                // 少し時間を置いて下に抜ける
                yield return new WaitForSeconds(dropThroughTime);

                // 元に戻す
                eff.rotationalOffset = originalOffset;
            }
        }

        // 小さな猶予を置いて二重呼び出しを防ぐ（調整可）
        yield return new WaitForSeconds(0.05f);
        isDropping = false;
    }

}