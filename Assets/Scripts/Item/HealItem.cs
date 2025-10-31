/* =======================================
 * �t�@�C���� : HealItem.cs
 * �T�v : �񕜃A�C�e���擾�X�N���v�g
 * Created Date : 2025/10/14
 * Date : 2025/10/14
 * Version : 0.01
 * �X�V���e : �V�K�쐬
 * ======================================= */
using UnityEngine;

public class HealItem : MonoBehaviour {
    [SerializeField] private HealItemData healItemData;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            var player = other.GetComponent<PlayerController>();
            if (player != null){
                healItemData.Apply(player);
                Destroy(gameObject);
            }
        }
    }
}