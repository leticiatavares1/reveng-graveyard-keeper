using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerColorCustomizationSkinElementData
{
	public int skinId;

	public List<Texture2D> palettes;

	public Texture2D GetPaletteByIndex(int index)
	{
		if (index >= palettes.Count)
		{
			return null;
		}
		return palettes[index];
	}
}
