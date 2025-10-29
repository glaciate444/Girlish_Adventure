/* =======================================
 * �t�@�C���� : BootLoader.cs
 * �T�v : �Q�[���N������Persistent�����[�h���ď풓������X�N���v�g
 * Date : 2025/10/29
 * Version : 0.01
 * ======================================= */
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour {
    private void Start(){
        // ���Ƀ��[�h�ς݂Ȃ�X�L�b�v
        if (!SceneManager.GetSceneByName("Persistent").isLoaded){
            SceneManager.LoadSceneAsync("Persistent", LoadSceneMode.Additive);
        }
    }
}