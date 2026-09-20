using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorPresetData", menuName = "GK2/ArmorPresetData")]
public class ArmorPresetData : ScriptableObject
{
	public List<ArmorColorPalette> colorPalettes;

	public List<CustomizablePartType> affectedPartTypes;

	public Texture2D sourcePalette;

	public ColorReplacePalette GetColorReplacePalette(int index)
	{
		return PaletteReplaceHelper.CombinePalettes(colorPalettes[index].GetPalettes(), sourcePalette);
	}
}
