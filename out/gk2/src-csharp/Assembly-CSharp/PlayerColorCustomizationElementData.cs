using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerColorCustomizationElementData", menuName = "GK2/PlayerColorCustomizationElementData")]
public class PlayerColorCustomizationElementData : ScriptableObject
{
	public PlayerColorCustomizationType playerColorCustomizationType;

	[SerializeField]
	private List<PlayerColorCustomizationSkinElementData> skinElements;

	[SerializeField]
	private Texture2D emptyTexture;

	public IReadOnlyList<PlayerColorCustomizationSkinElementData> SkinElements => skinElements;

	public int GetPalettesCountForSkin(int skin)
	{
		return GetSkinElement(skin).palettes.Count;
	}

	public int GetMaxPaletteCount()
	{
		int num = 0;
		for (int i = 0; i < skinElements.Count; i++)
		{
			if (skinElements[i].palettes.Count > num)
			{
				num = skinElements[i].palettes.Count;
			}
		}
		return num;
	}

	public Texture2D GetPaletteByIndex(int index, int skin)
	{
		Texture2D texture2D = null;
		if (skinElements.Count == 1)
		{
			texture2D = skinElements[0].GetPaletteByIndex(index);
		}
		for (int i = 0; i < skinElements.Count; i++)
		{
			if (skinElements[i].skinId == skin)
			{
				texture2D = skinElements[i].GetPaletteByIndex(index);
				break;
			}
		}
		if (texture2D == null)
		{
			texture2D = skinElements[0].GetPaletteByIndex(index);
		}
		if (texture2D == null)
		{
			texture2D = emptyTexture;
		}
		return texture2D;
	}

	public PlayerColorCustomizationSkinElementData GetSkinElement(int skin)
	{
		if (skinElements.Count == 1)
		{
			return skinElements[0];
		}
		for (int i = 0; i < skinElements.Count; i++)
		{
			if (skinElements[i].skinId == skin)
			{
				return skinElements[i];
			}
		}
		Debug.LogWarning($"No skin element for id:[{skin}]");
		return null;
	}
}
