using DG.Tweening;
using UnityEngine;

public class PanelAutoScroll : MonoBehaviour
{
	private const float AUTO_SCROLL_TIME = 0.4f;

	[SerializeField]
	private Vector2 _offset;

	private UIWidget _widget;

	private UIScrollView _scroll_view;

	private UIPanel _scroll_panel;

	private Transform _scroll_panel_tf;

	private bool _initialized;

	private bool _avaible;

	public bool avaible
	{
		get
		{
			if (!_initialized)
			{
				Init();
			}
			return _avaible;
		}
	}

	public void Init(UIScrollView scroll_view = null)
	{
		if (!_initialized)
		{
			_widget = GetComponent<UIWidget>();
			if (scroll_view == null)
			{
				scroll_view = GetComponentInParent<UIScrollView>();
			}
			_scroll_view = scroll_view;
			_scroll_panel = ((_scroll_view != null) ? _scroll_view.GetComponent<UIPanel>() : null);
			_scroll_panel_tf = ((_scroll_view != null) ? _scroll_view.transform : null);
			_initialized = true;
			_avaible = _widget != null && _scroll_view != null;
		}
	}

	public void Perform(bool with_animation = true)
	{
		if (!_initialized)
		{
			Init();
		}
		if (!_avaible)
		{
			return;
		}
		_scroll_panel_tf.DOKill();
		Vector4 finalClipRegion = _scroll_panel.finalClipRegion;
		Bounds bounds = _widget.CalculateBounds(_scroll_panel_tf);
		Vector2 vector = new Vector2(finalClipRegion.x, finalClipRegion.y);
		Vector2 vector2 = new Vector2(finalClipRegion.z, finalClipRegion.w) / 2f;
		Vector2 vector3 = vector - vector2;
		Vector2 vector4 = vector + vector2 - (_scroll_panel.clipSoftness + _offset);
		vector3 += _scroll_panel.clipSoftness + _offset;
		Vector3 vector5 = (Vector3)vector4 - bounds.max;
		if (_scroll_view.movement == UIScrollView.Movement.Horizontal)
		{
			if (vector5.x > 0f)
			{
				vector5.x = vector3.x - bounds.min.x;
				if (vector5.x < 0f)
				{
					vector5.x = 0f;
				}
			}
		}
		else
		{
			vector5.x = 0f;
		}
		if (_scroll_view.movement == UIScrollView.Movement.Vertical)
		{
			if (vector5.y > 0f)
			{
				vector5.y = vector3.y - bounds.min.y;
				if (vector5.y < 0f)
				{
					vector5.y = 0f;
				}
			}
		}
		else
		{
			vector5.y = 0f;
		}
		if (!(vector5.magnitude < 0.001f))
		{
			if (with_animation)
			{
				_scroll_panel_tf.DOLocalMove(_scroll_panel_tf.localPosition + vector5, 0.4f, snapping: true).SetEase(Ease.OutCubic);
			}
			else
			{
				_scroll_panel_tf.localPosition += vector5;
			}
		}
	}
}
