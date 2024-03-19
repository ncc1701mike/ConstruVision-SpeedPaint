using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintCalculator : MonoBehaviour
{
    [SerializeField] PlaneController planeController;
    [SerializeField] Game_UI game_UI;

    [SerializeField] private float cumulativeArea = 0.0f;

    private List<float> zoneAreas = new List<float>();
    private List<float> zonePointCalculation = new List<float>();
    private List<float> zoneSliderValue = new List<float>();

    private int currentZoneIndex = 0;

    private const float metersToFt = 3.28084f;

    private void Start()
    {
        zoneAreas.Add(0.0f);
        zonePointCalculation.Add(0.0f);
        zoneSliderValue.Add(0.0f);
        UpdateZoneDropdown();
        game_UI.uiElements.SetActive(false);
        game_UI.CreateBackgroundPanel();
    }

    private string ConvertToFeetAndInches(float measurementInFeet)
    {
        int feet = Mathf.FloorToInt(measurementInFeet);
        float inches = (measurementInFeet - feet) * 12.0f;
        return $"{feet}' {inches:F2}\"";
    }

    private void UpdateZoneDropdown()
    {
        game_UI.zoneDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < zoneAreas.Count; i++)
        {
            options.Add($"Room {i + 1}");
        }

        game_UI.zoneDropdown.AddOptions(options);
        game_UI.zoneDropdown.value = currentZoneIndex;
    }

    private void UpdateZoneArea(float newArea)
    {
        zoneAreas[currentZoneIndex] += newArea;
        game_UI.UpdateTotalAreaText(zoneAreas[currentZoneIndex]);
    }

    private void CalculatePlaneSize(ReadPlane readPlane)
    {
        float widthInFeet = readPlane.plane.size.x * metersToFt;
        float heightInFeet = readPlane.plane.size.y * metersToFt;
        float areaInSquareFeet = widthInFeet * heightInFeet;

        UpdateZoneArea(areaInSquareFeet);

        cumulativeArea += areaInSquareFeet;
        game_UI.UpdateCumulativeAreaText(cumulativeArea);

        game_UI.UpdateWallsDimensionsText(ConvertToFeetAndInches(widthInFeet), ConvertToFeetAndInches(heightInFeet), areaInSquareFeet);
    }





}
