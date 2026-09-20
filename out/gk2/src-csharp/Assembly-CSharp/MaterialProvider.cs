using UnityEngine;

public class MaterialProvider : MonoBehaviour
{
	[SerializeField]
	private MultiMaterial multiMaterial;

	[SerializeField]
	private Renderer targetRenderer;

	private void Awake()
	{
		if (targetRenderer == null)
		{
			targetRenderer = GetComponent<Renderer>();
		}
	}

	private void Start()
	{
		ApplyMaterial();
	}

	public void ApplyMaterial()
	{
		if (!(targetRenderer == null) && !(multiMaterial == null))
		{
			targetRenderer.sharedMaterial = ResolveMaterial();
		}
	}

	public void SetMultiMaterial(MultiMaterial multiMaterial)
	{
		this.multiMaterial = multiMaterial;
	}

	private Material ResolveMaterial()
	{
		return multiMaterial.GetMaterial(GetMaterialType());
	}

	public static PlatformSpecificMaterialType GetMaterialType()
	{
		return PlatformFeatures.GetWaterMaterialType();
	}

	public static bool ShouldKeepMaterialForBuild(string fieldName)
	{
		if (!TryGetMaterialTypeFromFieldName(fieldName, out var type))
		{
			return true;
		}
		return type == GetMaterialType();
	}

	private static bool TryGetMaterialTypeFromFieldName(string fieldName, out PlatformSpecificMaterialType type)
	{
		switch (fieldName)
		{
		case "standaloneMaterial":
			type = PlatformSpecificMaterialType.Standalone;
			return true;
		case "switchMaterial":
			type = PlatformSpecificMaterialType.Switch;
			return true;
		case "switchMaterialSimple":
			type = PlatformSpecificMaterialType.SwitchSimple;
			return true;
		case "mobileMaterial":
			type = PlatformSpecificMaterialType.Mobile;
			return true;
		case "minimalMaterial":
			type = PlatformSpecificMaterialType.Minimal;
			return true;
		default:
			type = PlatformSpecificMaterialType.Standalone;
			return false;
		}
	}
}
