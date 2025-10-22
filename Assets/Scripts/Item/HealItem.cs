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