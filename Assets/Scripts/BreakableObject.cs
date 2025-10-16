using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : MonoBehaviour {
    [Header("破壊後に消える対象（このオブジェクトを含むことも可能）")]
    public GameObject targetObject;

    [Header("破壊エフェクト（ParticleSystemプレハブ）")]
    public ParticleSystem breakEffectPrefab;

    [Header("ドロップアイテム（任意）")]
    public GameObject dropItemPrefab;

    [Header("壊れた状態のスプライト（任意）")]
    public Sprite brokenSprite;

    private SpriteRenderer spriteRenderer;
    private bool isBroken = false;

    [Header("イベント（任意）")]
    public UnityEvent onBreak;

    [Header("破壊後にオブジェクトを消すまでの遅延時間")]
    public float destroyDelay = 0.1f;

    private int testcnt = 0;


    private void Awake(){
        if (targetObject == null)
            targetObject = this.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other){
        Debug.Log($"isBroken -> {isBroken}");

        // すでに壊れているならスキップ
        if (isBroken) return;

        // 攻撃タグのみ反応
        if (other.CompareTag("PlayerAttack")){
            // 最初にロックする！（ここが重要）
            isBroken = true;
            Break();
        }
    }

    private void Break(){
        // パーティクル再生
        if (breakEffectPrefab){
            // 自分自身を誤って指定していないかチェック
            if (breakEffectPrefab.gameObject == this.gameObject){
                Debug.LogError("❌ breakEffectPrefab に自分自身が指定されています！正しい ParticleSystem プレハブを指定してください。");
                return;
            }

            ParticleSystem effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            effect.Play();

            float destroyTime = effect.main.duration + effect.main.startLifetime.constantMax;
            Destroy(effect.gameObject, destroyTime);
        }

        // ドロップ生成
        if (dropItemPrefab){
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(targetObject, 0.1f);
    }
}