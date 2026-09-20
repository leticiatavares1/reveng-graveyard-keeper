using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlueColorReplaceComponent : MonoBehaviour
{
	private Image image;

	private void OnEnable()
	{
		SetColor();
	}

	private void SetColor()
	{
		if (image == null)
		{
			image = GetComponent<Image>();
		}
		image.BlueColorReplace(image.color);
	}
}
