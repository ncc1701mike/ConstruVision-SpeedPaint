using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game_UI : MonoBehaviour
{
    [Header("Instruction Elements")]
    [SerializeField] private TMP_Text floorInstructionText;
    [SerializeField] private TMP_Text ceilingInstructionText;
    [SerializeField] private TMP_Text wallInstructionText;

    [Header("Paint Calculator Elements")]
    [SerializeField] private GameObject dropDown;
    [SerializeField] private GameObject slider;
    [SerializeField] private GameObject addZoneButton;

    #region Instruction Methods
    public void ShowFloorInstructions()
    {
        floorInstructionText.gameObject.SetActive(true);
        ceilingInstructionText.gameObject.SetActive(false);
        wallInstructionText.gameObject.SetActive(false);
    }

    public void ShowCeilingInstructions()
    {
        floorInstructionText.gameObject.SetActive(false);
        ceilingInstructionText.gameObject.SetActive(true);
        wallInstructionText.gameObject.SetActive(false);
    }

    public void ShowWallInstructions()
    {
        floorInstructionText.gameObject.SetActive(false);
        ceilingInstructionText.gameObject.SetActive(false);
        wallInstructionText.gameObject.SetActive(true);
    }

    #endregion

    #region Paint Calculator Methods
    public void ShowPaintCalculator()
    {
        dropDown.SetActive(true);
        slider.SetActive(true);
        addZoneButton.SetActive(true);
    }

    public void HidePaintCalculator()
    {
        dropDown.SetActive(false);
        slider.SetActive(false);
        addZoneButton.SetActive(false);
    }

    #endregion




}
