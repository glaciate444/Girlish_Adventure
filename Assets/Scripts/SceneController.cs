using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    public static void LoadScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadSceneAsync(string sceneName){
        SceneManager.LoadSceneAsync(sceneName);
    }

    public static void LoadAdditive(string sceneName){
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public static void UnloadScene(string sceneName){
        SceneManager.UnloadSceneAsync(sceneName);
    }
}