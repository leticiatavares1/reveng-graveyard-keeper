using UnityEngine;

[CreateAssetMenu(fileName = "ColorReplacePalette", menuName = "GK2/Color Replace Palette", order = 1)]
public class ColorReplacePalette : ScriptableObject
{
	public bool isLut = true;

	public Texture2D palette;

	public Color[] colors;

	public Color[] colorsNew;

	private static readonly int idPalette = Shader.PropertyToID("_Palette");

	private static readonly int idReplaceLut = Shader.PropertyToID("_ReplaceLUT");

	private static readonly int[] idColorsA = new int[20]
	{
		Shader.PropertyToID("_RColor1A"),
		Shader.PropertyToID("_RColor2A"),
		Shader.PropertyToID("_RColor3A"),
		Shader.PropertyToID("_RColor4A"),
		Shader.PropertyToID("_RColor5A"),
		Shader.PropertyToID("_RColor6A"),
		Shader.PropertyToID("_RColor7A"),
		Shader.PropertyToID("_RColor8A"),
		Shader.PropertyToID("_RColor9A"),
		Shader.PropertyToID("_RColor10A"),
		Shader.PropertyToID("_RColor11A"),
		Shader.PropertyToID("_RColor12A"),
		Shader.PropertyToID("_RColor13A"),
		Shader.PropertyToID("_RColor14A"),
		Shader.PropertyToID("_RColor15A"),
		Shader.PropertyToID("_RColor16A"),
		Shader.PropertyToID("_RColor17A"),
		Shader.PropertyToID("_RColor18A"),
		Shader.PropertyToID("_RColor19A"),
		Shader.PropertyToID("_RColor20A")
	};

	private static readonly int[] idColorsB = new int[20]
	{
		Shader.PropertyToID("_RColor1B"),
		Shader.PropertyToID("_RColor2B"),
		Shader.PropertyToID("_RColor3B"),
		Shader.PropertyToID("_RColor4B"),
		Shader.PropertyToID("_RColor5B"),
		Shader.PropertyToID("_RColor6B"),
		Shader.PropertyToID("_RColor7B"),
		Shader.PropertyToID("_RColor8B"),
		Shader.PropertyToID("_RColor9B"),
		Shader.PropertyToID("_RColor10B"),
		Shader.PropertyToID("_RColor11B"),
		Shader.PropertyToID("_RColor12B"),
		Shader.PropertyToID("_RColor13B"),
		Shader.PropertyToID("_RColor14B"),
		Shader.PropertyToID("_RColor15B"),
		Shader.PropertyToID("_RColor16B"),
		Shader.PropertyToID("_RColor17B"),
		Shader.PropertyToID("_RColor18B"),
		Shader.PropertyToID("_RColor19B"),
		Shader.PropertyToID("_RColor20B")
	};

	public void UpdateColorsFromPalette()
	{
		if (palette == null || isLut)
		{
			colors = new Color[0];
			colorsNew = new Color[0];
			return;
		}
		if (palette.height != 2)
		{
			Debug.LogError($"Palette \"{palette.name}\" should be of height = 2 (currently: {palette.height}).", palette);
			return;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(palette.width, palette.height);
		temporary.filterMode = FilterMode.Point;
		Graphics.Blit(palette, temporary);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(palette.width, palette.height);
		texture2D.filterMode = FilterMode.Point;
		texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		colors = new Color[texture2D.width];
		colorsNew = new Color[texture2D.width];
		Color[] pixels = texture2D.GetPixels();
		for (int i = 0; i < texture2D.width; i++)
		{
			colors[i] = pixels[i + texture2D.width];
			colorsNew[i] = pixels[i];
		}
		Object.Destroy(texture2D);
	}

	public void ApplyToMaterial(Material material)
	{
		if (isLut)
		{
			material.DisableKeyword("USE_PALETTE_COLOR_REPLACE");
			material.DisableKeyword("USE_PARAMETER_COLOR_REPLACE");
			if (palette == null)
			{
				material.DisableKeyword("USE_LUT_COLOR_REPLACE");
				return;
			}
			material.EnableKeyword("USE_LUT_COLOR_REPLACE");
			material.SetTexture(idReplaceLut, palette);
		}
		else if (colors.Length > 20)
		{
			material.EnableKeyword("USE_PALETTE_COLOR_REPLACE");
			material.SetTexture(idPalette, palette);
		}
		else
		{
			material.DisableKeyword("USE_PALETTE_COLOR_REPLACE");
			material.EnableKeyword("USE_PARAMETER_COLOR_REPLACE");
			for (int i = 0; i < idColorsA.Length; i++)
			{
				material.SetColor(idColorsA[i], (i < colors.Length) ? colors[i] : Color.black);
				material.SetColor(idColorsB[i], (i < colorsNew.Length) ? colorsNew[i] : Color.black);
			}
		}
	}
}
