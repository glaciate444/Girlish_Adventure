using UnityEngine;

public class EnemySpawner : MonoBehaviour{
    [SerializeField, Header("敵オブジェクト")]
    private GameObject enemy;

    [SerializeField] private PlayerController player;
    private GameObject enemyObj;


    void Start(){
        player = FindObjectOfType<PlayerController>();
        enemyObj = null;
    }

    void Update(){
    }

    private void SpawnEnemy(){
        if (player != null) return;
    
        Vector3 playerPos = player.transform.position;
        Vector3 cameraMaxPos = Camera.main.ScreenToViewportPoint(new Vector3(Screen.width, Screen.height));
        Vector3 scale = enemy.transform.lossyScale;

        float distance = Vector2.Distance(transform.position, new Vector2(playerPos.x, transform.position.y));
        float spawnDis = Vector2.Distance(playerPos, new Vector2(cameraMaxPos.x + scale.x / 2.0f, playerPos.y));

        if(distance <= spawnDis && enemyObj == null){
            enemyObj= Instantiate(enemy);
            enemyObj.transform.position = transform.position;
            transform.parent = enemyObj.transform;
        }
    }
}
