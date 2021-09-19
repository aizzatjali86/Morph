using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Psuedo-code:
* Correlate old mesh to new mesh
* - Find bounds and center of model
* - Get all tris in relation to center, Tc of old and new model
* - Correspond each Tc to a sphere of average radius (same for both new and old)
* - Vc is Tc along vector from center to radius
* - Loop through all Vc of new:
*      - Find closest to Vc of old
*      - Get correlation (dict) of new Tc to old Tc
* 
* Use Correlation to animate change between old and new
* - Each Tc old will move to Tc new
*/

public class MeshMorpher : MonoBehaviour
{
    void CorrelateVector(Mesh oldMesh, Mesh newMesh)
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

        //Compare both SphericalVertices
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
}
