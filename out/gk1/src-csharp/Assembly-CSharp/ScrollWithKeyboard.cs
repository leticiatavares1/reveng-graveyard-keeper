using UnityEngine;

[RequireComponent(typeof(UIScrollView))]
public class ScrollWithKeyboard : MonoBehaviour
{
	public enum KeyboardScrollType
	{
		Vertical,
		Horizontal
	}

	public const float MOMENTUM_DECAY = 1f;

	public KeyboardScrollType scroll_type;

	public float scroll_sensivity = 0.1f;

	private UIScrollView _scroll_view;

	public UIScrollView scroll_view => _scroll_view ?? (_scroll_view = GetComponent<UIScrollView>());
}
