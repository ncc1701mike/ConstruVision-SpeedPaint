using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game_UI : MonoBehaviour
{
    [Header("Instruction Elements")]
    [SerializeField] private TMP_Text floorInstructionText;
    [SerializeField] private TMP_Text ceilingInstructionText;
    [SerializeField] private TMP_Text wallInstructionText;

    [Header("Paint Calculator Elements")]
    [SerializeField] private PaintCalculator paintCalculator;
    [SerializeField] private GameObject dropDown;
    [SerializeField] private GameObject slider;
    [SerializeField] private GameObject addZoneButton;
    [SerializeField] private TextMeshProUGUI dimensionsText;
    [SerializeField] private TextMeshProUGUI areaText;
    [SerializeField] private TextMeshProUGUI totalAreaText;
    [SerializeField] private TextMeshProUGUI paintAmount;
    [SerializeField] private TextMeshProUGUI totalCumulativeAreaText;
    public GameObject uiElements;
    public GameObject panel;
    public TMP_Dropdown zoneDropdown;
    public Slider zoneSlider;

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

    public void HideInstructions()
    {
        floorInstructionText.gameObject.SetActive(false);
        ceilingInstructionText.gameObject.SetActive(false);
        wallInstructionText.gameObject.SetActive(false);
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

    public void CreateBackgroundPanel()
    {
        // Create a GameObject for the panel
        GameObject panelObject = new GameObject("BackgroundPanel");
        panelObject.transform.SetParent(uiElements.transform, false); // Set the new panel as a child of the UI container

        // Add and configure the Image component to make the background semi-translucent
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 1, 0.5f); // Semi-translucent blue

        // Configure RectTransform to stretch the entire canvas and anchor to the left
        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.sizeDelta = new Vector2(200, 0); // Width of 200 pixels and height to stretch the parent

        // Set the TextMeshPro components as children of the panel
        dimensionsText.transform.SetParent(panelObject.transform, false);
        areaText.transform.SetParent(panelObject.transform, false);
        totalAreaText.transform.SetParent(panelObject.transform, false);
    }

    public void SetSliderValue(float value)
    {
        zoneSlider.value = value;
    }

    public void UpdateTotalAreaText(float newArea)
    {
        totalAreaText.text = $"Room Area: {newArea:F2} ft^2";
    }
    public void UpdateCumulativeAreaText(float cumulativeArea)
    {
        totalCumulativeAreaText.text = $"Total Area: {cumulativeArea:F2} ft^2";
    }
    public void UpdateWallsDimensionsText(string width, string height, float area)
    {
        dimensionsText.text = $"Width: {width} Height: {height}";
        areaText.text = $"Area: {area:F2} ft^2";
    }

    #endregion





}
