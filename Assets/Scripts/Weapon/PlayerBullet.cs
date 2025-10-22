using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBullet : BaseBullet {
    [SerializeField] private float speed = 10f;
    private Vector2 direction;
    private Rigidbody2D rb;

    private bool initialized = false;

    protected override void Start(){
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // ’e‚ª—‰º‚µ‚È‚¢‚æ‚¤‚É
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    public void Setup(Vector2 dir){
        direction = dir.normalized;
        targetTag = "Enemy";
        initialized = true;

        // Rigidbody‚ª‚Ü‚¾æ“¾‚³‚ê‚Ä‚¢‚È‚¢ê‡‚à‚ ‚é‚½‚ßˆÀ‘S‚ÉŒÄ‚Ô
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Unity 6‚Å‚Í‚±‚±‚Å’¼Ú‘ã“ü‚µ‚Ä‚àOK
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate(){
        // ”O‚Ì‚½‚ßA”­ËŒã‚à‘¬“x‚ğˆÛ‚·‚é
        if (initialized && rb.linearVelocity.sqrMagnitude < 0.01f){
            rb.linearVelocity = direction * speed;
        }
    }

    protected override void HitTarget(Collider2D target){
        var enemy = target.GetComponent<BaseEnemy>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }
}
