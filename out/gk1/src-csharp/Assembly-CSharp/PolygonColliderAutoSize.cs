using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderAutoSize : MonoBehaviour
{
	public float padding;

	public float offset;

	public Vector2 shift = Vector2.zero;

	public Vector2 expand = Vector2.zero;

	private UIWidget _ui_widget;

	private PolygonCollider2D _collider;

	private int _width;

	private int _height;

	private void Update()
	{
		RecalcColliderSize(force: false);
	}

	private void OnValidate()
	{
		padding = (float)Mathf.RoundToInt(padding * 10f) / 10f;
		offset = (float)Mathf.RoundToInt(offset * 10f) / 10f;
		RecalcColliderSize(force: true);
	}

	private void RecalcColliderSize(bool force)
	{
		if (_ui_widget == null)
		{
			_ui_widget = GetComponent<UIWidget>();
		}
		if (_collider == null)
		{
			_collider = GetComponent<PolygonCollider2D>();
			if (_collider == null)
			{
				_collider = base.gameObject.AddComponent<PolygonCollider2D>();
			}
		}
		if (_ui_widget == null || _collider == null)
		{
			_width = (_height = 0);
		}
		else
		{
			if (!force && _width == _ui_widget.width && _height == _ui_widget.height && _collider.points.Length == 4)
			{
				return;
			}
			_width = _ui_widget.width;
			_height = _ui_widget.height;
			float num = (float)_width / 2f;
			float num2 = (float)_height / 2f - offset;
			float num3 = 0f;
			float num4 = 0f;
			switch (_ui_widget.pivot)
			{
			case UIWidget.Pivot.Left:
				num4 = (float)_width / 2f;
				break;
			case UIWidget.Pivot.TopLeft:
				num4 = (float)_width / 2f;
				num3 = (float)(-_height) / 2f;
				break;
			case UIWidget.Pivot.Top:
				num3 = (float)(-_height) / 2f;
				break;
			}
			num4 += shift.x;
			num3 += shift.y;
			Vector2[] array = new Vector2[4]
			{
				new Vector2(num + num4 + expand.x, num2 + padding + num3 + expand.y),
				new Vector2(0f - num + num4 - expand.x, num2 + padding + num3 + expand.y),
				new Vector2(0f - num + num4 - expand.x, 0f - num2 + padding + num3 - expand.y),
				new Vector2(num + num4 + expand.x, 0f - num2 + padding + num3 - expand.y)
			};
			Vector2[] points = _collider.points;
			bool flag = points.Length != 4;
			if (!flag)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (!points[i].EqualsTo(array[i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				_collider.points = array;
			}
		}
	}
}
