using UnityEngine;

public class HealItem : MonoBehaviour {
    [SerializeField] private int healAmount = 2;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            var player = other.GetComponent<PlayerController>();
            if (player != null){
                player.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}