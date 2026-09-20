using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UI2DSprite))]
public class UIChangingSprite : MonoBehaviour
{
	public List<Sprite> sprites = new List<Sprite>();

	public void ChangeSprite(int n)
	{
		GetComponent<UI2DSprite>().sprite2D = sprites[n];
	}
}
