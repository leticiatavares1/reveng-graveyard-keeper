using UnityEngine;

public class TextureReader
{
	private readonly Texture2D sourceTexture;

	private Texture2D readableTexture;

	public Texture2D ReadableTexture
	{
		get
		{
			if (!readableTexture)
			{
				readableTexture = CreateReadableTexture();
			}
			return readableTexture;
		}
	}

	public TextureReader(Texture2D texture2D)
	{
		sourceTexture = texture2D;
	}

	private Texture2D CreateReadableTexture()
	{
		RenderTexture temporary = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
		Texture2D obj = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.Alpha8, mipChain: false)
		{
			filterMode = FilterMode.Point
		};
		RenderTexture active = RenderTexture.active;
		Graphics.Blit(sourceTexture, temporary);
		RenderTexture.active = temporary;
		obj.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
		obj.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return obj;
	}
}
