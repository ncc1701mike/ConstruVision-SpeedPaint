using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.XR.ARSubsystems;

public class PlaneManager : MonoBehaviour
{
    public TextMeshProUGUI dimensionsText, areaText, totalAreaText, paintAmount, totalCumulativeAreaText;
    public GameObject uiElements;
    public GameObject panel;
    public TMP_Dropdown zoneDropdown;
    public Slider zoneSlider; 
    
    private const float metersToFt = 3.28084f;
    private float cumulativeArea = 0.0f;
    private float sliderValue = 0.0f;
    private HashSet<ARPlane> selectedPlanes = new HashSet<ARPlane>();
    private Dictionary<ARPlane, float> planeAreas = new Dictionary<ARPlane, float>();
    private List<float> zoneAreas = new List<float>();
    private List<float> zonePaintCalculation = new List<float>(); 
    private List<float> zoneSliderValue = new List<float>(); 
    private int currentZoneIndex = 0;
    
    void Start()
    {
        zoneAreas.Add(0.0f);
        zonePaintCalculation.Add(0.0f); 
        zoneSliderValue.Add(0.0f); 
        UpdateZoneDropdown();
        uiElements.SetActive(false);
        CreateBackgroundPanel();
    }

     public void ShowPlaneSize(ARPlane plane)
    {
        if (!selectedPlanes.Contains(plane))
        {
            selectedPlanes.Add(plane);
            panel.SetActive(true);
            uiElements.SetActive(true);

            var lineRenderer = plane.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }

            // Convert size and update area
            float widthInFeet = plane.size.x * metersToFt;
            float heightInFeet = plane.size.y * metersToFt;
            float areaInSquareFeet = widthInFeet * heightInFeet;

            // Update Area Calculations
             UpdateZoneArea(areaInSquareFeet);
             cumulativeArea += areaInSquareFeet;
             totalCumulativeAreaText.text = $"Total Area: {cumulativeArea:F2} ft^2";

            // Update Dictionary & UI 
            planeAreas[plane] = areaInSquareFeet;
            UpdateUI(widthInFeet, heightInFeet, areaInSquareFeet);
            CalculateAndDispayResults();

            // Update Dropdown & Paint Amount UI
            if(!uiElements.activeSelf)
            {
                InitializeSliderAndPaintAmount();
            }
        }
    }

    public void InitializeSliderAndPaintAmount()
    {
        zoneSlider.value = 0.0f;

        var sliderDisplayScript = zoneSlider.GetComponent<SliderDisplay>();
        if (sliderDisplayScript != null)
        {
            sliderDisplayScript.ValueChangeCheck();
        }

        paintAmount.text = "Adjust slider for paint needed";
    }

    public void RemovePlaneSize(ARPlane plane)
    {
        if (selectedPlanes.Contains(plane))
        {
            selectedPlanes.Remove(plane);

            var lineRenderer = plane.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            if (planeAreas.TryGetValue(plane, out float areaToRemove))
            {
                planeAreas.Remove(plane);
                UpdateZoneArea(-areaToRemove);
                cumulativeArea -= areaToRemove;

                totalAreaText.text = $"Room Area: {zoneAreas[currentZoneIndex]:F2} ft^2";
                totalCumulativeAreaText.text = $"Total Area: {cumulativeArea:F2} ft^2";
                UpdateUIForSelectedZone();
                CalculateAndDispayResults();
            }

        }
    }


    public void UpdatePlaneSize(ARPlane plane, Vector2 newSize)
    {
        float newWidthInFeet = newSize.x * metersToFt;
        float newHeightInFeet = newSize.y * metersToFt;
        float newAreaInSquareFeet = newWidthInFeet * newHeightInFeet;

        if (planeAreas.TryGetValue(plane, out float oldArea))
        {

            float areaDifference = newAreaInSquareFeet - oldArea;

            zoneAreas[currentZoneIndex] += areaDifference;
            cumulativeArea += areaDifference;

            // Update plane tracking
            planeAreas[plane] = newAreaInSquareFeet;

            // Update UI
            totalAreaText.text = $"Room Area: {zoneAreas[currentZoneIndex]:F2} ft^2";
            totalCumulativeAreaText.text = $"Total Area: {cumulativeArea:F2} ft^2";

            if (selectedPlanes.Contains(plane))
            {
                UpdateUI(newWidthInFeet, newHeightInFeet, newAreaInSquareFeet);
            }

            CalculateAndDispayResults();
        }
    }

    private void UpdateZoneArea(float newArea)
    {
        zoneAreas[currentZoneIndex] += newArea;
        totalAreaText.text = $"Room Area: {zoneAreas[currentZoneIndex]:F2} ft^2";
        totalCumulativeAreaText.text = $"Total Area: {cumulativeArea:F2} ft^2";
    }

    private void UpdateUI(float width, float height, float area)
    {
        dimensionsText.text = $"Width: {ConvertToFeetAndInches(width)}'  Height: {ConvertToFeetAndInches(height)}'";
        areaText.text = $"Wall Area: {area:F2} ft^2";
    }


    public void AddZone()
    {
        zoneAreas.Add(0.0f);
        zonePaintCalculation.Add(0.0f);
        zoneSliderValue.Add(0.0f);
        currentZoneIndex = zoneAreas.Count - 1;
        UpdateZoneDropdown();
    }


    public void OnZoneDropdownChanged(int newIndex)
    {
        if (newIndex >= 0 && newIndex < zoneAreas.Count)
        {
            currentZoneIndex = newIndex;
            ClearUI();
            UpdateUIForSelectedZone();

            if (currentZoneIndex < zonePaintCalculation.Count)
            {
                zoneSlider.value = zoneSliderValue[currentZoneIndex];
            }
            else
            {
                ResetSlider();
            }
        }
    }


    private void UpdateZoneDropdown()
    {
        zoneDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < zoneAreas.Count; i++)
        {
            options.Add($"Room {i + 1}");
        }

        zoneDropdown.AddOptions(options);
        zoneDropdown.value = currentZoneIndex; 
    }


    private void UpdateUIForSelectedZone()
    {
        totalAreaText.text = $"Room Area: {zoneAreas[currentZoneIndex]:F2} ft^2";
        totalCumulativeAreaText.text = $"Total Area: {cumulativeArea:F2} ft^2";

        if (currentZoneIndex < zonePaintCalculation.Count)
        {
            paintAmount.text = $"Paint Needed: {zonePaintCalculation[currentZoneIndex]:F2} gallons";
        }
        else
        {
            paintAmount.text = "Enter Paint Coverage";
        }
    }


    public void ClearUI()
    {
        panel.SetActive(false);
        dimensionsText.text = "";
        areaText.text = "";
        totalAreaText.text = "";
    }


    public void CalculateAndDispayResults()
    {
        float zoneArea = zoneAreas[currentZoneIndex];
        sliderValue = zoneSlider.value;
        float paintNeeded = zoneArea / sliderValue * 1.1f;

        if (sliderValue == 0.0f)
        {
            paintAmount.text = "Adjust slider for paint needed";
            return;
        }

        paintAmount.text = $"Paint Needed: {paintNeeded:F2} gallons";

        if (currentZoneIndex < zonePaintCalculation.Count)
        {
            zonePaintCalculation[currentZoneIndex] = paintNeeded;
            zoneSliderValue[currentZoneIndex] = sliderValue;
        }
        else
        {
            zonePaintCalculation.Add(paintNeeded);
            zoneSliderValue.Add(sliderValue);
        }
    }


    private void ResetSlider()
    {
        sliderValue = 0.0f;
        zoneSlider.value = 0.0f;
        paintAmount.text = "Adjust slider to calculate paint needed";
    }


    private string ConvertToFeetAndInches(float measurementInFeet)
    {
        int feet = Mathf.FloorToInt(measurementInFeet);
        float inches = (measurementInFeet - feet) * 12.0f;
        return $"{feet}' {inches:F2}\"";
    }


    private void CreateBackgroundPanel()
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

}



