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

    public UnityEvent FloorSet = new UnityEvent();
    public UnityEvent CeilingSet = new UnityEvent();
    public UnityEvent WallSet = new UnityEvent();

    [SerializeField] List<ARPlane> detectedPlanes = new List<ARPlane>();
    [SerializeField] List<ARPlane> readPlanes = new List<ARPlane>();



    private void Start()
    {
        planeManager.planesChanged += PlaneManagerOnPlanesChanged;
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
            readPlanes.Remove(plane);
        }

        foreach (var plane in obj.updated)
        {
            ReadPlaneData(plane);
        }
    }

    private void ReadPlaneData(ARPlane plane)
    {
        var readPlane = new ReadPlane();
        if (readPlanes.Contains(plane) == false)
        {
            readPlanes.Add(plane);
        }
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
    }


}
