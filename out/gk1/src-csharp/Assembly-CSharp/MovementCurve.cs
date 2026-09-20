using UnityEngine;

[CreateAssetMenu(fileName = "MovementCurve", menuName = "Movement Curve", order = 1)]
public class MovementCurve : ScriptableObject
{
	public AnimationCurve curve;
}
