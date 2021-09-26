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
        StartCoroutine(Morph(GetComponent<MeshFilter>().sharedMesh, targetMesh.sharedMesh, 2));
    }

    private void FixedUpdate()
    {

    }

    IEnumerator Morph(Mesh original, Mesh target, float morphTime)
    {
        //initialize correlated vertices
        Vector3[] originalVertices = CorrelateVector(target, original);
        Vector3[] targetVertices = CorrelateVector(original, target);

        instTargetMesh = new Mesh();
        instTargetMesh.vertices = targetVertices;
        instTargetMesh.triangles = target.triangles;

        Material mat = new Material(Shader.Find("Standard"));
        ChangeRenderMode(mat, BlendMode.Fade);

        GetComponent<MeshFilter>().sharedMesh = instTargetMesh;
        GetComponent<MeshRenderer>().material = mat;
        Color matColor = mat.color;
        matColor.a = 0;
        GetComponent<MeshRenderer>().material.color = matColor;
        //GetComponent<MeshCollider>().sharedMesh = instTargetMesh;
        Vector3[] originalTempVertices = original.vertices;

        instOriginalMesh = new Mesh();
        instOriginalMesh.vertices = originalTempVertices;
        instOriginalMesh.triangles = original.triangles;

        Material mat2 = new Material(Shader.Find("Standard"));
        ChangeRenderMode(mat2, BlendMode.Fade);

        GameObject temp = new GameObject();
        temp.transform.parent = transform;
        temp.transform.localPosition = Vector3.zero;
        temp.AddComponent<MeshRenderer>();
        temp.GetComponent<MeshRenderer>().material = mat2;
        Color matColor2 = mat2.color;
        matColor2.a = 1;
        temp.GetComponent<MeshRenderer>().material.color = matColor2;
        temp.AddComponent<MeshFilter>();
        temp.GetComponent<MeshFilter>().sharedMesh = instOriginalMesh;

        int steps = (int) (morphTime / 0.05f);
        for (int t = 0; t <= steps; t++)
        {
            yield return new WaitForSeconds(0.05f);
            List<Vector3> tvs = new List<Vector3>();
            int i = 0;
            foreach (Vector3 v in targetVertices)
            {
                Vector3 vLerp = Vector3.Lerp(v, target.vertices[i],(float) t/steps);
                tvs.Add(vLerp);
                //isMorphed = vLerp == target.vertices[i];
                i++;
            }

            List<Vector3> ovs = new List<Vector3>();
            int j = 0;
            foreach (Vector3 v in originalTempVertices)
            {
                Vector3 vLerp = Vector3.Lerp(v, originalVertices[j],(float) t/steps);
                ovs.Add(vLerp);
                //isMorphed = vLerp == original.vertices[j];
                j++;
            }

            //instTargetMesh = new Mesh();
            instTargetMesh.vertices = tvs.ToArray();
            instTargetMesh.triangles = target.triangles;

            //instOriginalMesh = new Mesh();
            instOriginalMesh.vertices = ovs.ToArray();
            instOriginalMesh.triangles = original.triangles;

            GetComponent<MeshFilter>().sharedMesh = instTargetMesh;
            matColor.a = (float)t / steps;
            GetComponent<MeshRenderer>().material.color = matColor;

            temp.GetComponent<MeshFilter>().sharedMesh = instOriginalMesh;
            matColor2.a = 1 - (float) t / steps;
            temp.GetComponent<MeshRenderer>().material.color = matColor2;
        }

        Destroy(temp);
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

        Vector3[] oldSphericalVertices = null;
        Vector3[] newSphericalVertices = null;

        if (oldBounds.magnitude > newBounds.magnitude)
        {
            oldSphericalVertices = ConvertToSphericalVertices(oldVertices, oldCenter, newBounds.magnitude);
            newSphericalVertices = ConvertToSphericalVertices(newVertices, newCenter, newBounds.magnitude);
        }
        else
        {
            oldSphericalVertices = ConvertToSphericalVertices(oldVertices, oldCenter, oldBounds.magnitude);
            newSphericalVertices = ConvertToSphericalVertices(newVertices, newCenter, oldBounds.magnitude);
        }

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

    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }

    }
}
