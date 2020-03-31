using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/*
 * A quick solution for camera's buggy positional jumping and failed positional tracking.
 * By attaching this script to the Player object, which parents the Main Camera, the camera location can be controlled by adjsuting the Player's position. 
 * 
 * Solution 1:
 * Wait a few seconds when the scene first plays, and then reset the camera to the desired position.
 * +: Still allows camera rotation/orientation. Allows for headset translation because the camera isn't fixed at all times.
 * -: Camera might have unexpected jumps later in the program and not in the first few seconds. This solution won't be able to adjust to those later jumps. 
 * 
 * Solution 2:
 * Keep the camera at a constant position at all times by using the update function.
 * +: Still allows camera rotation/orientation. Fixes all unexpected camera jumps at random times that aren't in the first few seconds.
 * -: Completely disables headset positional tracking. i.e. moving your head will not change the camera's position, so everything in the scene will follow you around as if you weren't translating your head.
 */
public class CameraControl : MonoBehaviour
{
    public float x;
    public float y;
    public float z;

    void Start()
    {
        StartCoroutine(reset());
    }


    void LateUpdate()
    {
        //GameObject.Find("Main Camera").GetComponent<Camera>().nearClipPlane = nearClippingPlane;
    }


    //Solution 1
    IEnumerator reset()
    {
        yield return new WaitForSeconds(1);
        transform.position = new Vector3(0, 0, 0);
        Vector3 cameraPosition = Camera.main.transform.position;
        transform.position -= cameraPosition;
        transform.position += new Vector3(x, y, z);
        Quaternion viewDirection = Quaternion.Euler(0, 0, 0);
        transform.rotation = viewDirection;
    }



    //Solution 2
    /*
    private void Update()
    {
        transform.position = new Vector3(0, 0, 0);
        Vector3 cameraPosition = InputTracking.GetLocalPosition(XRNode.Head);
        transform.position -= cameraPosition;
        transform.position += new Vector3(x, y, z);

        Quaternion viewDirection = Quaternion.Euler(0, 0, 0);
        transform.rotation = viewDirection;

    }*/
}
