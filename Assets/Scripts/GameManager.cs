/* =======================================
 * �t�@�C���� : GameManager.cs
 * �T�v : �Q�[�����C���X�N���v�g
 * Create Date : 2025/10/01
 * Date : 2025/10/24
 * Version : 0.04
 * �X�V���e : Presistent�Ή�
 * ======================================= */
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    private UIManager uiManager;
    public int CurrentStage { get; private set; }

    [SerializeField] private SoundManager soundManager; // Inspector��Persistent���SoundManager�Q��
    public SoundManager Sound => soundManager;
    public UIManager UI => uiManager;
    private void Awake(){
        if (Instance != null){
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterUI(UIManager ui){
        uiManager = ui;
    }

    public void UnregisterUI(){
        uiManager = null;
    }

    // ===== �V�[���J�� =====

    public void LoadStage(int stageNumber){
        CurrentStage = stageNumber;
        SceneManager.LoadSceneAsync($"Stage_{stageNumber}");
    }

    public void ReturnToWorldMap(){
        SceneManager.LoadSceneAsync("WorldMap");
    }

    public void GoToTitle(){
        SceneManager.LoadSceneAsync("Title");
    }

    // ===== �T�E���h���� =====

    public void PlayBGM(int bgmID){
        if (soundManager != null)
            soundManager.PlayBGM(bgmID);
        else
            Debug.LogWarning("SoundManager �����ݒ�ł��BPersistent �V�[���ɔz�u���Ă��������B");
    }

    public void StopBGM(){
        soundManager?.StopBGM();
    }

    public void PlaySE(int seID){
        soundManager?.PlaySE(seID);
    }
}
