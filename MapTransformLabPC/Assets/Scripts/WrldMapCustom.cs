using UnityEngine;
using Wrld;
using Wrld.Space;
using Wrld.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

//56.45974
//-2.9775


public class WrldMapCustom : MonoBehaviour
{
    [Header("Camera/View Settings")]
    [Tooltip("Camera used to request streamed resources")]
    [SerializeField]
    private Camera m_streamingCamera = null;

    [Header("Setup")]
    [SerializeField]
    private string m_apiKey;

    [Tooltip("In degrees")]
    [Range(-90.0f, 90.0f)]
    [SerializeField]
    private double m_latitudeDegrees = 37.771092;

    [Tooltip("In degrees")]
    [Range(-180.0f, 180.0f)]
    [SerializeField]
    private double m_longitudeDegrees = -122.468385;

    [Tooltip("The distance of the camera from the interest point (meters)")]
    [SerializeField]
    [Range(300.0f, 7000000.0f)]
    private double m_distanceToInterest = 300.0f;

    [Tooltip("Direction you are facing")]
    [SerializeField]
    [Range(0, 360.0f)]
    private double m_headingDegrees = 0.0;

    [Header("Map Behaviour Settings")]
    [Tooltip("Coordinate System to be used. ECEF requires additional calculation and setup")]
    [SerializeField]
    private CoordinateSystem m_coordSystem = CoordinateSystem.UnityWorld;

    [Tooltip("Whether to determine streaming LOD based on distance, instead of altitude")]
    [SerializeField]
    private bool m_streamingLodBasedOnDistance = false;

    [Header("Theme Settings")]
    [Tooltip("Directory within the Resources folder to look for materials during runtime. Default is provided with the package")]
    [SerializeField]
    private string m_materialDirectory = "WrldMaterials/";

    [Tooltip("The material to override all landmarks with. Uses standard diffuse if null.")]
    [SerializeField]
    private Material m_overrideLandmarkMaterial = null;

    [Tooltip("Set to true to use the default mouse & keyboard/touch controls, false if controlling the camera by some other means.")]
    [SerializeField]
    private bool m_useBuiltInCameraControls = true;

    [Header("Collision Settings")]
    [Tooltip("Set to true for Terrain collisions")]
    [SerializeField]
    private bool m_terrainCollisions = true;

    [Tooltip("Set to true for Road collision")]
    [SerializeField]
    private bool m_roadCollisions = true;

    [Tooltip("Set to true for Building collision")]
    [SerializeField]
    private bool m_buildingCollisions = true;

    [Header("Label Settings")]
    [Tooltip("Set to true to display text labels for road names, buildings, and other landmarks.")]
    [SerializeField]
    private bool m_enableLabels = false;

    private Api m_api;
    private Transform pivot;

    private Vector3 originalMapPosition;

    void Awake()
    {
        originalMapPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        pivot = new GameObject("WrldMap Pivot").transform;
        gameObject.transform.parent = pivot;
        SetupApi();

        //Cube that marks the center of the map used for debugging
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cube);
        center.transform.localScale = new Vector3(0.01f, 10f, 0.01f);
        center.transform.parent = gameObject.transform;
        center.transform.localPosition = new Vector3(0f, 0f, 0f);

