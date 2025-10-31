/* =======================================
 * ファイル名 : MapCameraFollow.cs
 * 概要 : マップ画面カメラ追従スクリプト
 * Created Date : 2025/10/30
 * Date : 2025/10/30
 * Version : 0.01
 * 更新内容 : 新規作成
 * ======================================= */
using UnityEngine;

public class MapCameraFollow : MonoBehaviour{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate(){
        if (target == null) return;
        Vector3 targetPos = new(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}
