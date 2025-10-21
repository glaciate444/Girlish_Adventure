using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/MoveBehavior/FlySin")]
public class Move_FlySin : MoveBehaviorSO {
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float frequency = 2f;

    public override void Initialize(BaseEnemy enemy, MoveState state){
        // Še“G‚Åƒ‰ƒ“ƒ_ƒ€‚ÈˆÊ‘Š‚ğİ’è
        state.timeOffset = Random.Range(0f, Mathf.PI * 2);
    }

    public override void Move(BaseEnemy enemy, MoveState state){
        float y = Mathf.Sin(Time.time * frequency + state.timeOffset) * amplitude;
        enemy.Rb.linearVelocity = new Vector2(enemy.MoveDirection.x * enemy.MoveSpeed, y);
    }
    public override MoveState CreateState(){
        var s = base.CreateState();
        s.timeOffset = Random.Range(0f, Mathf.PI * 2f);
        return s;
    }
}
