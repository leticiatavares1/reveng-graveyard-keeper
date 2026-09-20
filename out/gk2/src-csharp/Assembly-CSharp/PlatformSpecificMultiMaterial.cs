using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Materials/Platform Specific Multi Material", fileName = "PlatformSpecificMultiMaterial")]
public class PlatformSpecificMultiMaterial : MultiMaterial
{
	[SerializeField]
	private Material standaloneMaterial;

	[SerializeField]
	private Material switchMaterial;

	[SerializeField]
	private Material switchMaterialSimple;

	[SerializeField]
	private Material mobileMaterial;

	[SerializeField]
	private Material minimalMaterial;

	public override Material GetMaterial(PlatformSpecificMaterialType platform)
	{
		return platform switch
		{
			PlatformSpecificMaterialType.Switch => switchMaterial, 
			PlatformSpecificMaterialType.SwitchSimple => switchMaterialSimple, 
			PlatformSpecificMaterialType.Mobile => mobileMaterial, 
			PlatformSpecificMaterialType.Minimal => minimalMaterial, 
			_ => standaloneMaterial, 
		};
	}
}
