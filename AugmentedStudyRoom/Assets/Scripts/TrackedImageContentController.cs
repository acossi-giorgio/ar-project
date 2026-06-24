using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImageContentController : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private ARRaycastManager raycastManager;

    public enum ModelPositioning
    {
        OnFloor,
        OnSurface
    }

    [System.Serializable]
    public struct ARContent
    {
        public string imageName;

        [Header("UI Card")]
        public GameObject cardPrefab;

        [Header("3D Model (Optional)")]
        public GameObject modelPrefab;
        public ModelPositioning modelPositioning;
    }

    [Header("Image to Prefab association (Cards)")]
    [SerializeField] private List<ARContent> arContents = new List<ARContent>();
    [SerializeField] private GameObject defaultContentPrefab;

    [Header("Position offset for horizontal images (e.g. table)")]
    [SerializeField] private Vector3 horizontalOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Position offset for vertical images (e.g. wall)")]
    [SerializeField] private Vector3 verticalOffset = new Vector3(0.15f, 0f, 0f);

    [Header("3D model offset relative to image (in meters)")]
    [SerializeField] private Vector3 modelOnFloorOffset = new Vector3(0f, 0f, 0.1f);

    [Header("Threshold to distinguish vertical from horizontal")]
    [SerializeField] private float verticalAngleThreshold = 45f;

    [Header("Plane Detection - Stability")]
    [SerializeField] private float disablePlaneDetectionAfterSeconds = 5f;
    [SerializeField] private ARPlaneManager planeManager;

    private readonly Dictionary<TrackableId, GameObject> _spawnedCards = new Dictionary<TrackableId, GameObject>();
    private readonly Dictionary<TrackableId, GameObject> _spawnedModels = new Dictionary<TrackableId, GameObject>();
    private readonly Dictionary<TrackableId, ModelPositioning> _spawnedModelTypes = new Dictionary<TrackableId, ModelPositioning>();
    private readonly HashSet<TrackableId> _placedModels = new HashSet<TrackableId>();
    private readonly HashSet<TrackableId> _hiddenByUser = new HashSet<TrackableId>();
    private readonly HashSet<TrackableId> _activeThisFrame = new HashSet<TrackableId>();

    private void Start()
    {
        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>();

        if (disablePlaneDetectionAfterSeconds > 0f && planeManager != null)
        {
            StartCoroutine(DisablePlaneDetectionAfterDelay());
        }
    }

    private IEnumerator DisablePlaneDetectionAfterDelay()
    {
        yield return new WaitForSeconds(disablePlaneDetectionAfterSeconds);
        planeManager.enabled = false;
        Debug.Log($"[AR] Plane detection disabled after {disablePlaneDetectionAfterSeconds}s for greater stability.");
    }

    private void Update()
    {
        CheckForTouchToReopen();

        _activeThisFrame.Clear();

        foreach (var trackedImage in trackedImageManager.trackables)
        {
            TrackableId id = trackedImage.trackableId;
            string imgName = trackedImage.referenceImage.name;
            _activeThisFrame.Add(id);

            if (!_spawnedCards.ContainsKey(id))
            {
                GameObject cardPrefabToSpawn = defaultContentPrefab;
                GameObject modelPrefabToSpawn = null;
                ModelPositioning modelPos = ModelPositioning.OnFloor;

                foreach (var content in arContents)
                {
                    if (content.imageName == imgName)
                    {
                        if (content.cardPrefab != null) cardPrefabToSpawn = content.cardPrefab;
                        modelPrefabToSpawn = content.modelPrefab;
                        modelPos = content.modelPositioning;
                        break;
                    }
                }

                if (cardPrefabToSpawn != null)
                {
                    GameObject cardInstance = Instantiate(cardPrefabToSpawn);
                    cardInstance.SetActive(false);
                    _spawnedCards[id] = cardInstance;
                }
                else
                {
                    continue;
                }

                if (modelPrefabToSpawn != null)
                {
                    GameObject modelInstance = Instantiate(modelPrefabToSpawn);
                    modelInstance.SetActive(false);
                    _spawnedModels[id] = modelInstance;
                    _spawnedModelTypes[id] = modelPos;
                }
            }

            GameObject cardPopup = _spawnedCards[id];
            GameObject modelPopup = _spawnedModels.ContainsKey(id) ? _spawnedModels[id] : null;

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                bool isVertical = IsImageVertical(trackedImage.transform);
                Vector3 cardWorldPosition;
                if (isVertical)
                {
                    Vector3 imageRight = trackedImage.transform.right;
                    imageRight.y = 0f;
                    if (imageRight.sqrMagnitude < 0.01f) imageRight = Vector3.right;
                    cardWorldPosition = trackedImage.transform.position + imageRight.normalized * verticalOffset.x;
                }
                else
                {
                    cardWorldPosition = trackedImage.transform.position + Vector3.up * horizontalOffset.y;
                }

                Vector3 cardLookDir = Camera.main.transform.position - cardWorldPosition;
                if (cardLookDir.sqrMagnitude < 0.001f) cardLookDir = Vector3.forward;
                Quaternion cardRotation = Quaternion.LookRotation(-cardLookDir.normalized, Camera.main.transform.up);

                cardPopup.transform.SetPositionAndRotation(cardWorldPosition, cardRotation);

                if (modelPopup != null)
                {
                    ModelPositioning mType = _spawnedModelTypes[id];
                    Vector3 modelWorldPosition = trackedImage.transform.position;

                    if (mType == ModelPositioning.OnFloor)
                    {
                        // OnFloor
                        if (!_placedModels.Contains(id))
                        {
                            modelWorldPosition.y = GetFloorYPosition(Camera.main.transform.position.y);
                            modelWorldPosition += trackedImage.transform.TransformDirection(modelOnFloorOffset);

                            Vector3 forwardDir;
                            if (IsImageVertical(trackedImage.transform))
                                forwardDir = trackedImage.transform.up;
                            else
                                forwardDir = trackedImage.transform.forward;

                            forwardDir.y = 0f;
                            if (forwardDir.sqrMagnitude < 0.001f) forwardDir = Vector3.forward;

                            Quaternion modelRotation = Quaternion.LookRotation(forwardDir.normalized, Vector3.up);
                            modelPopup.transform.SetPositionAndRotation(modelWorldPosition, modelRotation);
                            _placedModels.Add(id);
                        }
                    }
                    else if (mType == ModelPositioning.OnSurface)
                    {
                        // OnSurface
                        if (!_placedModels.Contains(id))
                        {
                            modelWorldPosition += trackedImage.transform.TransformDirection(modelOnFloorOffset);

                            Vector3 forwardDir = trackedImage.transform.up;
                            forwardDir.y = 0f;
                            if (forwardDir.sqrMagnitude < 0.001f) forwardDir = trackedImage.transform.forward;
                            forwardDir.y = 0f;
                            if (forwardDir.sqrMagnitude < 0.001f) forwardDir = Vector3.forward;

                            Quaternion modelRotation = Quaternion.LookRotation(forwardDir.normalized, Vector3.up);
                            modelPopup.transform.SetPositionAndRotation(modelWorldPosition, modelRotation);
                            _placedModels.Add(id);
                        }
                    }
                }

                if (!_hiddenByUser.Contains(id))
                {
                    cardPopup.SetActive(true);
                    if (modelPopup != null) modelPopup.SetActive(true);
                }
            }
            else
            {
                cardPopup.SetActive(false);
                if (modelPopup != null) modelPopup.SetActive(false);
                _hiddenByUser.Remove(id);
            }
        }

        foreach (var kvp in _spawnedCards)
        {
            if (!_activeThisFrame.Contains(kvp.Key))
            {
                kvp.Value.SetActive(false);
                if (_spawnedModels.ContainsKey(kvp.Key)) _spawnedModels[kvp.Key].SetActive(false);
                _hiddenByUser.Remove(kvp.Key);
            }
        }
    }

    private void CheckForTouchToReopen()
    {
        if (!TryGetTouchPosition(out Vector2 touchPosition)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(touchPosition);

        foreach (var trackedImage in trackedImageManager.trackables)
        {
            if (trackedImage.trackingState != TrackingState.Tracking) continue;

            Plane imagePlane = new Plane(trackedImage.transform.up, trackedImage.transform.position);

            if (imagePlane.Raycast(ray, out float distance))
            {
                Vector3 hitPointWorld = ray.GetPoint(distance);
                Vector3 hitPointLocal = trackedImage.transform.InverseTransformPoint(hitPointWorld);
                Vector2 imgSize = trackedImage.size;
                if (imgSize.x <= 0.001f) imgSize = new Vector2(0.2f, 0.2f);
                float margin = 0.05f;

                if (Mathf.Abs(hitPointLocal.x) <= (imgSize.x / 2f) + margin &&
                    Mathf.Abs(hitPointLocal.z) <= (imgSize.y / 2f) + margin)
                {
                    TrackableId id = trackedImage.trackableId;
                    if (_hiddenByUser.Contains(id))
                    {
                        _hiddenByUser.Remove(id);
                        if (_spawnedCards.ContainsKey(id))
                        {
                            _spawnedCards[id].SetActive(true);
                            if (_spawnedModels.ContainsKey(id)) _spawnedModels[id].SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private bool TryGetTouchPosition(out Vector2 position)
    {
        position = Vector2.zero;

        var pointer = UnityEngine.InputSystem.Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            position = pointer.position.ReadValue();
            return true;
        }

        return false;
    }

    private float GetFloorYPosition(float cameraY)
    {
        if (planeManager == null) return cameraY - 1.5f;

        float lowestY = float.MaxValue;
        bool found = false;

        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                if (plane.transform.position.y < lowestY)
                {
                    lowestY = plane.transform.position.y;
                    found = true;
                }
            }
        }

        return found ? lowestY : cameraY - 1.5f;
    }

    private bool IsImageVertical(Transform imageTransform)
    {
        float angle = Vector3.Angle(imageTransform.up, Vector3.up);
        return angle > verticalAngleThreshold;
    }

    public void HideCurrentPopup()
    {
        foreach (var kvp in _spawnedCards)
        {
            if (kvp.Value.activeSelf)
            {
                kvp.Value.SetActive(false);
                if (_spawnedModels.ContainsKey(kvp.Key)) _spawnedModels[kvp.Key].SetActive(false);
                _hiddenByUser.Add(kvp.Key);
            }
        }
    }

    public void HideSpecificPopup(GameObject popupInstance)
    {
        foreach (var kvp in _spawnedCards)
        {
            if (kvp.Value == popupInstance)
            {
                popupInstance.SetActive(false);
                if (_spawnedModels.ContainsKey(kvp.Key)) _spawnedModels[kvp.Key].SetActive(false);
                _hiddenByUser.Add(kvp.Key);
                break;
            }
        }
    }
}
