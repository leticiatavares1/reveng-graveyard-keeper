using UnityEngine;

[CreateAssetMenu(menuName = "GK2/FX Settings")]
public class FXSettings : ScriptableObject
{
	public float treeChopAnimLen = 1f;

	public float treeDestroyAnimLen = 1f;

	public float treeDestroyActionTime = 0.8f;

	public float treeDestroyGndSpriteDisableTime = 0.3f;

	[Tooltip("Base tree height. It sets default animation behaviour")]
	public float treeDestroyDefaultHeight = 1f;

	[Tooltip("Uses for the calculation of the time for lower and higher trees")]
	public float treeDestroyHeightK = 1f;

	public float GetTreeDestroyAnimLen(float height)
	{
		return treeDestroyAnimLen * (1f + (height - treeDestroyDefaultHeight) * treeDestroyHeightK);
	}
}
