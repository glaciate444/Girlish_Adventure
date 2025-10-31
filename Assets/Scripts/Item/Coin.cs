/* =======================================
 * �t�@�C���� : Coin.cs
 * �T�v : �R�C���擾�X�N���v�g
 * Created Date : 2025/10/14
 * Date : 2025/10/14
 * Version : 0.01
 * �X�V���e : �V�K�쐬
 * ======================================= */
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