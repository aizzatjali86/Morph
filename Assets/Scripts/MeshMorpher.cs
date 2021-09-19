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
    public Mesh newMesh;


}
