using System.Collections.Generic;
using UnityEngine;

public static class NGUIExtensionMethods
{
	private const float COLOR_DURATION = 0.1f;

	public static void ChangeSize(this UIWidget w, Vector2 to, float duration, GJCommons.VoidDelegate on_complete = null, float delay = 0f)
	{
		w.ChangeSize(w.width, w.height, (int)to.x, (int)to.y, duration, on_complete, delay);
	}

	public static void ChangeSize(this UIWidget w, float to_x, float to_y, float duration, GJCommons.VoidDelegate on_complete = null, float delay = 0f)
	{
		w.ChangeSize(w.width, w.height, (int)to_x, (int)to_y, duration, on_complete, delay);
	}

	public static void ChangeSize(this UIWidget w, Vector2 from, Vector2 to, float duration, GJCommons.VoidDelegate on_complete = null, float delay = 0f)
	{
		w.ChangeSize((int)from.x, (int)from.y, (int)to.x, (int)to.y, duration, on_complete, delay);
	}

	public static void ChangeSize(this UIWidget w, int from_x, int from_y, int to_x, int to_y, float duration, GJCommons.VoidDelegate on_complete = null, float delay = 0f)
	{
		TweenWidth tweenWidth = TweenWidth.Begin(w, duration, to_x);
		tweenWidth.from = from_x;
		tweenWidth.animationCurve = NGUIAnimCurves.me.size;
		TweenHeight tweenHeight = TweenHeight.Begin(w, duration, to_y);
		tweenHeight.from = from_y;
		tweenHeight.animationCurve = NGUIAnimCurves.me.size;
		if (delay > 0f)
		{
			tweenWidth.delay = delay;
			tweenHeight.delay = delay;
		}
		tweenHeight.SetOnFinished(delegate
		{
			Object.DestroyImmediate(w.GetComponent<TweenWidth>());
			Object.DestroyImmediate(w.GetComponent<TweenHeight>());
			if (on_complete != null)
			{
				on_complete();
			}
		});
	}

	public static void ChangeAlpha(this UIRect w, float from, float to, float duration = 0.1f, GJCommons.VoidDelegate on_complete = null, float delay = 0f, bool apply_from_before_delay = true)
	{
		if (duration.EqualsTo(0f))
		{
			EasyTimer.VoidDelegate voidDelegate = delegate
			{
				w.alpha = to;
				on_complete.TryInvoke();
			};
			if (delay.EqualsTo(0f))
			{
				voidDelegate();
			}
			else
			{
				EasyTimer.Add(delay, voidDelegate);
			}
			return;
		}
		TweenAlpha tweenAlpha = TweenAlpha.Begin(w.gameObject, duration, to);
		tweenAlpha.from = from;
		if (delay > 0f)
		{
			tweenAlpha.delay = delay;
		}
		if (apply_from_before_delay)
		{
			w.alpha = from;
		}
		tweenAlpha.animationCurve = NGUIAnimCurves.me.alpha;
		tweenAlpha.SetOnFinished(delegate
		{
			Object.DestroyImmediate(w.GetComponent<TweenAlpha>());
			if (on_complete != null)
			{
				on_complete();
			}
		});
	}

	public static void TryFinishAlphaTween(this GameObject go)
	{
		TweenAlpha component = go.GetComponent<TweenAlpha>();
		UIRect component2 = go.GetComponent<UIRect>();
		if (component != null && component2 != null)
		{
			component2.alpha = component.to;
			component.DestroyComponent();
		}
	}

	public static void Open(this UIWidget w)
	{
		w.PlaceAtStartPos(create_start: true);
		w.Activate();
	}

	public static void Hide(this UIWidget w)
	{
		w.Deactivate();
		w.PlaceAtStartPos(create_start: false);
	}

	public static void Open(this UIWidget w, float duration, GJCommons.VoidDelegate on_complete = null, float delay = 0f)
	{
		w.Activate();
		w.PlaceAtStartPos(create_start: true);
		Vector3 start_pos = w.transform.localPosition;
		w.ChangePos(w.transform.localPosition - new Vector3(0f, -400f, 0f), w.transform.localPosition, duration, delegate
		{
			w.transform.localPosition = start_pos;
			if (on_complete != null)
			{
				on_complete();
			}
		}, delay);
	}

	public static void Hide(this UIWidget w, float duration, GJCommons.VoidDelegate on_complete = null, bool up = false, float delay = 0f)
	{
		w.Activate();
		Vector3 start_pos = w.PlaceAtStartPos(create_start: true);
		w.ChangePos(w.transform.localPosition, w.transform.localPosition - new Vector3(0f, up ? (-400) : 400, 0f), duration, delegate
		{
			w.transform.localPosition = start_pos;
			if (on_complete != null)
			{
				on_complete();
			}
			w.Deactivate();
		}, delay);
	}

	public static void ChangePos(this UIWidget w, Vector3 from, Vector3 to, float duration, GJCommons.VoidDelegate on_complete = null, float delay = 0f)
	{
		TweenPosition component = w.GetComponent<TweenPosition>();
		AnimationCurve animationCurve = ((component == null) ? NGUIAnimCurves.me.position : component.animationCurve);
		TweenPosition tweenPosition = TweenPosition.Begin(w.gameObject, duration, to);
		tweenPosition.from = from;
		if (delay > 0f)
		{
			tweenPosition.delay = delay;
		}
		tweenPosition.animationCurve = animationCurve;
		tweenPosition.SetOnFinished(delegate
		{
			if (on_complete != null)
			{
				on_complete();
			}
		});
	}

