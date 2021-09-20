using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/* Pseudo-code:
* Correlate old mesh to new mesh
* - Find bounds and center of model
* - Get all verts in relation to center, Vc of old and new model
* - Correspond each Vc to a sphere of average radius (same for both new and old)
* - Spherical verts, Sc is Tc along vector from center to radius
* - Loop through all Sc of new:
*      - Find closest to Sc of old
*      - Get correlation (dict) of new Vc to old Vc
* 
* Use Correlation to animate change between old and new
* - Each Vc old will move to Vc new
*/

public class MeshMorpher : MonoBehaviour
{
    public Mesh targetMesh;
    Dictionary<int, int> targetInds;

    private void Start()
    {
        targetInds = CorrelateVector(GetComponent<MeshFilter>().sharedMesh, targetMesh);
    }

    private void Update()
    {
        Mesh instMesh = new Mesh();

        instMesh.triangles = targetMesh.triangles;

        instMesh.vertices = targetMesh.vertices;

        //TODO Change the value of target vertices by lerping from old reference value to target value
    }

    Dictionary<int, int> CorrelateVector(Mesh oldMesh, Mesh newMesh)
    {
        Vector3 oldBounds = oldMesh.bounds.extents;
        Vector3 oldCenter = oldMesh.bounds.center;

        int[] oldTris = oldMesh.triangles;
        Vector3[] oldVertices = oldMesh.vertices;

        Vector3[] oldSphericalVertices = ConvertToSphericalVertices(oldVertices, oldCenter, oldBounds.magnitude);

        Vector3 newBounds = newMesh.bounds.extents;
        Vector3 newCenter = newMesh.bounds.center;

        int[] newTris = newMesh.triangles;
        Vector3[] newVertices = newMesh.vertices;

        Vector3[] newSphericalVertices = ConvertToSphericalVertices(newVertices, newCenter, oldBounds.magnitude);

        //TODO Change to check based on index of vertices instaead of tris
        //Compare both SphericalVertices
        Dictionary<int, int> newToOld = new Dictionary<int, int>();
        int i = 0;
        foreach (int newTr in newTris)
        {
            int oldTr = Array.IndexOf(oldSphericalVertices, oldSphericalVertices.Select(n => new { n, distance = (n - newSphericalVertices[newTr]).magnitude }).OrderBy(p => p.distance).First().n);
            try
            {
                newToOld.Add(newTr, oldTr);
            }
            catch { }
            i++;
        }

        return newToOld;
    }

    Vector3[] ConvertToSphericalVertices(Vector3[] vertices, Vector3 center, float radius)
    {
        List<Vector3> sphereVerts = new List<Vector3>();

        foreach (Vector3 v in vertices)
        {
            Vector3 unitVec = ((v - center) / (v - center).magnitude) * radius;

            sphereVerts.Add(unitVec);
        }

        return sphereVerts.ToArray();
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetComponent<MeshFilter>().sharedMesh.bounds.center, GetComponent<MeshFilter>().sharedMesh.bounds.extents.magnitude);
    }
}
