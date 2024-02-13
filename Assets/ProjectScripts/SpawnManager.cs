using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using UnityEngine.UI;


public class SpawnManager : MonoBehaviour
{
    [SerializeField] private ARRaycastManager _raycastManager;
    [SerializeField] private GameObject _spawnPrefabs;
    public TextMeshProUGUI dimensionsText, areaText;
    public GameObject uiElements;

    private const float metersToFt = 3.28084f;
    private const float sqMetersToSqFt = 10.7639f;
    private List<ARRaycastHit> _raycasthits = new List<ARRaycastHit>();
    private GameObject _spawnedObject;

    
    void Start()
    {
        _spawnedObject = null;
        uiElements.SetActive(false);
        CreateBackgroundPanel();
    }

   
    
    private void SpawnPrefab(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (_spawnedObject != null)
        {
            Destroy(_spawnedObject);
        }
            _spawnedObject = Instantiate(_spawnPrefabs, spawnPosition, spawnRotation);
    }
    public void DestroyPrefab()
    {
        if (_spawnedObject != null)
        {
            Destroy(_spawnedObject);
            _spawnedObject = null;
        }
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
    }


    void Update()
    {
        _raycasthits.Clear();

       if (Input.touchCount ==0)
       {
           return;
       }

        Touch touch = Input.GetTouch(0);

        if (_raycastManager.Raycast(touch.position, _raycasthits, TrackableType.PlaneWithinPolygon))
        {    
            if (touch.phase == TouchPhase.Began)
            { 
                bool isTouchOverUI = touch.position.IsPointOverUIObject();
                
                if (!isTouchOverUI)
                {
                    ARPlane hitPlane = _raycasthits[0].trackable as ARPlane;
                    if (hitPlane != null)
                    {
                        Vector3 planeCenter = hitPlane.transform.position;
                        Quaternion planeRotation = hitPlane.transform.rotation;

                        SpawnPrefab(planeCenter, planeRotation);

                        ShowPlaneSize(hitPlane);
                    }

                }

            }

            else if (touch.phase == TouchPhase.Moved && _spawnedObject != null)
            {
                _spawnedObject.transform.position = _raycasthits[0].pose.position;
            }
        }

    }

    private string ConvertToFeetAndInches(float measurementInFeet)
    {
        int feet = Mathf.FloorToInt(measurementInFeet);
        float inches = (measurementInFeet - feet) * 12;
        return $"{feet}' {inches:F2}\"";
    }

     public void ShowPlaneSize(ARPlane plane)
    {
        // Enable the UI elements
        if (!uiElements.activeSelf)
        {
            uiElements.SetActive(true);
        }

        // Convert size from meters to feet
        Vector2 sizeFeet = plane.size * metersToFt;
        float widthInFeet = sizeFeet.x;
        float heightInFeet = sizeFeet.y;
        float areaInSquareFeet = widthInFeet * heightInFeet;

        // Convert measurements to feet and inches
        string widthText = ConvertToFeetAndInches(widthInFeet);
        string heightText = ConvertToFeetAndInches(heightInFeet);
        string areaTextFormatted = $"{areaInSquareFeet * sqMetersToSqFt:F2} sq ft";

        // Update TMP Text
        dimensionsText.text = $"Width: {widthText}, Height: {heightText}";
        areaText.text = $"Area: {areaTextFormatted}";
    }

    public void ClearUI()
    {
        // Destroy dimension & background panel UI elements 
        // Destroy(uiElements);
        // Disable for ReUse
        uiElements.SetActive(false);
    }

}
