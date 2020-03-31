using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class ClippableObject : MonoBehaviour {
    public void OnEnable() {
        //let's just create a new material instance.
        GetComponent<MeshRenderer>().material.shader = Shader.Find("Custom/StandardClippable");
    }

    public void Start() {
        Vector3 plane1 = new Vector3(minX, 0, 0);
        Vector3 rotation1 = new Vector3(0, 0, -90);
        Vector3 plane2 = new Vector3(0, 0, minZ);
        Vector3 rotation2 =  new Vector3(0, 90, 90);
        Vector3 plane3 = new Vector3(maxX, 0, 0);
        Vector3 rotation3 = new Vector3(0, 0, 90);
        Vector3 plane4 = new Vector3(0, 0, maxZ);
        Vector3 rotation4 = new Vector3(0, -90, 90);
        planeSettings = new Vector3[] {plane1, rotation1, plane2, rotation2, plane3, rotation3, plane4, rotation4 };
    }

    //only 3 clip planes for now, will need to modify the shader for more.
    [Range(0, 4)]
    public int clipPlanes = 4;

    //preview size for the planes. Shown when the object is selected.
    public float planePreviewSize = 5.0f;

    public float minX = -1f;
    public float maxX = 1f;
    public float minZ = -1f;
    public float maxZ = 1f;

    //Default positions and rotations for the planes. The rotations will be converted into normals to be used by the shaders.
    private Vector3[] planeSettings;

    public float[] getXZMinMax()
    {
        float[] minmax = { minX, minZ, maxX, maxZ};
        return minmax;
    }

    //Only used for previewing a plane. Draws diagonals and edges of a limited flat plane.
    private void DrawPlane(Vector3 position, Vector3 euler) {
        var forward = Quaternion.Euler(euler) * Vector3.forward;
        var left = Quaternion.Euler(euler) * Vector3.left;

        var forwardLeft = position + forward * planePreviewSize * 0.5f + left * planePreviewSize * 0.5f;
        var forwardRight = forwardLeft - left * planePreviewSize;
        var backRight = forwardRight - forward * planePreviewSize;
        var backLeft = forwardLeft - forward * planePreviewSize;

        Gizmos.DrawLine(position, forwardLeft);
        Gizmos.DrawLine(position, forwardRight);
        Gizmos.DrawLine(position, backRight);
        Gizmos.DrawLine(position, backLeft);

        Gizmos.DrawLine(forwardLeft, forwardRight);
        Gizmos.DrawLine(forwardRight, backRight);
        Gizmos.DrawLine(backRight, backLeft);
        Gizmos.DrawLine(backLeft, forwardLeft);
    }

    private void OnDrawGizmosSelected() {
        for(int i = 0; i < clipPlanes; i++)
        {
            DrawPlane(planeSettings[2*i], planeSettings[2*i+1]);
        }


        /*
        if (clipPlanes >= 1) {
            DrawPlane(plane1Position, plane1Rotation);
        }
        if (clipPlanes >= 2) {
            DrawPlane(plane2Position, plane2Rotation);
        }
        if (clipPlanes >= 3)
        {
            DrawPlane(plane3Position, plane3Rotation);
        }
        if (clipPlanes >= 4)
        {
            DrawPlane(plane4Position, plane4Rotation);
        }*/
    }

    //Ideally the planes do not need to be updated every frame, but we'll just keep the logic here for simplicity purposes.
    public void Update()
    {
        var sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        //Only should enable one keyword. If you want to enable any one of them, you actually need to disable the others. 
        //This may be a bug...
        switch (clipPlanes) {
            case 0:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                sharedMaterial.DisableKeyword("CLIP_FOUR");
                break;
            case 1:
                sharedMaterial.EnableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                sharedMaterial.DisableKeyword("CLIP_FOUR");
                break;
            case 2:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.EnableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                sharedMaterial.DisableKeyword("CLIP_FOUR");
                break;
            case 3:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.EnableKeyword("CLIP_THREE");
                sharedMaterial.DisableKeyword("CLIP_FOUR");
                break;

            case 4:
                sharedMaterial.DisableKeyword("CLIP_ONE");
                sharedMaterial.DisableKeyword("CLIP_TWO");
                sharedMaterial.DisableKeyword("CLIP_THREE");
                sharedMaterial.EnableKeyword("CLIP_FOUR");
                break;
        }

        //pass the planes to the shader if necessary.
        for (int i = 0; i < clipPlanes; i++)
        {
            string posName = "_planePos" + (i+1);
            string normName = "_planeNorm" + (i+1);
            if (i == 0)
            {
                posName = "_planePos";
                normName = "_planeNorm";
            }

            sharedMaterial.SetVector(posName, planeSettings[2*i]);
            sharedMaterial.SetVector(normName, Quaternion.Euler(planeSettings[2*i + 1]) * Vector3.up);
        }


        /*
        if (clipPlanes >= 1)
        {
            sharedMaterial.SetVector("_planePos", plane1Position);
            sharedMaterial.SetVector("_planeNorm", Quaternion.Euler(plane1Rotation) * Vector3.up);
        }

        if (clipPlanes >= 2)
        {
            sharedMaterial.SetVector("_planePos2", plane2Position);
            sharedMaterial.SetVector("_planeNorm2", Quaternion.Euler(plane2Rotation) * Vector3.up);
        }

        if (clipPlanes >= 3)
        {
            sharedMaterial.SetVector("_planePos3", plane3Position);
            sharedMaterial.SetVector("_planeNorm3", Quaternion.Euler(plane3Rotation) * Vector3.up);
        }

        if (clipPlanes >= 4)
        {
            sharedMaterial.SetVector("_planePos4", plane4Position);
            sharedMaterial.SetVector("_planeNorm4", Quaternion.Euler(plane4Rotation) * Vector3.up);
        }*/
    }
}