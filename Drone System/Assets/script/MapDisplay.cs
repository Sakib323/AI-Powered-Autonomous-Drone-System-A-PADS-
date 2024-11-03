using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using UnityEngine.Networking;

public class MapDisplay : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RawImage mapImage;
    [SerializeField] private GameObject markerPrefab; // Changed to GameObject
    [SerializeField] private float markerHeight = 1f; // Height above the map
    [SerializeField] private GameObject sidebar;

    [Header("Map Settings")]
    [SerializeField] private string subscriptionKey = "YOUR_AZURE_MAPS_KEY";
    [SerializeField] private float latitude = 24.8943264f;
    [SerializeField] private float longitude = 91.8699036f;
    [SerializeField] private int zoomLevel = 10;
    [SerializeField] private int mapWidth = 800;
    [SerializeField] private int mapHeight = 600;
    

    [Header("Pan Settings")]
    [SerializeField] private float panSensitivity = 0.1f;
    
    private Vector2 currentCenter;
    private Vector2 lastDragPosition;
    private GameObject currentMarker;
    private bool isDragging = false;
    private float metersPerPixel;
    private const float EARTH_RADIUS = 6378137;
    private Camera mainCamera;

void Start()
{
    if (string.IsNullOrEmpty(subscriptionKey) || subscriptionKey == "YOUR_AZURE_MAPS_KEY")
    {
        Debug.LogError("Please set your Azure Maps subscription key in the inspector!");
        return;
    }

    mainCamera = Camera.main;
    if (mainCamera == null)
    {
        Debug.LogError("Main camera not found!");
        return;
    }

    // Set mapWidth and mapHeight to device's screen size
    mapWidth = Screen.width;
    mapHeight = Screen.height;

    // Set sidebar dimensions to match screen dimensions
    RectTransform sidebarRect = sidebar.GetComponent<RectTransform>();
    sidebarRect.sizeDelta = new Vector2(300, Screen.height);

    currentCenter = new Vector2(longitude, latitude);
    CalculateMetersPerPixel();
    StartCoroutine(LoadMapImage());
}



    private void CalculateMetersPerPixel()
    {
        metersPerPixel = (EARTH_RADIUS * 2 * Mathf.PI * Mathf.Cos(latitude * Mathf.Deg2Rad)) 
                        / (mapWidth * Mathf.Pow(2, zoomLevel));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 dragDelta = eventData.position - lastDragPosition;
        float latitudeDelta = (dragDelta.y * metersPerPixel * panSensitivity) / 111111f;
        float longitudeDelta = (dragDelta.x * metersPerPixel * panSensitivity) / (111111f * Mathf.Cos(currentCenter.y * Mathf.Deg2Rad));

        currentCenter += new Vector2(-longitudeDelta, -latitudeDelta);
        lastDragPosition = eventData.position;

        StartCoroutine(LoadMapImage());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    IEnumerator LoadMapImage()
    {
        string latString = currentCenter.y.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
        string lonString = currentCenter.x.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

        string url = "https://atlas.microsoft.com/map/static/png?" +
            $"subscription-key={subscriptionKey}&" +
            $"api-version=2.0&" +
            $"center={lonString}%2C{latString}&" +
            $"zoom={zoomLevel}&" +
            "layer=basic&" +
            "style=main&" +
            $"width={mapWidth}&" +
            $"height={mapHeight}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null && mapImage != null)
                {
                    mapImage.texture = texture;
                    mapImage.SetNativeSize();
                    UpdateMarker();
                }
            }
            else
            {
                Debug.LogError($"Failed to load map: {request.error}\n" +
                             $"Response Code: {request.responseCode}\n" +
                             $"URL: {url}");
            }
        }
    }

private void UpdateMarker()
{
    // Ensure the marker is enabled and available
    if (currentMarker == null && markerPrefab != null)
    {
        currentMarker = markerPrefab;
        currentMarker.SetActive(true);  // Make sure it's visible
    }

    if (currentMarker != null && mapImage != null)
    {
        // Get the world position of the map's center
        RectTransform mapRect = mapImage.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        mapRect.GetWorldCorners(corners);

        // Calculate the center position of the map in world space
        Vector3 mapCenter = (corners[0] + corners[2]) / 2f; // Average of bottom-left and top-right corners

        // Position the marker at the calculated center point
        currentMarker.transform.position = mapCenter + Vector3.forward * markerHeight;

        // Make sure the marker is facing the camera
        if (mainCamera != null)
        {
            // Only rotate around the Y-axis to keep the marker upright
            Vector3 directionToCamera = mainCamera.transform.position - currentMarker.transform.position;
            directionToCamera.y = 0; // Keep marker upright
            if (directionToCamera != Vector3.zero)
            {
                currentMarker.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}

    // Optional: Add this method to update marker position when the camera moves
    void LateUpdate()
    {
        if (currentMarker != null && mainCamera != null)
        {
            // Update marker rotation to face camera
            Vector3 directionToCamera = mainCamera.transform.position - currentMarker.transform.position;
            directionToCamera.y = 0; // Keep marker upright
            if (directionToCamera != Vector3.zero)
            {
                currentMarker.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}