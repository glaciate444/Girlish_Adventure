using UnityEngine;

public class Projectile : MonoBehaviour{
    private Vector2 direction;
    private int damage;
    private float speed;

    public float lifeTime = 3f;

    public void Setup(Vector2 dir, int dmg, float spd){
        direction = dir.normalized;
        damage = dmg;
        speed = spd;
        Destroy(gameObject, lifeTime);
    }

    void Update(){
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Enemy"))
        {
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject); // àÍìxìñÇΩÇ¡ÇΩÇÁè¡Ç¶ÇÈ
        }else if (other.gameObject.layer == LayerMask.NameToLayer("Ground")){
            Destroy(gameObject);
        }
    }
}
