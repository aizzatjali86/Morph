using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
