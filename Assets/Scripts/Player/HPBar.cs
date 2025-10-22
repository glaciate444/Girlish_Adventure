using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueHPText;
    [SerializeField] private TMP_Text maxHPText;


    public void SetHP(int hp, int maxHP){
        if (slider != null){
            slider.maxValue = maxHP;
            slider.value = hp;
        }

        if (valueHPText != null) valueHPText.text = hp.ToString();
        if (maxHPText != null) maxHPText.text = maxHP.ToString();
    }
}