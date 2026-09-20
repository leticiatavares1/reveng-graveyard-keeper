using UnityEngine;
using UnityEngine.UI;

public class UIHUDWheelElement : MonoBehaviour
{
	public RectTransform rectTransform;

	public Image icon;

	public Color iconColorDefault = new Color(1f, 1f, 1f, 1f);

	public Color iconColorInactive = new Color(1f, 1f, 1f, 0.4f);
}
