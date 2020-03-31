using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.XR;
using Wrld;

public class OneHandControl : MonoBehaviour {

    public GameObject map;
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grabAction;
    public String GrabTag = "Grabbable";
    private Transform currentObject;
    private float grabDistance;
    private bool grabbingStarted = false;
    private Vector3 grabOffset;
    private Vector3 lastHandPosition;
    private bool grabbingEnabled = true;

    private GameObject controllerCollider;

    void Start()
    {
        controllerCollider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if(gameObject.name == "Controller (right)")
        {
            controllerCollider.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            controllerCollider.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        controllerCollider.transform.parent = gameObject.transform;
        SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
        controllerCollider.transform.localPosition = sphereCollider.center;
        controllerCollider.transform.localScale = new Vector3(sphereCollider.radius * 2, sphereCollider.radius * 2, sphereCollider.radius * 2);
        
        currentObject = null;
        lastHandPosition = transform.position;
        grabDistance = transform.localScale.x / 2;
    }


    public Transform getCurrentObject()
    {
        return currentObject;
    }

    public void toggleGrabbing(bool toggleState)
    {
        grabbingEnabled = toggleState;
    }

    public void recomputeOffset()
    {
        if (currentObject != null)
        {
            grabOffset = currentObject.position - transform.position;
        }
    }

    void Update () {
        if (currentObject == null)
        {
            //check for other objects in proximity
            Collider[] colliders = Physics.OverlapSphere(controllerCollider.transform.position, controllerCollider.transform.localScale.x / 2);
            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.name == gameObject.name || !collider.gameObject.CompareTag(GrabTag)) /////////////////////////////////////////////////////////////////
                    {
                        List<Collider> collidersAL = new List<Collider>(colliders);
                        collidersAL.Remove(collider);
                        colliders = collidersAL.ToArray();
                    }
                }
            }

            if (colliders.Length > 0 && GetGrab())
            {
                currentObject = colliders[0].transform;
                //Add a rigid body to the currentObject
                if (currentObject.GetComponent<Rigidbody>() == null)
                {
                    currentObject.gameObject.AddComponent<Rigidbody>();
                }
                //set grab object to kinematic (disable physics while object is grabbed)
                currentObject.GetComponent<Rigidbody>().isKinematic = true;
                //set grab object to no gravity
                currentObject.GetComponent<Rigidbody>().useGravity = false;
            }
        }




        //If an object got grabbed
        if (currentObject != null)
        {
            //If you grabbed a part of the map, currentObject is now the map
            try
            {
                if (GameObject.ReferenceEquals(currentObject.root.transform.GetChild(0).transform, map.transform))
                {
                    currentObject = map.transform;
                }
            }
            catch
            {

            }

            //if it's the first time grabbing currentObject, calibrate ObjectGrabOffset
            if (grabbingStarted == false)
            {
                grabbingStarted = !grabbingStarted;
                grabOffset = currentObject.position - transform.position;
            }

            if (grabbingEnabled)
            {
                translation();
            }

            //if we we release, release the object and reset
            if (!GetGrab())
            {
                currentObject = null;
                grabbingStarted = false;
            }
        }

        lastHandPosition = transform.position;
    }

    public bool GetGrab()
    {
        return grabAction.GetState(handType);
    }

    void translation()
    {
        if (GameObject.ReferenceEquals(currentObject, map))
        {
            float oldY = map.transform.parent.transform.position.y;
            Vector3 newPosition = map.transform.parent.transform.position + grabOffset;
            currentObject.position = new Vector3(newPosition.x, oldY, newPosition.z);
        }
        else
        {
            float oldY = currentObject.position.y;
            Vector3 newPosition = transform.position + grabOffset;
            currentObject.position = new Vector3(newPosition.x, oldY, newPosition.z);
        }

    }
}