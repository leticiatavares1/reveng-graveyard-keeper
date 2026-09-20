using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorColorPalette", menuName = "GK2/ArmorColorPalette")]
public class ArmorColorPalette : ScriptableObject
{
	public Texture2D goldPalette;

	public Texture2D steelePalette;

	public Texture2D bluePalette;

	public Texture2D redPalette;

	public List<Texture2D> GetPalettes()
	{
		return new List<Texture2D> { goldPalette, steelePalette, bluePalette, redPalette };
	}
}
