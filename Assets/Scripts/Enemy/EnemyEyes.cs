using UnityEngine;

public class EnemyEyes : MonoBehaviour{
    [SerializeField] private GameObject player;

    // Update is called once per frame
    void Update(){
        Vector3 dir = (player.transform.position - transform.position);
        transform.rotation = Quaternion.FromToRotation(Vector2.left, dir);
    }
}
