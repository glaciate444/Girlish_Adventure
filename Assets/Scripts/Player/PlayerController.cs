/* =======================================
 * ファイル名 : PlayerController.cs
 * 概要 : プレイヤースクリプト（非物理風・完全安定版）
 * Date : 2025/10/24
 * Version : 0.02
 * ======================================= */
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour{
    [Header("プレイヤーの移動の速さ")]
    public float moveSpeed = 5f;
    [Header("プレイヤーのジャンプの高さ（垂直速度）")]
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
    [SerializeField] private float dropThroughTime = 0.3f;

    private bool facingRight = true;
    [Header("攻撃アクション反転など")]
    [SerializeField] private SwordFlipHandler swordHandler;
    [SerializeField] private WeaponBase weaponBase;
    [Header("スペシャル技")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int specialCost = 1;
    [Header("ヒットしたエフェクト")]
    [SerializeField] private HitEffectSpawner hitEffectSpawner;

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
    [SerializeField] private GroundCheck groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isDropping = false;
    private PlatformEffector2D currentEffector;
    //アニメーションbool値
    private bool isGrounded;
    private bool isAttack;
    private bool isAttacking;
    private bool isAirAttacking;
    //そのほかのフィールド変数
    private bool isInvincible = false;
    public delegate void OnDamageDelegate();
    public event OnDamageDelegate OnDamage;
    private bool isDead = false;
    
    // シンプルな坂道制御用の変数
    private float currentSlopeAngle = 0f;
    private bool wasOnSlope = false;
    private float slopeTransitionBuffer = 0f;

    // 非物理風制御用（物理は当たり判定用に残す）
    [Header("非物理風パラメータ")]
    [SerializeField] private float airControlFactor = 0.8f; // 空中の横移動倍率
    [SerializeField] private float slopeAcceptAngle = 50f; // これ以下は平地扱い
    [SerializeField] private float slopeTransitionSpeed = 0.1f; // 坂道移行の滑らかさ
    [SerializeField] private float angleHysteresis = 5f; // 角度判定のヒステリシス

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        
        // 物理演算の干渉を防ぐ設定（Unity 6対応）
        if (rb != null){
            rb.freezeRotation = true; // 回転を固定
            rb.linearDamping = 0f; // 線形減衰を0に（Unity 6の新しいAPI）
            rb.angularDamping = 0f; // 角減衰を0に（Unity 6の新しいAPI）
        }

        if (groundCheck != null){
            groundCheck.OnGroundedChanged += OnGroundedChanged;
        }

        if (anim != null){
            anim.SetBool("FacingRight", facingRight);
        }
        Debug.Log($"Start - 初期facingRight: {facingRight}");
    }
    private void OnDestroy(){
        if (groundCheck != null){
            groundCheck.OnGroundedChanged -= OnGroundedChanged;
        }
    }

    void Update(){
        if (anim != null){
            anim.SetBool("Walk", Mathf.Abs(moveInput.x) > 0.1f);
            // 攻撃中はJumpパラメータを更新しない（空中攻撃のため）
            if (!isAttacking){
                anim.SetBool("Jump", !isGrounded);
            }
        }
        ForceSyncFacingDirection();
        
        // シンプルな坂道制御
        UpdateSlopeTransition();
    }

    private void FixedUpdate(){
        if (isDead) return;

        // 動く床の速度補正（当たり判定はPhysicsに任せる）
        ApplyMovingPlatformVelocity();

        if (!isAttacking){
            MoveAlongSlope(); // linearVelocity を使用した移動（AddForceは使わない）
        }

        LookMoveDirection();
        Dead();

        // 可変ジャンプ処理（ただしジャンプ実行後はジャンプ解除でカット）
        if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 0){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            jumpCutApplied = true;
        }

        // Animator同期
        if (anim != null){
            // 攻撃中はJumpパラメータを更新しない（空中攻撃のため）
            if (!isAttacking){
                anim.SetBool("Jump", !isGrounded);
            }
            anim.SetBool("Walk", Mathf.Abs(moveInput.x) > 0.1f);
        }
    }

    private void MoveAlongSlope(){
        if (isAttack || isAttacking) return;

        // 左右移動を統一するため、入力値を正規化
        float moveX = 0f;
        if (Mathf.Abs(moveInput.x) > 0.1f){
            moveX = Mathf.Sign(moveInput.x); // 左右の方向のみを取得（-1 または 1）
        }
        float rawSlopeAngle = 0f;

        if (groundCheck != null && groundCheck.IsGrounded){
            rawSlopeAngle = Vector2.Angle(groundCheck.GroundNormal, Vector2.up);
        }

        // 角度の滑らかな変化（ヒステリシス適用）
        float targetAngle = rawSlopeAngle;
        if (wasOnSlope && rawSlopeAngle < slopeAcceptAngle + angleHysteresis){
            // 坂道から平地への移行時は緩やかに角度を減らす
            targetAngle = Mathf.Lerp(currentSlopeAngle, rawSlopeAngle, slopeTransitionSpeed);
        }else{
            // 通常時は即座に角度を更新
            targetAngle = rawSlopeAngle;
        }
        
        currentSlopeAngle = targetAngle;
        bool isCurrentlyOnSlope = currentSlopeAngle > slopeAcceptAngle;
        
        // シンプルな坂道制御：移行時にバッファを設定
        if (wasOnSlope && !isCurrentlyOnSlope){
            slopeTransitionBuffer = 0.1f; // 0.1秒のバッファ
            Debug.Log("坂道から平地への移行検出");
        }
        
        wasOnSlope = isCurrentlyOnSlope;

        // シンプルな移動制御
        if (groundCheck != null && groundCheck.IsGrounded && currentSlopeAngle <= slopeAcceptAngle){
            float targetVX = moveX * moveSpeed;
            
            // デバッグ用：左右速度の確認
            if (Mathf.Abs(moveX) > 0.1f){
                Debug.Log($"地上移動 - 正規化後moveX: {moveX}, targetVX: {targetVX}, 現在速度: {rb.linearVelocity.x}");
            }
            
            // 移行バッファ中は垂直速度を制限
            if (slopeTransitionBuffer > 0f){
                float currentVY = rb.linearVelocity.y;
                float limitedVY = Mathf.Max(currentVY, -1f); // 下向き速度を制限
                rb.linearVelocity = new Vector2(targetVX, limitedVY);
            }else{
                rb.linearVelocity = new Vector2(targetVX, rb.linearVelocity.y);
            }
        }
        // 坂道では移動を制限
        else if (groundCheck != null && groundCheck.IsGrounded && currentSlopeAngle > slopeAcceptAngle){
            float currentVX = rb.linearVelocity.x;
            float newVX = Mathf.Lerp(currentVX, 0f, slopeTransitionSpeed * 2f);
            rb.linearVelocity = new Vector2(newVX, rb.linearVelocity.y);
        }
        // 空中は空中制御（左右速度を統一）
        else{
            float targetVX = moveX * moveSpeed * airControlFactor;
            
            // デバッグ用：左右速度の確認
            if (Mathf.Abs(moveX) > 0.1f){
                Debug.Log($"空中移動 - 正規化後moveX: {moveX}, targetVX: {targetVX}, 現在速度: {rb.linearVelocity.x}");
            }
            
            // 左右の速度を統一するため、直接設定（Lerpを削除）
            rb.linearVelocity = new Vector2(targetVX, rb.linearVelocity.y);
        }

        // 最大速度制限（左右統一）
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
        if (currentSpeed > moveSpeed){
            float direction = Mathf.Sign(rb.linearVelocity.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void LookMoveDirection(){
        if (moveInput.x > 0.1f){
            facingRight = true;
            if (sr != null) sr.flipX = false;
        }else if (moveInput.x < -0.1f){
            facingRight = false;
            if (sr != null) sr.flipX = true;
        }

        if (sr != null){
            if (!sr.flipX && !facingRight){
                facingRight = true;
            }else if (sr.flipX && facingRight){
                facingRight = false;
            }
        }
        swordHandler?.UpdateSwordDirection(facingRight);

        if (weaponBase != null){
            var swordWeapon = weaponBase.GetComponent<SwordWeapon>();
            if (swordWeapon != null){
                swordWeapon.SetFacingRight(facingRight);
            }
        }else{
            Debug.LogWarning("weaponBaseがnullです。InspectorでWeaponBaseを設定してください。");
        }

        if (anim != null){
            anim.SetBool("FacingRight", facingRight);
        }
    }

    private void ForceSyncFacingDirection(){
        if (sr == null || anim == null) return;
        if (!sr.flipX && !facingRight){
            facingRight = true;
            anim.SetBool("FacingRight", true);
        }else if (sr.flipX && facingRight){
            facingRight = false;
            anim.SetBool("FacingRight", false);
        }
    }

    private void OnGroundedChanged(bool grounded){
        isGrounded = grounded;
        if (anim != null){
            anim.SetBool("IsGrounded", grounded);
        }
    }

    private void OnCollisionEnter2D(Collision2D other){
        if (isInvincible) return;
        if (other.gameObject.CompareTag("Enemy")){
            HitEnemy(other.gameObject);
            hitEffectSpawner.SpawnHitEffect(other.transform.position);
        }
    }

    private void HitEnemy(GameObject enemy){
        float halfscaleY = transform.lossyScale.y / 2.0f;
        float enemyHalfScaleY = enemy.transform.lossyScale.y / 2.0f;
        if (transform.position.y - (halfscaleY - 0.1f) >= enemy.transform.position.y + (enemyHalfScaleY - 0.1f)){
            Destroy(enemy);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.5f);
            gameObject.layer = LayerMask.NameToLayer("Player");
        }else{
            if (!isInvincible){
                gameObject.layer = LayerMask.NameToLayer("PlayerDamage");
                enemy.GetComponent<BaseEnemy>().Attack(this);
                StartCoroutine(Damage());
            }
        }
    }

    private IEnumerator Damage(){
        if (isInvincible) yield break;
        isInvincible = true;
        gameObject.layer = LayerMask.NameToLayer("PlayerDamage");
        Color color = sr.color;
        float elapsed = 0f;
        while (elapsed < damageTime){
            sr.color = new Color(color.r, color.g, color.b, 0.1f);
            yield return new WaitForSeconds(flashTime);
            sr.color = new Color(color.r, color.g, color.b, 1.0f);
            yield return new WaitForSeconds(flashTime);
            elapsed += flashTime * 2f;
        }
        sr.color = color;
        gameObject.layer = LayerMask.NameToLayer("Player");
        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("FallZone")){
            StartCoroutine(HandleFallDeath());
        }
    }

    private IEnumerator HandleFallDeath(){
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void Dead(){
        if (isDead) return;
        if (hp > 0) return;
        isDead = true;
        Debug.Log("Player Dead");
        this.gameObject.SetActive(false);
    }

    // Input callbacks (Unity Input System)
    public void OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();
        
        // デバッグ用：入力値の確認
        if (Mathf.Abs(moveInput.x) > 0.1f){
            Debug.Log($"入力値 - moveInput.x: {moveInput.x}, 絶対値: {Mathf.Abs(moveInput.x)}");
        }
        
        LookMoveDirection();

        if (moveInput.y < -0.5f && !isDropping && groundCheck != null && groundCheck.IsGrounded){
            StartCoroutine(DropThroughPlatform());
        }
    }

    public void OnJump(InputAction.CallbackContext context){
        if (context.started){
            jumpPressed = true;
            jumpHeld = true;

            if (isGrounded && !isAttacking){
                float slopeAngle = 0f;
                if (groundCheck != null && groundCheck.IsGrounded)
                    slopeAngle = Vector2.Angle(groundCheck.GroundNormal, Vector2.up);

                if (slopeAngle <= slopeAcceptAngle){
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    jumpPressed = false;
                    jumpCutApplied = false;
                    if (anim != null) anim.SetTrigger("Jump");
                }else{
                    // 角度が急ならジャンプ禁止（安定のため）
                }
            }
        }else if (context.canceled){
            jumpHeld = false;
            if (rb.linearVelocity.y > 0f){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context){
        if (context.started && !isAttacking){
            isAttacking = true;
            isAirAttacking = !isGrounded;

            // 攻撃トリガーを設定（空中攻撃の場合は特別な処理）
            if (anim != null){
                if (isAirAttacking){
                    // 空中攻撃の場合は、Jumpをfalseにしてから強制的にAirAttackアニメーションを再生
                    Debug.Log($"攻撃前 - Jump: {anim.GetBool("Jump")}, IsGrounded: {isGrounded}");
                    anim.SetBool("Jump", false);
                    Debug.Log($"攻撃後 - Jump: {anim.GetBool("Jump")}");
                    anim.Play(facingRight ? "AirAttack_Sword_Right" : "AirAttack_Sword_Left", 0, 0f);
                    Debug.Log($"空中攻撃開始 - Jumpをfalseに設定、FacingRight: {facingRight}, IsGrounded: {isGrounded}");
                }else{
                    // 地上攻撃の場合は通常の処理
                    anim.ResetTrigger("Attack");
                    anim.SetTrigger("Attack");
                }
            }

            StartCoroutine(AttackRoutine());

            if (weaponBase != null){
                weaponBase.StartAttack(moveInput);
            }else{
                Debug.LogError("weaponBaseがnullです。攻撃処理をスキップします。");
            }
        }
    }

    public void OnSpecialA(InputAction.CallbackContext context){
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
        // 攻撃トリガーは既にOnAttackで設定済み
        yield return new WaitForSeconds(0.05f);
        float attackDuration = isGrounded ? 0.3f : 0.6f;
        yield return new WaitForSeconds(attackDuration);
        
        // 空中攻撃の場合は、AirAttackStateBehaviourでトリガーをリセット
        // 地上攻撃の場合はここでリセット
        if (anim != null && isGrounded) {
            anim.ResetTrigger("Attack");
        }
        
        // 空中攻撃終了時にJumpパラメータをtrueに戻す（バックアップ処理）
        if (anim != null && isAirAttacking && !isGrounded){
            anim.SetBool("Jump", true);
            Debug.Log("AttackRoutine - 空中攻撃終了、Jumpパラメータをtrueに戻す");
        }
        
        isAttacking = false;
        isAirAttacking = false;
        
        // 攻撃終了後、Jumpパラメータを正常に復元
        if (anim != null && !isGrounded){
            anim.SetBool("Jump", true);
            Debug.Log("攻撃終了後 - Jumpパラメータを復元");
        }
    }

    public void TakeDamage(int damage){
        if (isInvincible) return;
        hp = Mathf.Clamp(hp - damage, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
        OnDamage?.Invoke();
    }

    public void HealHP(int healAmount){
        hp = Mathf.Clamp(hp + healAmount, 0, maxHP);
        UIManager.Instance?.UpdateHP(hp, maxHP);
    }

    public void UseSpecial(int useSP){
        sp = Mathf.Clamp(sp - useSP, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    public void HealSP(int healSpAmount){
        sp = Mathf.Clamp(sp + healSpAmount, 0, maxSP);
        UIManager.Instance?.UpdateSP(sp, maxSP);
    }

    public int GetHP() { return hp; }
    public int GetSP() { return sp; }

    public void EndAttack(){
        isAttack = false;
        anim?.ResetTrigger("Attack");
    }

    // 動く床の速度補正（左右速度を統一）
    private void ApplyMovingPlatformVelocity(){
        if (!isGrounded) return;

        Vector2 groundVelocity = Vector2.zero;
        if (groundCheck != null)
            groundVelocity = groundCheck.GetGroundVelocity();

        // デバッグ用：動く床の速度確認
        if (Mathf.Abs(groundVelocity.x) > 0.01f){
            Debug.Log($"動く床検出 - 床速度: {groundVelocity.x}, 適用前速度: {rb.linearVelocity.x}");
        }

        // 前フレーム適用分を相殺
        rb.linearVelocity -= new Vector2(appliedGroundVelocityX, 0f);

        // 新しい床速度を適用（水平＋垂直両方）
        // 左右の速度差を防ぐため、床速度を直接加算
        rb.linearVelocity += groundVelocity;

        appliedGroundVelocityX = groundVelocity.x;
        
        // デバッグ用：適用後の速度確認
        if (Mathf.Abs(groundVelocity.x) > 0.01f){
            Debug.Log($"動く床適用後 - 最終速度: {rb.linearVelocity.x}");
        }
    }
    private IEnumerator DropThroughPlatform(){
        isDropping = true;

        if (groundCheck == null){
            yield return new WaitForSeconds(dropThroughTime);
            isDropping = false;
            yield break;
        }

        Collider2D hit = Physics2D.OverlapCircle(groundCheck.transform.position, groundCheck.checkRadius, groundCheck.groundLayer);

        if (hit != null){
            PlatformType platformType = hit.GetComponentInParent<PlatformType>();
            PlatformEffector2D eff = hit.GetComponentInParent<PlatformEffector2D>();

            if (platformType != null && platformType.allowDropThrough && eff != null){
                float originalOffset = eff.rotationalOffset;
                eff.rotationalOffset = 180f;
                yield return new WaitForSeconds(dropThroughTime);
                eff.rotationalOffset = originalOffset;
            }
        }

        yield return new WaitForSeconds(0.05f);
        isDropping = false;
    }

    // 旧：複雑な物理跳ね返り抑制は撤廃し安定重視（必要なら後で再導入）
    private void PreventUnintendedJump(){
        // Intentionally left minimal to avoid cutting valid jumps.
    }

    // シンプルな坂道移行制御
    private void UpdateSlopeTransition(){
        if (slopeTransitionBuffer > 0f){
            slopeTransitionBuffer -= Time.deltaTime;
            if (slopeTransitionBuffer <= 0f){
                slopeTransitionBuffer = 0f;
                Debug.Log("坂道移行バッファ終了");
            }
        }
    }

    void OnDrawGizmosSelected(){
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheck.checkRadius);
        Gizmos.DrawLine(groundCheck.transform.position, groundCheck.transform.position + (Vector3)(-groundCheck.GroundNormal * 0.5f));
    }
}
