using UnityEngine;

[System.Serializable]
public class Move_FlySin : IMoveBehavior{
    private float timeOffset = Random.Range(0f, Mathf.PI * 2);
    public float amplitude = 0.5f;
    public float frequency = 2f;

    public void Move(BaseEnemy enemy){
        float y = Mathf.Sin(Time.time * frequency + timeOffset) * amplitude;
        enemy.Rb.linearVelocity = new Vector2(enemy.MoveDirection.x * enemy.MoveSpeed, y);
    }
}
