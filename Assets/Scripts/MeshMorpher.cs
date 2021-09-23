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
    Vector3[] targetVertices;

    Mesh instMesh;

    private void Start()
    {
        StartCoroutine(Morph(GetComponent<MeshFilter>().sharedMesh, targetMesh));
    }

    private void FixedUpdate()
    {

    }

    IEnumerator Morph(Mesh original, Mesh target)
    {
        //initialize correlated vertices
        Vector3[] originalVertices = CorrelateVector(target, original);
        Vector3[] targetVertices = CorrelateVector(original, target);

        instMesh = new Mesh();
        instMesh.vertices = targetVertices;
        instMesh.triangles = targetMesh.triangles;

        GetComponent<MeshFilter>().sharedMesh = instMesh;

        bool isMorphed = false;

        int n = 0;
        while (!isMorphed)
        {
            yield return new WaitForSeconds(0.01f);
            List<Vector3> vs = new List<Vector3>();
            int i = 0;
            foreach (Vector3 v in targetVertices)
            {
                Vector3 vLerp = Vector3.Lerp(v, targetMesh.vertices[i], 0.01f);
                vs.Add(vLerp);
                isMorphed = vLerp == targetMesh.vertices[i];
                i++;
            }

            instMesh.vertices = vs.ToArray();
            targetVertices = instMesh.vertices;

            instMesh = new Mesh();
            instMesh.vertices = targetVertices;
            instMesh.triangles = targetMesh.triangles;

            GetComponent<MeshFilter>().sharedMesh = instMesh;

            Debug.Log(n);
            n++;
        }
    }

    Vector3[] CorrelateVector(Mesh oldMesh, Mesh newMesh)
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
        List<Vector3> newToOld = new List<Vector3>();
        int i = 0;
        foreach (Vector3 newV in newVertices)
        {
            Vector3 oldV = oldSphericalVertices.Select(n => new { n, distance = (n - newSphericalVertices[i]).magnitude }).OrderBy(p => p.distance).First().n;
            newToOld.Add(oldV);
            i++;
        }

        return newToOld.ToArray();
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
