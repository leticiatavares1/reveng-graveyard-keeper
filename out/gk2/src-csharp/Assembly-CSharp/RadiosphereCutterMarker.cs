using UnityEngine;

public class RadiosphereCutterMarker : MonoBehaviour
{
	[Tooltip("If true, the radius from the CapsuleCollider on this object will be used for NavMesh cutting when a worker is assigned.")]
	public bool useRadiusFromCapsule = true;
}
