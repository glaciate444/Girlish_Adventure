using UnityEngine;

public class WorldMapInitializer : MonoBehaviour {
    private void Start(){
        var fader = FindAnyObjectByType<SceneFader>();
        if (fader != null)
            StartCoroutine(fader.FadeIn());
    }
}