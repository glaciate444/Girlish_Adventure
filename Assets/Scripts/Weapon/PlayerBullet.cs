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
        rb.gravityScale = 0; // �e���������Ȃ��悤��
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    public void Setup(Vector2 dir){
        direction = dir.normalized;
        targetTag = "Enemy";
        initialized = true;

        // Rigidbody���܂��擾����Ă��Ȃ��ꍇ�����邽�߈��S�ɌĂ�
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Unity 6�ł͂����Œ��ڑ�����Ă�OK
        rb.linearVelocity = direction * speed;
    }

    private void FixedUpdate(){
        // �O�̂��߁A���ˌ�����x���ێ�����
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
