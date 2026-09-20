using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretSettings", menuName = "GK2/Fighting/Turret Settings")]
public class TurretSettings : ScriptableObject
{
	public float scanInterval = 0.5f;

	public List<float> shotSampleTimings = new List<float>();
}
