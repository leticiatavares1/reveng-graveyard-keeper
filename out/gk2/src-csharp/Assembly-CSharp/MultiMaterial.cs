using UnityEngine;

public abstract class MultiMaterial : ScriptableObject
{
	public abstract Material GetMaterial(PlatformSpecificMaterialType platform);
}
