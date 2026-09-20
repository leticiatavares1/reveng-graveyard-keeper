using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "DeformingGrassSettings", menuName = "GK2/Deforming Grass Settings")]
public class DeformingGrassSettings : LazySingletonSO<DeformingGrassSettings>
{
	public bool grassShadow = true;

	[Space(20f)]
	public int grassTextureWidth = 16;

	public int grassTextureHeight = 64;

	public void ApplyShadowSettings()
	{
		DeformingGrass[] array = Object.FindObjectsOfType<DeformingGrass>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyShadowSettings();
		}
	}
}
