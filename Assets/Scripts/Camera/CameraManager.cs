using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour {
    [SerializeField, Header("振動する時間")]
    private float shakeTime = 0.3f;
    [SerializeField, Header("振動の大きさ")]
    private float shakeMagnitude = 0.5f;
    [SerializeField] private PlayerController player;

    private Vector3 initPos;
    private int lastPlayerHP;

    private void Start(){
        initPos = transform.position;
        lastPlayerHP = player != null ? player.GetHP() : 0;
        
        if (player != null){
            player.OnDamage += () =>
                transform.DOShakePosition(shakeTime, shakeMagnitude)
                         .OnComplete(() => transform.position = initPos);
        }
        else{
            Debug.LogError("CameraManager: Player reference is not assigned! Please assign the Player in the Inspector.");
        }
    }

    void Update(){
        if (player == null){
            // プレイヤー消滅後のフォールバック処理
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

        transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);

    }
}
