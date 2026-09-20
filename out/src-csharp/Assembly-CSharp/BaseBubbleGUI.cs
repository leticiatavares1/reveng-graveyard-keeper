using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseBubbleGUI : MonoBehaviour
{
	protected const int LD = 0;

	protected const int RD = 1;

	protected const int LU = 2;

	protected const int RU = 3;

	protected const int BOTTOM_CENTER = 4;

	[Range(0f, 0.3f)]
	public float alpha_anim_time = 0.2f;

	[Header("ld, rd, lu, ru, (bc)")]
	public GameObject[] corners;

	[HideInInspector]
	[SerializeField]
	protected Transform tf;

	protected bool rigth = true;

	protected bool up = true;

	protected bool try_show_to_left;

	protected bool try_show_down;

	protected int current_corner_index;

	protected Transform linked_tf;

	public UI2DSprite back_spr;

	[SerializeField]
	protected List<BubbleCornerPoint> points;

	[NonSerialized]
	protected BubbleCornerPoint current_point;

	[SerializeField]
	protected UIWidget[] all_widgets;

	[SerializeField]
	protected bool initialized;

	[NonSerialized]
	protected bool serialized;

	[NonSerialized]
	protected string txt;

	[NonSerialized]
	public bool try_bottom_center;

	public Vector2 offset;

	protected GJCommons.VoidDelegate on_disappeared;

	private void DebugRecalcShifts()
	{
		RecalcShifts();
	}

	public virtual void Init()
	{
		tf = base.transform;
		points = new List<BubbleCornerPoint>();
		GameObject[] array = corners;
		for (int i = 0; i < array.Length; i++)
		{
			BubbleCornerPoint componentInChildren = array[i].GetComponentInChildren<BubbleCornerPoint>(includeInactive: true);
			componentInChildren.Init(tf);
			points.Add(componentInChildren);
		}
		current_point = points[0];
		all_widgets = GetComponentsInChildren<UIWidget>(includeInactive: true);
		base.gameObject.SetActive(value: false);
		initialized = true;
	}

	protected void RecalcShifts(bool for_hints = false)
	{
		UIWidget[] array = all_widgets;
		foreach (UIWidget uIWidget in array)
		{
			if (for_hints)
			{
				uIWidget.UpdateAnchors();
			}
			uIWidget.Update();
		}
		foreach (BubbleCornerPoint point in points)
		{
			point.CalcShift(MainGame.me.gui_cam);
		}
	}

	protected virtual void UpdateBubble(Vector3 world_pos, bool use_world_cam, Vector3 alternative_world_pos = default(Vector3), bool ignore_halfres_magic = false)
	{
		Camera camera = (use_world_cam ? MainGame.me.world_cam : MainGame.me.gui_cam);
		Vector2 vector = points[0].pixel_shift - points[3].pixel_shift;
		Vector2 vector2 = camera.WorldToScreenPoint(world_pos);
		bool flag = alternative_world_pos.magnitude > 0f;
		Vector2 vector3 = (flag ? camera.WorldToScreenPoint(alternative_world_pos) : Vector3.zero);
		Vector2 vector4 = vector2 + vector;
		Vector2 vector5 = (flag ? vector3 : vector2) - vector;
		rigth = !try_show_to_left && vector4.x < (float)Screen.width;
		if (try_show_down)
		{
			up = vector5.y < 0f;
		}
		else
		{
			up = vector4.y < (float)Screen.height;
		}
		UpdateCornerPoint();
		if (!(current_point == null))
		{
			if (!up && flag)
			{
				world_pos = alternative_world_pos;
			}
			tf.SetGUIPosToWorldPos(world_pos, camera, MainGame.me.gui_cam, current_point.shift, !ignore_halfres_magic);
			if (offset.magnitude > 0f)
			{
				tf.localPosition += (Vector3)offset;
			}
		}
	}

	protected void OnContentChanged(bool is_hint = false)
	{
		RecalcShifts();
	}

	public virtual void DestroyBubble()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual void LateUpdate()
	{
		if (linked_tf != null)
		{
			UpdateBubble(linked_tf.position, use_world_cam: true);
		}
	}

	private void UpdateCornerPoint()
	{
		int num = ((!up) ? (rigth ? 2 : 3) : ((!rigth) ? 1 : 0));
		if (try_bottom_center && corners.Length > 4)
		{
			num = 4;
		}
		if (current_corner_index == num && !(current_point == null))
		{
			return;
		}
		current_corner_index = num;
		for (int i = 0; i < corners.Length; i++)
		{
			corners[i].SetActive(i == num);
			if (i == num)
			{
				current_point = points[i];
			}
		}
	}

	protected void StartDisappear()
	{
		if (alpha_anim_time.EqualsTo(0f))
		{
			DestroyBubble();
			on_disappeared.TryInvoke();
			return;
		}
		UIWidget component = GetComponent<UIWidget>();
		component.ChangeAlpha(component.alpha, 0f, alpha_anim_time, delegate
		{
			DestroyBubble();
			on_disappeared.TryInvoke();
		});
	}

	protected void StartAppear()
	{
		GetComponent<UIWidget>().ChangeAlpha(0f, 1f, alpha_anim_time);
	}
}
