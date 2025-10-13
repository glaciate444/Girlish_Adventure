using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject{
    public string enemyName;
    public float moveSpeed;
    public int maxHP;
    public int attackPower;

}
