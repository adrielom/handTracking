using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPhysics : MonoBehaviour
{

    public static void SetMeshCollider(GameObject line)
    {

        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        MeshCollider meshCollider;

        if (line.GetComponentInChildren<MeshCollider>() == null)
        {
            GameObject child = new GameObject("child");
            child.transform.position = line.transform.position;
            child.transform.SetParent(line.transform);
            meshCollider = child.AddComponent<MeshCollider>();
        }
        else
        {
            meshCollider = line.GetComponentInChildren<MeshCollider>();
        }

        Mesh mesh = new Mesh();

        try
        {
            lineRenderer.BakeMesh(mesh, true);
        }
        catch (System.Exception)
        {
        }
        finally
        {
            meshCollider.sharedMesh = mesh;
        }
    }


}
