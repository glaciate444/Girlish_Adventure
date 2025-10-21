/* =======================================
 * ファイル名 : BaseBullet.cs
 * 概要 : 弾の基底クラススクリプト
 * Date : 2025/10/22
 * Version : 0.01
 * ※プレイヤー用弾（PlayerBullet）を作る場合は targetTag = "Enemy" に変更すればOK。
 * ======================================= */
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseBullet : MonoBehaviour{
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float lifeTime = 5f;
    [SerializeField] protected string targetTag = "Player"; // デフォルトは敵弾向け

    protected virtual void Start(){
        Destroy(gameObject, lifeTime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag(targetTag)){
            HitTarget(other);
            Destroy(gameObject);
        }else if (other.CompareTag("Ground")){
            Destroy(gameObject);
        }
    }

    protected virtual void HitTarget(Collider2D target){
        // 派生クラス側で具体的処理を実装
    }

    protected virtual void OnBecameInvisible(){
        Destroy(gameObject);
    }
}
