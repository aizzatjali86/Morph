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
    public MeshFilter targetMesh;
    Mesh instTargetMesh;
    Mesh instOriginalMesh;

    private void Start()
    {
        StartCoroutine(Morph(GetComponent<MeshFilter>().sharedMesh, targetMesh.sharedMesh));
    }

    private void FixedUpdate()
    {

    }

    IEnumerator Morph(Mesh original, Mesh target)
    {
        //initialize correlated vertices
        Vector3[] originalVertices = CorrelateVector(target, original);
        Vector3[] targetVertices = CorrelateVector(original, target);

        instTargetMesh = new Mesh();
        instTargetMesh.vertices = targetVertices;
        instTargetMesh.triangles = target.triangles;

        GetComponent<MeshFilter>().sharedMesh = instTargetMesh;
        //GetComponent<MeshCollider>().sharedMesh = instTargetMesh;

        instOriginalMesh = new Mesh();
        instOriginalMesh.vertices = originalVertices;
        instOriginalMesh.triangles = original.triangles;

        GameObject temp = new GameObject();
        temp.transform.parent = transform;
        temp.transform.localPosition = Vector3.zero;
        temp.AddComponent<MeshRenderer>();
        temp.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        temp.AddComponent<MeshFilter>();
        temp.GetComponent<MeshFilter>().sharedMesh = instOriginalMesh;

        bool isMorphed = false;

        while (!isMorphed)
        {
            yield return new WaitForSeconds(0.1f);
            List<Vector3> tvs = new List<Vector3>();
            int i = 0;
            foreach (Vector3 v in targetVertices)
            {
                Vector3 vLerp = Vector3.Lerp(v, target.vertices[i], 0.01f);
                tvs.Add(vLerp);
                isMorphed = vLerp == target.vertices[i];
                i++;
            }

            List<Vector3> ovs = new List<Vector3>();
            int j = 0;
            foreach (Vector3 v in originalVertices)
            {
                Vector3 vLerp = Vector3.Lerp(v, original.vertices[i], 0.01f);
                ovs.Add(vLerp);
                isMorphed = vLerp == original.vertices[i];
                j++;
            }

            instTargetMesh.vertices = tvs.ToArray();
            targetVertices = instTargetMesh.vertices;

            instTargetMesh = new Mesh();
            instTargetMesh.vertices = targetVertices;
            instTargetMesh.triangles = target.triangles;

            instOriginalMesh.vertices = ovs.ToArray();
            originalVertices = instOriginalMesh.vertices;

            instOriginalMesh = new Mesh();
            instOriginalMesh.vertices = originalVertices;
            instOriginalMesh.triangles = original.triangles;

            GetComponent<MeshFilter>().sharedMesh = instTargetMesh;
            temp.GetComponent<MeshFilter>().sharedMesh = instOriginalMesh;
        }
    }

    Vector3[] CorrelateVector(Mesh oldMesh, Mesh newMesh)
    {
        Vector3 oldBounds = oldMesh.bounds.extents;
        Vector3 oldCenter = oldMesh.bounds.center;

        int[] oldTris = oldMesh.triangles;
        Vector3[] oldVertices = oldMesh.vertices;

        Vector3 newBounds = newMesh.bounds.extents;
        Vector3 newCenter = newMesh.bounds.center;

        int[] newTris = newMesh.triangles;
        Vector3[] newVertices = newMesh.vertices;

        Vector3[] oldSphericalVertices = ConvertToSphericalVertices(oldVertices, oldCenter, (oldBounds.magnitude + newBounds.magnitude) / 2);
        Vector3[] newSphericalVertices = ConvertToSphericalVertices(newVertices, newCenter, (oldBounds.magnitude + newBounds.magnitude) / 2);

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
