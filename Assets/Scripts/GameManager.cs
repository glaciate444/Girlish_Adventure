/* =======================================
 * ファイル名 : GameManager.cs
 * 概要 : ゲームメインスクリプト
 * Create Date : 2025/10/01
 * Date : 2025/10/24
 * Version : 0.04
 * 更新内容 : Presistent対応
 * ======================================= */
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    private UIManager uiManager;
    public int CurrentStage { get; private set; }

    [SerializeField] private SoundManager soundManager; // InspectorでPersistent上のSoundManager参照
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

    // ===== シーン遷移 =====

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

    // ===== サウンド操作 =====

    public void PlayBGM(int bgmID){
        if (soundManager != null)
            soundManager.PlayBGM(bgmID);
        else
            Debug.LogWarning("SoundManager が未設定です。Persistent シーンに配置してください。");
    }

    public void StopBGM(){
        soundManager?.StopBGM();
    }

    public void PlaySE(int seID){
        soundManager?.PlaySE(seID);
    }
}
