using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderDisplay : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text sliderDisplay;
    [SerializeField] PlaneManager planeManager;


    
    void Start()
    {
       if (slider != null)
       {
           slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
       } 
    }

    public void ValueChangeCheck()
    {
        int value = Mathf.RoundToInt(slider.value/10)*10;
        if (sliderDisplay != null)
        {
            sliderDisplay.text = value.ToString();
        }

        if (planeManager != null)
        {
            planeManager.CalculateAndDispayResults();
        }
    }

}
