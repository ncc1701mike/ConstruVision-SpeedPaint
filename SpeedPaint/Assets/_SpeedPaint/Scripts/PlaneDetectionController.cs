using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetectionController : MonoBehaviour
{
    [SerializeField] private PlaneManager _planeManager;
    [SerializeField] private GameObject floorMarkerPrefab, ceilingMarkerPrefab;
    [SerializeField] private Material selectedPlaneMaterial;

    [SerializeField] private TMP_Text floorInstructionText, ceilingInstructionText, wallInstructionText;
    [SerializeField] private GameObject dropdown, slider, addZoneButton;

    [SerializeField] private Animator targetAnimator;

    private GameObject _floorMarkerInstance, _ceilingMarkerInstance;
    private ARPlaneManager _arPlaneManager;
    private ARRaycastManager _arRaycastManager;
    private Camera _arCamera;

    private Vector3? _floorPosition = null;
    private Vector3? _ceilingPosition = null;
    private const float minWallHeight = 2.0f;
    private int touchCount = 0;
    private HashSet<ARPlane> selectedPlanes = new HashSet<ARPlane>();
    private Dictionary<ARPlane, Vector2> planeSizes = new Dictionary<ARPlane, Vector2>();


    void Awake()
    {
        _arPlaneManager = GetComponent<ARPlaneManager>();
        _arRaycastManager = GetComponent<ARRaycastManager>();
        _arCamera = GetComponentInChildren<Camera>();

        _arPlaneManager.enabled = true;
        SetAllPlanesActive(false);

        _arPlaneManager.planesChanged += OnPlanesChanged;
    }

    void Start()
    {
        SetAllPlanesActive(false);
        ShowInstructions();
        PaintCalculatorUI(false);
        TriggerTargetAnimation(false);
    }

    public void OnZoneChanged(int newZoneIndex)
    {
        _planeManager.OnZoneDropdownChanged(newZoneIndex);
    }

    public void OnAddZone()
    {
        _planeManager.AddZone();
    }

    void OnDestroy()
    {
        _arPlaneManager.planesChanged -= OnPlanesChanged;
    }

    private void TriggerTargetAnimation(bool shouldAnimate)
    {
        if (targetAnimator != null)
        {
            targetAnimator.SetBool("IsOverTarget", shouldAnimate);
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            plane.gameObject.SetActive(false);

            if (plane.alignment == PlaneAlignment.Vertical)
            {
                EvaluatePlaneHeight(plane);
            }
            else if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                EvaluateCeiling(plane);
            }
        }

        foreach (var plane in args.updated)
        {
            if (planeSizes.TryGetValue(plane, out Vector2 oldSize))
            {
                float sizeIncrease = (plane.size - oldSize).magnitude / oldSize.magnitude;
                if (sizeIncrease > 0.03f)
                {
                    _planeManager.UpdatePlaneSize(plane, plane.size);
                }
            }
            planeSizes[plane] = plane.size;

            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                EvaluateCeiling(plane);
            }
        }
    }


    void SetAllPlanesActive(bool value)
    {
        foreach (var plane in _arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
    }


    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId) ||
                (touch.phase == TouchPhase.Began && EventSystem.current.IsPointerOverGameObject()))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                if (touchCount < 2)
                {
                    PlaceMarkers(touch.position);
                }
                else
                {
                    SelectPlane(touch.position);
                }
            }
        }

        CheckforSelectablePlanes();
    }


    private void CheckforSelectablePlanes()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (_arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            ARPlane plane = hit.trackable as ARPlane;

            if (plane.alignment == PlaneAlignment.Vertical || plane.alignment == PlaneAlignment.HorizontalUp)
            {
                TriggerTargetAnimation(true);
            }

            else
            {
                TriggerTargetAnimation(false);
            }
        }
        else
        {
            TriggerTargetAnimation(false);
        }
    }

    private void PaintCalculatorUI(bool value)
    {
        dropdown.SetActive(value);
        slider.SetActive(value);
        addZoneButton.SetActive(value);
    }

    private void ShowInstructions()
    {
        floorInstructionText.gameObject.SetActive(true);
        ceilingInstructionText.gameObject.SetActive(false);
        wallInstructionText.gameObject.SetActive(false);
    }

    private void PlaceMarkers(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (_arRaycastManager.Raycast(touchPosition, hits, TrackableType.FeaturePoint))
        {
            Pose hitPose = hits[0].pose;

            if (touchCount == 0 && _floorMarkerInstance == null)
            {
                _floorMarkerInstance = Instantiate(floorMarkerPrefab, hitPose.position, hitPose.rotation);
                _floorPosition = hitPose.position;
                touchCount++;
                floorInstructionText.gameObject.SetActive(false);
                ceilingInstructionText.gameObject.SetActive(true);
            }
            else if (touchCount == 1 && _ceilingMarkerInstance == null)
            {
                _ceilingMarkerInstance = Instantiate(ceilingMarkerPrefab, hitPose.position, hitPose.rotation);
                _ceilingPosition = hitPose.position;
                touchCount++;
                ceilingInstructionText.gameObject.SetActive(false);
                wallInstructionText.gameObject.SetActive(true);

                if (_floorPosition.HasValue && _ceilingPosition.HasValue)
                {
                    _arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Vertical | PlaneDetectionMode.Horizontal;
                    StartCoroutine(EvaluateWallsAfterDelay(0.1f));
                }
            }
        }
    }

    IEnumerator<WaitForSeconds> EvaluateWallsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EvaluateWalls();
    }

    private void SelectPlane(Vector2 touchPosition)
    {
        Destroy(_floorMarkerInstance);
        Destroy(_ceilingMarkerInstance);

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (_arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            ARPlane plane = hit.trackable as ARPlane;

            if (selectedPlanes.Contains(plane))
            {
                plane.gameObject.SetActive(false);
                selectedPlanes.Remove(plane);
                _planeManager.RemovePlaneSize(plane);
            }

            else
            {
                plane.gameObject.SetActive(true);
                plane.GetComponentInChildren<MeshRenderer>().material = selectedPlaneMaterial;
                plane.GetComponent<LineRenderer>().enabled = true;
                selectedPlanes.Add(plane);
                _planeManager.ShowPlaneSize(plane);

                if (selectedPlanes.Count == 1)
                {
                    PaintCalculatorUI(true);
                    wallInstructionText.gameObject.SetActive(false);
                    _planeManager.InitializeSliderAndPaintAmount();
                }

            }
        }
    }

    public void RestoreSelectedPlanes()
    {
        foreach (var plane in _arPlaneManager.trackables)
        {
            var renderer = plane.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = selectedPlaneMaterial;
            }
        }
    }

    private void EvaluateWalls()
    {
        foreach (var plane in _arPlaneManager.trackables)
        {
            if (plane.alignment != PlaneAlignment.Vertical)
            {
                plane.gameObject.SetActive(false);
            }
            else
            {
                EvaluatePlaneHeight(plane);
            }
        }
    }

    private void EvaluateCeiling(ARPlane plane)
    {
        if (_ceilingPosition.HasValue)
        {
            float ceilingThreshold = 0.3f;
            bool isNearCeiling = Mathf.Abs(plane.transform.position.y - _ceilingPosition.Value.y) < ceilingThreshold;
            plane.gameObject.SetActive(isNearCeiling);
        }
    }

    private void EvaluatePlaneHeight(ARPlane plane)
    {
        bool shouldBeActive = false;
        if (plane.alignment == PlaneAlignment.Vertical)
        {
            float estimatedHeight = CalculateHeightFromFloorAndCeiling(plane.transform.position);
            shouldBeActive = estimatedHeight >= minWallHeight;
        }
        else
        {
            plane.gameObject.SetActive(false);
        }
    }


    private float CalculateHeightFromFloorAndCeiling(Vector3 planePosition)
    {
        if (_floorPosition.HasValue && _ceilingPosition.HasValue)
        {
            float heightAboveFloor = Mathf.Abs(planePosition.y - _floorPosition.Value.y);
            float heightBelowCeiling = Mathf.Abs(_ceilingPosition.Value.y - planePosition.y);
            float totalHeight = heightAboveFloor + heightBelowCeiling;
            return totalHeight;
        }
        return 0f;
    }
}

