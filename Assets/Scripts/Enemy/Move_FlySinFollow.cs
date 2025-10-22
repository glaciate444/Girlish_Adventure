using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/FlySinFollow")]
public class Move_FlySinFollow : MoveBehaviorSO {
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private float turnSpeed = 2f; // ’Ç”ö‚Ì•ûŒü•âŠÔ‘¬“x

    public override void Initialize(BaseEnemy enemy, MoveState state){
        state.timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public override void Move(BaseEnemy enemy, MoveState state){
        var rb = enemy.Rb;

        // ---- ’Ç”öˆ— ----
        Vector2 moveDir = enemy.MoveDirection;
        if (followPlayer && enemy.Player != null){
            Vector2 targetDir = (enemy.Player.position - enemy.transform.position).normalized;
            moveDir = Vector2.Lerp(moveDir, targetDir, Time.fixedDeltaTime * turnSpeed);
            enemy.MoveDirection = moveDir;
        }

        // ---- Sin”g”òs ----
        float y = Mathf.Sin(Time.time * frequency + state.timeOffset) * amplitude;
        rb.linearVelocity = new Vector2(moveDir.x * enemy.MoveSpeed, y);
    }
    public override MoveState CreateState(){
        var s = base.CreateState();
        s.timeOffset = Random.Range(0f, Mathf.PI * 2f);
        s.timer = 0f;
        return s;
    }
}
