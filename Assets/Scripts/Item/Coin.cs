using UnityEngine;

public class Coin : MonoBehaviour {
    [SerializeField] private int addCoins = 1; // ���l�̓C���X�y�N�^�[��

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            // �R�C���擾��ʒm
            CoinManager.Instance.AddCoins(addCoins);

            // �R�C���I�u�W�F�N�g���폜
            Destroy(gameObject);
        }
    }
}