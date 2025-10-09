using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour{
    [Header("UI設定")]
    [SerializeField] private Slider slider;
    [SerializeField] private PlayerController player;
    private int maxHP;

    void Start(){
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();

        // スライダーの初期設定
        slider.maxValue = player.hp;
        slider.value = player.hp;
    }

    // Update is called once per frame
    void Update(){
        slider.value = player.hp;
    }
}
