using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerColorCustomizationData", menuName = "GK2/PlayerColorCustomizationData")]
public class PlayerColorCustomizationData : ScriptableObject
{
	public List<PlayerColorCustomizationElementData> customizationElements;

	public List<CustomizablePartType> affectedPartTypes;

	public Texture2D sourcePalette;

	public ArmorPresetData armorPresetData;
}
