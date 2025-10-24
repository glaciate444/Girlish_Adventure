/* =======================================
 * ファイル名 : GameManager.cs
 * 概要 : ゲームメインスクリプト
 * Date : 2025/10/21
 * Version : 0.01
 * ======================================= */
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject uiPrefab;
    public enum GameState { Playing, Cleared, GameOver }
    public GameState CurrentState { get; private set; }

    private void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (UIManager.Instance == null && uiPrefab != null){
                var ui = Instantiate(uiPrefab);
                DontDestroyOnLoad(ui);
            }
        }else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        SetState(GameState.Playing);
    }

    public void SetState(GameState state){
        CurrentState = state;
        Debug.Log($"GameState: {state}");

        switch (state){
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Cleared:
                Time.timeScale = 0f;
                UIManager.Instance?.ShowClearUI();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                // UIManager.Instance?.ShowGameOverUI();
                break;
        }
    }

    public void RetryStage(){
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToNextStage()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextIndex);
    }
}