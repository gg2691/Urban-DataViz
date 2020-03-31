using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoHandControl : MonoBehaviour
{
    private Transform grabbedObject;
    private Vector3 centroid = new Vector3(0.0f, 0.0f, 0.0f);
    private Transform pivot;
    private List<Transform> controllers = new List<Transform>();
    private bool grabbingStarted = false;
    private Vector3 grabOffset;
    private Vector3 lastCentroidPosition;
    private Vector3 lastControllerToControllerVector;
    private Vector3 grabInstanceController0Position;
    private Vector3 grabInstanceController1Position;
    private Vector3 grabInstancePivotScale;
    public GameObject map;
    private double grabInstanceMapAltitude;
    private int currentMode = 1;

    void Start()
    {
        controllers.Add(this.gameObject.transform.GetChild(0));
        controllers.Add(this.gameObject.transform.GetChild(1));
    }

    void Update()
    {
        if (!twoGrabbed())
        {
            if(currentMode == 2)
            {
                resetVars();
                toggleSingleControllerGrabbing(true);
                currentMode = 1;
            }
        }
        else
        {
            if(currentMode == 1)
            {
                toggleSingleControllerGrabbing(false);
                currentMode = 2;
            }
            twoHandTransformations();
        }
    }



    public void recomputeOffset()
    {
        grabbingStarted = false;
    }

    public bool twoGrabbed()
    {
        Transform temp = controllers[0].GetComponent<OneHandControl>().getCurrentObject();
        foreach (Transform controller in controllers)
        {
            Transform controllerCurrentObject = controller.GetComponent<OneHandControl>().getCurrentObject();
            if (controllerCurrentObject == null || !GameObject.ReferenceEquals(temp, controllerCurrentObject))
            {
                return false;
            }
        }

        //If both controllers grabbed something, and grabbed the same object...
        grabbedObject = controllers[0].GetComponent<OneHandControl>().getCurrentObject();
        return true;
    }

    void toggleSingleControllerGrabbing(bool toggleState)
    {
        foreach (Transform controller in controllers)
        {
            
            if (toggleState == true)
            {
                controller.GetComponent<OneHandControl>().recomputeOffset();
            }
            controller.GetComponent<OneHandControl>().toggleGrabbing(toggleState);
        }
    }







    void twoHandTransformations()
    {
        centroid = new Vector3(0.0f, 0.0f, 0.0f);
        foreach (Transform controller in controllers)
        {
            centroid += controller.transform.position;
        }
        centroid /= controllers.Count;


        //if it's the first time grabbing an object with 2 hands...
        if (grabbingStarted == false)
        {
            grabbingStarted = !grabbingStarted;
            grabOffset = grabbedObject.transform.position - centroid;

            if (grabbedObject.parent == null)
            {
                pivot = new GameObject(grabbedObject.name + " Pivot").transform;
                grabbedObject.transform.parent = pivot;
                pivot.position = new Vector3(centroid.x, grabbedObject.transform.position.y, centroid.z);
                grabbedObject.transform.localPosition = new Vector3(grabOffset.x, 0, grabOffset.z);
            }
            else
            {
                Vector3 grabInstanceObjectPosition = grabbedObject.transform.position;
                pivot = grabbedObject.transform.parent.transform;
                pivot.position = new Vector3(centroid.x, grabbedObject.transform.position.y, centroid.z);
                grabbedObject.transform.position = grabInstanceObjectPosition;
            }

            grabInstanceController0Position = controllers[0].position;
            grabInstanceController1Position = controllers[1].position;
            lastControllerToControllerVector = grabInstanceController0Position - grabInstanceController1Position;
            grabInstancePivotScale = pivot.localScale;
            if (GameObject.ReferenceEquals(grabbedObject, map.transform))
            {
                grabInstanceMapAltitude = grabbedObject.transform.GetComponent<WrldMapCustom>().getAltitude();
            }
        }
        

        //translation
        pivot.position = new Vector3(centroid.x, pivot.position.y, centroid.z);

        //rotation
        Vector3 handDir0 = lastControllerToControllerVector;
        Vector3 handDir1 = (controllers[0].position - controllers[1].position).normalized;
        Quaternion handRot = Quaternion.FromToRotation(handDir0, handDir1);
        Quaternion newRotation = handRot * pivot.rotation;
        //pivot.rotation = newRotation;
        Vector3 stabilizedRotation = new Vector3(pivot.eulerAngles.x, newRotation.eulerAngles.y, pivot.eulerAngles.z);
        Quaternion stabilizedQuaternion = Quaternion.Euler(stabilizedRotation);
        pivot.rotation = Quaternion.Slerp(pivot.rotation, stabilizedQuaternion, 1);

        //scale
        Vector3 oldVector = grabInstanceController0Position - grabInstanceController1Position;
        Vector3 newVector = controllers[0].position - controllers[1].position;
        float scale = newVector.magnitude / oldVector.magnitude;
        pivot.localScale = grabInstancePivotScale * scale;

        //altitude change based on scale (mouse scrollwheel functionality)
        /*if (GameObject.ReferenceEquals(grabbedObject, map.transform))
        {
            double newAlt = grabInstanceMapAltitude+10;
            grabbedObject.transform.GetComponent<WrldMapCustom>().setAltitude(newAlt);
            print(newAlt);
        }*/


        //update vars
        lastControllerToControllerVector = controllers[0].position - controllers[1].position;
    }

    void resetVars()
    {
        grabbedObject = null;
        grabbingStarted = false;
    }
}