        m_api.CameraApi.SetControlledCamera(m_streamingCamera);
    }

    void OnEnable()
    {
        if (Api.Instance == null)
        {
            SetupApi();
        }
        m_api.SetEnabled(true);
    }

    void OnDisable()
    {
        m_api.SetEnabled(false);
    }

    string GetApiKey()
    {
        if (APIKeyHelpers.AppearsValid(m_apiKey))
        {
            APIKeyHelpers.CacheAPIKey(m_apiKey);
        }
        else
        {
            var cachedAPIKey = APIKeyHelpers.GetCachedAPIKey();

            if (cachedAPIKey != null)
            {
                m_apiKey = cachedAPIKey;
            }
        }

        return m_apiKey;
    }

    void SetupApi()
    {
        var config = ConfigParams.MakeDefaultConfig();
        config.DistanceToInterest = m_distanceToInterest;
        config.LatitudeDegrees = m_latitudeDegrees;
        config.LongitudeDegrees = m_longitudeDegrees;
        config.HeadingDegrees = m_headingDegrees;
        config.StreamingLodBasedOnDistance = m_streamingLodBasedOnDistance;
        config.MaterialsDirectory = m_materialDirectory;
        config.OverrideLandmarkMaterial = m_overrideLandmarkMaterial;
        config.Collisions.TerrainCollision = m_terrainCollisions;
        config.Collisions.RoadCollision = m_roadCollisions;
        config.Collisions.BuildingCollision = m_buildingCollisions;
        config.EnableLabels = m_enableLabels;

        try
        {
            Api.Create(GetApiKey(), m_coordSystem, transform, config);
        }
        catch (InvalidApiKeyException)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(
                "Error: Invalid WRLD API Key",
                string.Format("Please enter a valid WRLD API Key (see the WrldMap script on GameObject \"{0}\" in the Inspector).",
                    gameObject.name),
                "OK");
#endif
            throw;
        }

        m_api = Api.Instance;

        if (m_useBuiltInCameraControls)
        {
            m_api.CameraApi.SetControlledCamera(m_streamingCamera);
        }

        Wrld.Space.LatLongAltitude latLongAltitude = new Wrld.Space.LatLongAltitude();
        latLongAltitude.SetAltitude(0); /////
        latLongAltitude.SetLatitude(m_latitudeDegrees);
        latLongAltitude.SetLongitude(m_longitudeDegrees);
        m_api.SetOriginPoint(latLongAltitude);
    }

    internal void OnDestroy()
    {
        if (m_api != null)
        {
            m_api.Destroy();
            m_api = null;
        }
    }

    void OnApplicationPause(bool isPaused)
    {
        SetApplicationPaused(isPaused);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        SetApplicationPaused(!hasFocus);
    }

    void SetApplicationPaused(bool isPaused)
    {
        if (isPaused)
        {
            m_api.OnApplicationPaused();
        }
        else
        {
            m_api.OnApplicationResumed();
        }
    }









    void refreshChildren()
    {
        Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren) //add grabbable tag to all children
        {
            if (!child.gameObject.CompareTag(gameObject.tag))
            {
                child.gameObject.tag = gameObject.tag;
            }
        }

        if (gameObject.GetComponent<ClippableObject>().enabled == true)
        {
            foreach (Transform child in allChildren)
            {
                if (child.childCount == 0) //leaf
                {
                    if (child.gameObject.GetComponent<MeshRenderer>() == null)
                    {
                        child.gameObject.AddComponent<MeshRenderer>();
                    }

                    if (child.gameObject.GetComponent<ClippableObject>() == null)
                    {
                        child.gameObject.AddComponent<ClippableObject>();
                        child.gameObject.GetComponent<ClippableObject>().clipPlanes = gameObject.GetComponent<ClippableObject>().clipPlanes;
                        child.gameObject.GetComponent<ClippableObject>().minX = gameObject.GetComponent<ClippableObject>().minX;
                        child.gameObject.GetComponent<ClippableObject>().maxX = gameObject.GetComponent<ClippableObject>().maxX;
                        child.gameObject.GetComponent<ClippableObject>().minZ = gameObject.GetComponent<ClippableObject>().minZ;
                        child.gameObject.GetComponent<ClippableObject>().maxZ = gameObject.GetComponent<ClippableObject>().maxZ;
                    }
                    child.gameObject.GetComponent<ClippableObject>().enabled = true;
                }
            }
        }
        else
        {//if we dont want to clip anything...
            foreach (Transform child in allChildren)
            {
                if (child.childCount == 0) //leaf
                {
                    if (child.gameObject.GetComponent<MeshRenderer>() != null)
                    {
                        if (child.gameObject.GetComponent<MeshRenderer>().material.shader.name != "Standard")
                        {
                            child.gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        refreshChildren();

        if (m_useBuiltInCameraControls && (m_streamingCamera == m_api.CameraApi.GetControlledCamera()))
        {
            m_api.StreamResourcesForBuiltInCamera(m_streamingCamera);
        }
        else
        {
            m_api.StreamResourcesForCamera(m_streamingCamera);
        }

        m_api.Update();

        //Redraw (api call) trigger
        float redrawThreshold = 0.1f;
        float distance = Vector3.Magnitude(gameObject.transform.position - originalMapPosition);
        if (distance >= redrawThreshold)
        {
            redrawMap();
        }
           
    }

    double[] getNewCoordinates(double x_offset, double z_offset, double old_lati, double old_longi)
    {
        double scale = transform.localScale.x;
        double r = 6371000;
        double dlati = (360 / Math.PI) * Math.Asin(Math.Tan(z_offset / (scale * 2 * r)) / Math.Sqrt(1 + (Math.Tan(z_offset / (scale * 2 * r)) * Math.Tan(z_offset / (scale * 2 * r)))));
        double dlongi = (360 / Math.PI) * Math.Asin(Math.Tan(x_offset / (scale * 2 * r)) / (Math.Sqrt(1 + (Math.Tan(x_offset / (scale * 2 * r)) * Math.Tan(x_offset / (scale * 2 * r)))) * Math.Cos(old_lati * Math.PI / 180)));
        double[] adjustedCoords = { old_lati - dlati, old_longi - dlongi};
        return adjustedCoords;
    }

    void redrawMap()
    {
        Vector3 currentMapPosition = gameObject.transform.position;
        gameObject.transform.parent.transform.position = originalMapPosition;
        gameObject.transform.position = currentMapPosition;

        //position of map relative to the map's pivot
        double x_offset = gameObject.transform.localPosition.x;
        double z_offset = gameObject.transform.localPosition.z;

        //coordinates for the api call...
        double[] newCoordinates = getNewCoordinates(x_offset, z_offset, m_latitudeDegrees, m_longitudeDegrees);
        m_latitudeDegrees = newCoordinates[0];
        m_longitudeDegrees = newCoordinates[1];

        //map's centroid is now at the new latlong
        Wrld.Space.LatLongAltitude latLongAltitude = new Wrld.Space.LatLongAltitude();
        latLongAltitude.SetAltitude(0);////
        latLongAltitude.SetLatitude(m_latitudeDegrees);
        latLongAltitude.SetLongitude(m_longitudeDegrees);
        m_api.SetOriginPoint(latLongAltitude);
        LatLong latLong = latLongAltitude.GetLatLong();
        //transform.position = new Vector3(0.0f, transform.position.y, 0.0f);
        transform.position = originalMapPosition;

        //generate new map around the center of the map
        Api.Instance.CameraApi.AnimateTo(latLong, distanceFromInterest: m_distanceToInterest, headingDegrees: m_headingDegrees, tiltDegrees: 0);

        //reset the grab offset because if you are grabbing the object while redrawMap() is called, the map is repositioned, but also the grab offset is preserved, so the map is moved double the desired distance.
        if (GameObject.Find("Controllers").GetComponent<TwoHandControl>().twoGrabbed())
        {
            GameObject.Find("Controllers").GetComponent<TwoHandControl>().recomputeOffset();
        }
        else
        {
            GameObject.Find("Controller (right)").GetComponent<OneHandControl>().recomputeOffset();
            GameObject.Find("Controller (left)").GetComponent<OneHandControl>().recomputeOffset();
        }
    }

    public double getAltitude()
    {
        return m_distanceToInterest;
    }

    public void setAltitude(double newAltitude)
    {
        Wrld.Space.LatLongAltitude latLongAltitude = new Wrld.Space.LatLongAltitude();
        latLongAltitude.SetAltitude(m_distanceToInterest);////
        latLongAltitude.SetLatitude(m_latitudeDegrees);
        latLongAltitude.SetLongitude(m_longitudeDegrees);
        m_api.SetOriginPoint(latLongAltitude);
        LatLong latLong = latLongAltitude.GetLatLong();

        m_distanceToInterest = newAltitude;
        Api.Instance.CameraApi.AnimateTo(latLong, distanceFromInterest: m_distanceToInterest, headingDegrees: m_headingDegrees, tiltDegrees: 0);
    }
}
