using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField, Header("エネミー")]
    private GameObject enemy;

    [SerializeField] private PlayerController player;
    private GameObject enemyObj;
    private Camera mainCamera;

    void Awake(){
        mainCamera = Camera.main;
    }

    void Start(){
        enemyObj = null;
    }

    void Update(){
        SpawnEnemy();
    }

    private void SpawnEnemy(){
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        Vector3 worldMaxPos = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.nearClipPlane));
        Vector3 scale = enemy.transform.lossyScale;

        float distance = Vector2.Distance(transform.position, new Vector2(playerPos.x, transform.position.y));
        float spawnDis = Vector2.Distance(playerPos, new Vector2(worldMaxPos.x + scale.x / 2.0f, playerPos.y));

        if (distance <= spawnDis && enemyObj == null){
            enemyObj = Instantiate(enemy, transform.position, Quaternion.identity);
            transform.SetParent(enemyObj.transform);
        }
    }
}