	public static void ChangeColor(this UIWidget w, Color color, float duration = 0.1f, GJCommons.VoidDelegate on_complete = null, float delay = 0f, bool ignore_alpha = false)
	{
		if (ignore_alpha)
		{
			color.a = w.color.a;
		}
		if (duration.EqualsTo(0f))
		{
			w.color = color;
			Object.DestroyImmediate(w.GetComponent<TweenColor>());
			if (on_complete != null)
			{
				on_complete();
			}
			return;
		}
		TweenColor tweenColor = TweenColor.Begin(w.gameObject, duration, color);
		if (delay > 0f)
		{
			tweenColor.delay = delay;
		}
		tweenColor.animationCurve = NGUIAnimCurves.me.color;
		tweenColor.SetOnFinished(delegate
		{
			Object.DestroyImmediate(w.GetComponent<TweenColor>());
			if (on_complete != null)
			{
				on_complete();
			}
		});
	}

	public static void ChangeColor(this GameObject go, Color color, float duration = 0.1f, GJCommons.VoidDelegate on_complete = null, float delay = 0f, bool ignore_alpha = false)
	{
	}

	public static void StopTweens(this UIWidget w, bool call_on_completes = false, bool in_children_too = true)
	{
		List<UITweener> list = new List<UITweener>(w.GetComponents<UITweener>());
		if (list.Count == 0 && in_children_too)
		{
			list.AddRange(w.GetComponentsInChildren<UITweener>(includeInactive: true));
		}
		while (list.Count > 0)
		{
			if (call_on_completes && list[0].onFinished != null)
			{
				foreach (EventDelegate item in list[0].onFinished)
				{
					item.Execute();
				}
			}
			Object.DestroyImmediate(list[0]);
			list.RemoveAt(0);
			if (list.Count == 0 && in_children_too)
			{
				list.AddRange(w.GetComponentsInChildren<UITweener>(includeInactive: true));
			}
		}
	}

	public static void AnimateTransition(this UIWidget from, UIWidget to, float anim_time, UIWidget alpha_target = null, GJCommons.VoidDelegate on_complete = null)
	{
		if (anim_time < 0f)
		{
			anim_time = Time.deltaTime;
		}
		to.transform.position = from.transform.position;
		Vector2 start_size = from.localSize;
		to.ChangeSize(from.localSize, to.localSize, anim_time);
		from.ChangeSize(from.localSize, to.localSize, anim_time);
		from.ChangeAlpha(1f, 0f, anim_time / 2f);
		if (alpha_target == null)
		{
			alpha_target = to;
		}
		alpha_target.ChangeAlpha(0f, 1f, anim_time, delegate
		{
			from.Hide();
			from.width = (int)start_size.x;
			from.height = (int)start_size.y;
			from.alpha = 1f;
			if (on_complete != null)
			{
				on_complete();
			}
		});
	}

	public static Vector3 PlaceAtStartPos(this UIWidget w, bool create_start)
	{
		StartWidgetPos component = w.GetComponent<StartWidgetPos>();
		if (component == null)
		{
			if (create_start)
			{
				component = w.gameObject.AddComponent<StartWidgetPos>();
				return component.pos = w.transform.localPosition;
			}
			return -1f * Vector3.one;
		}
		return w.transform.localPosition = component.pos;
	}

	public static void DrawAndResize(this UI2DSprite ui_sprite, Sprite sprite)
	{
		ui_sprite.sprite2D = sprite;
		ui_sprite.ResizeByContent();
	}

	public static void ResizeByContent(this UI2DSprite ui_sprite)
	{
		Sprite sprite2D = ui_sprite.sprite2D;
		if (!(sprite2D == null))
		{
			ui_sprite.type = UIBasicSprite.Type.Tiled;
			ui_sprite.width = Mathf.RoundToInt(sprite2D.textureRect.width);
			ui_sprite.height = Mathf.RoundToInt(sprite2D.textureRect.height);
		}
	}

	public static Collider2D[] GetCollidersUnderMouse(Camera cam)
	{
		return Physics2D.OverlapPointAll(cam.ScreenToWorldPoint(Input.mousePosition), 8192);
	}

	public static void InitEventTriggers(MonoBehaviour behaviour, EventDelegate.Callback on_over, EventDelegate.Callback on_out, EventDelegate.Callback on_press, bool clear_previous = false)
	{
		if (!(behaviour == null))
		{
			UIEventTrigger uIEventTrigger = behaviour.GetComponent<UIEventTrigger>();
			if (uIEventTrigger == null)
			{
				uIEventTrigger = behaviour.gameObject.AddComponent<UIEventTrigger>();
			}
			if (clear_previous)
			{
				uIEventTrigger.onHoverOver.Clear();
				uIEventTrigger.onHoverOut.Clear();
				uIEventTrigger.onPress.Clear();
			}
			uIEventTrigger.onHoverOver.Add(new EventDelegate(on_over));
			uIEventTrigger.onHoverOut.Add(new EventDelegate(on_out));
			uIEventTrigger.onPress.Add(new EventDelegate(on_press));
		}
	}
}
