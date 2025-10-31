/* =======================================
 * �t�@�C���� : WorldMapInitializer.cs
 * �T�v : �Q�[�����C���X�N���v�g
 * Create Date : 2025/10/01
 * Date : 2025/10/24
 * Version : 0.04
 * �X�V���e : Presistent�Ή�
 * ======================================= */
using UnityEngine;

public class WorldMapInitializer : MonoBehaviour {
    private void Start(){
        var fader = FindAnyObjectByType<SceneFader>();
        if (fader != null)
            StartCoroutine(fader.FadeIn());
    }
}