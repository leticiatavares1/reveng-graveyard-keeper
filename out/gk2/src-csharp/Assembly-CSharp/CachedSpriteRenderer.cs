using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CachedSpriteRenderer : MonoBehaviour
{
	private SpriteRenderer sprr;

	private bool isSprrSet;

	public SpriteRenderer SpriteRenderer
	{
		get
		{
			if (!isSprrSet)
			{
				isSprrSet = true;
				sprr = GetComponent<SpriteRenderer>();
			}
			return sprr;
		}
	}
}
