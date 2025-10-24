using UnityEngine;

public class HitEffectSpawner : MonoBehaviour {
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float lifetime = 0.4f;

    public void SpawnHitEffect(Vector3 position){
        if (hitEffectPrefab == null){
            Debug.LogError("HitEffect prefab is not assigned.");
            return;
        }

        // エフェクト生成
        var effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

        // Animator.Playで直接再生（レイヤー0の"HitFlash"）
        var animator = effect.GetComponent<Animator>();
        animator.Play("HitFlash", 0, 0f);

        // 寿命後に自動破棄
        Destroy(effect, lifetime);
    }
}
