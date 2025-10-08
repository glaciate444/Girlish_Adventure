using UnityEngine;

public class EnemyManager : MonoBehaviour{
    [Header("�ړ����x")]
    public float moveSpeed;
    [Header("�G�̍U����")]
    public int attackPower;

    private Rigidbody2D rb;
    private void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update(){
        Move();
    }

    private void Move(){
        rb.linearVelocity = new Vector2(Vector2.left.x * moveSpeed, rb.linearVelocity.y);
    }
    public void PlayerDamage(PlayerController player){
        player.Damage(attackPower);
    }
}
