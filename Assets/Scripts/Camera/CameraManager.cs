using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour{
    [SerializeField, Header("振動する時間")]
    private float shakeTime;
    [SerializeField, Header("振動の大きさ")]
    private float shakeMagnitude;

    [SerializeField] private PlayerController player;

    private Vector3 initpos;
    private float shakeCount;
    private int currentPlayerHP;

    void Start(){
        //player = FindObjectOfType<PlayerController>();
        currentPlayerHP = player.GetHP();
        initpos = transform.position;
    }
    private void Update(){
        ShakeCheck();
        FollowPlayer();
    }
    // HPが減ると振動させる(将来的にDoTweenで調整)
    private void ShakeCheck(){
        if(currentPlayerHP > player.GetHP()){
            currentPlayerHP = player.GetHP();
            shakeCount = 0.0f;
            StartCoroutine(Shake());
        }
    }
    // 振動時間(将来的にDoTweenで調整)
    IEnumerator Shake(){
        Vector3 initpos = transform.position;
        while(shakeCount < shakeTime){
            float x = initpos.x + Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = initpos.y + Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.position = new Vector3(x, y, initpos.z);

            shakeCount += Time.deltaTime;
            yield return null;
        }
        transform.position = initpos;
    }
    //カメラ追従(将来的にChinemachineCameraで調節)
    private void FollowPlayer(){
        float x = player.transform.position.x;
        x = Mathf.Clamp(x, initpos.x, Mathf.Infinity);
        transform.position = new Vector3(x,transform.position.y, transform.position.z); 
    }

}
