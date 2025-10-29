using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour{
    [SerializeField, Header("振動する時間")]
    private float shakeTime = 0.3f;
    [SerializeField, Header("振動の大きさ")]
    private float shakeMagnitude = 0.5f;

    [SerializeField] private PlayerController player;
    private Vector3 initPos;
    private int lastPlayerHP;

    private void Start(){
        initPos = transform.position;
        FindPlayer(); // 起動時に探す
    }

    private void OnEnable(){
        SceneEvents.OnSceneChanged += FindPlayer; // シーン切り替え時に再検出
    }

    private void OnDisable(){
        SceneEvents.OnSceneChanged -= FindPlayer;
    }

    private void FindPlayer(){
        player = FindAnyObjectByType<PlayerController>();

        if (player != null){
            lastPlayerHP = player.GetHP();
            Debug.Log($"[CameraManager] Player再設定: {player.name}");

            // ダメージ時の振動登録
            player.OnDamage += () =>
                transform.DOShakePosition(shakeTime, shakeMagnitude)
                         .OnComplete(() => transform.position = initPos);
        }else{
            Debug.Log("[CameraManager] Playerが見つからなかったため、静止モードで待機。");
        }
    }

    private void Update(){
        if (player == null){
            // タイトル画面などプレイヤーが存在しない時は初期位置固定
            transform.position = new Vector3(initPos.x, initPos.y, transform.position.z);
            return;
        }

        ShakeCheck();
        FollowPlayer();
    }

    private void ShakeCheck(){
        if (player == null) return;

        int currentHP = player.GetHP();
        if (currentHP < lastPlayerHP){
            transform.DOShakePosition(shakeTime, shakeMagnitude)
                     .OnComplete(() => transform.position = initPos);
        }
        lastPlayerHP = currentHP;
    }

    private void FollowPlayer(){
        if (player == null) return;

        Vector3 pos = transform.position;
        pos.x = player.transform.position.x;
        transform.position = pos;
    }
}
