using UnityEngine.SceneManagement;
using UnityEngine;

public static class SceneEvents{
    public static event System.Action OnSceneChanged;

    static SceneEvents(){
        SceneManager.sceneLoaded += (_, __) => OnSceneChanged?.Invoke();
    }
}
