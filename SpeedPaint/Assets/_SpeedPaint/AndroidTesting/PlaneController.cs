using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Events;

public class PlaneController : MonoBehaviour
{
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Camera ARCamera;
    [SerializeField] private Game_UI game_UI;
    [SerializeField] private PaintCalculator paintCalculator;

    const float minWallHeight = 2.0f;

    [SerializeField] private Material selectedPlaneMaterial;

    public UnityEvent FloorSet = new UnityEvent();
    public UnityEvent CeilingSet = new UnityEvent();
    public UnityEvent WallSet = new UnityEvent();

    private GameObject floorMarkerInstance;
    private GameObject ceilingMarkerInstance;

    public static Vector3 floorPosition;
    public static Vector3 ceilingPosition;

    [SerializeField] List<ARPlane> detectedPlanes = new List<ARPlane>();
    public List<ReadPlane> selectedPlanes = new List<ReadPlane>();
    [SerializeField] List<ReadPlane> readPlanes = new List<ReadPlane>();



    private void Start()
    {
        planeManager.planesChanged += PlaneManagerOnPlanesChanged;
        //subscribe to the OnTouch event
        UserInput.OnTouch.AddListener(OnTouch);
    }


    private void OnTouch()
    {
        if (UserInput.totalTouchCount == 1)
        {
            FloorSet.Invoke();
            PlaceMarkers(UserInput.touchPosition);
        }
        else if (UserInput.totalTouchCount == 2)
        {
            CeilingSet.Invoke();
            PlaceMarkers(UserInput.touchPosition);
        }
        else
        {
            SelectPlane(UserInput.touchPosition);
        }
    }


    private void PlaneManagerOnPlanesChanged(ARPlanesChangedEventArgs obj)
    {
        foreach (var plane in obj.added)
        {
            detectedPlanes.Add(plane);
            ReadPlaneData(plane);
        }

        foreach (var plane in obj.removed)
        {
            detectedPlanes.Remove(plane);
        }

        foreach (var plane in obj.updated)
        {
            ReadPlaneData(plane);
        }
    }

    private void ReadPlaneData(ARPlane plane)
    {
        var readPlane = plane.AddComponent<ReadPlane>();
        readPlane.plane = plane;

        if (plane.alignment == PlaneAlignment.Vertical)
        {
            readPlane.alignment = PlaneAlignment.Vertical;
        }
        else if (plane.alignment == PlaneAlignment.HorizontalUp)
        {
            readPlane.alignment = PlaneAlignment.HorizontalUp;
        }

        readPlane.size = plane.size;
        readPlane.centerPos = plane.center;

        readPlanes.Add(readPlane);
    }

    private void PlaceMarkers(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.FeaturePoint))
        {
            Pose hitPose = hits[0].pose;
            if (Input.touchCount == 1 && floorMarkerInstance == null)
            {
                floorMarkerInstance = Instantiate(floorMarkerInstance, hitPose.position, hitPose.rotation);
                floorPosition = hitPose.position;
            }
            else if (Input.touchCount == 2 && ceilingMarkerInstance == null)
            {
                ceilingMarkerInstance = Instantiate(ceilingMarkerInstance, hitPose.position, hitPose.rotation);
                ceilingPosition = hitPose.position;
            }
        }
    }

    private void SelectPlane(Vector2 touchPosition)
    {
        Destroy(floorMarkerInstance);
        Destroy(ceilingMarkerInstance);

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            ARPlane plane = hit.trackable as ARPlane;

            foreach (var readPlane in readPlanes)
            {
                if (readPlane.plane == plane)
                {
                    plane.gameObject.SetActive(true);
                    selectedPlanes.Remove(readPlane);
                }
                else
                {
                    plane.gameObject.SetActive(true);
                    plane.GetComponentInChildren<MeshRenderer>().material = selectedPlaneMaterial;
                    plane.GetComponent<LineRenderer>().enabled = true;
                    selectedPlanes.Add(readPlane);
                    planeCalculator.ShowPlaneSize(plane);


                    if (selectedPlanes.Count == 1)
                    {
                        game_UI.HideInstructions();
                        game_UI.ShowPaintCalculator();
                    }
                }
            }
        }
    }

    private void EvaluateWalls()
    {
        foreach (var readPlane in readPlanes)
        {
            if (readPlane.alignment != PlaneAlignment.Vertical)
            {
                readPlane.plane.gameObject.SetActive(false);
            }
            else
            {
                EvaluatePlaneHeight(readPlane);
            }
        }
    }

    private void EvaluatePlaneHeight(ReadPlane readPlane)
    {
        if (readPlane.alignment == PlaneAlignment.Vertical)
        {
            float estimatedHeight = CalculateHeightFromFloorAndCeiling(readPlane.plane.transform.position);
            if (estimatedHeight >= minWallHeight)
            {
                readPlane.plane.gameObject.SetActive(true);
            }
            else
            {
                readPlane.plane.gameObject.SetActive(false);
            }
        }
    }

    private float CalculateHeightFromFloorAndCeiling(Vector3 position)
    {
        if (floorPosition != null && ceilingPosition != null)
        {
            float heightAboveFloor = position.y - floorPosition.y;
            float heightBelowCeiling = ceilingPosition.y - position.y;
            return heightAboveFloor + heightBelowCeiling;
        }
        return 0f;
    }


}
