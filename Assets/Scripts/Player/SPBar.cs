using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SPBar : MonoBehaviour{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueSPText;
    [SerializeField] private TMP_Text maxSPText;


    public void SetSP(int sp, int maxSP){
        if (slider != null){
            slider.maxValue = maxSP;
            slider.value = sp;
        }

        if (valueSPText != null) valueSPText.text = sp.ToString();
        if (maxSPText != null) maxSPText.text = maxSP.ToString();
    }
}