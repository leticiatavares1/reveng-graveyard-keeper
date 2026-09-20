using System;
using DG.Tweening;
using UnityEngine;

public class AnimatedGUIPanel : MonoBehaviour
{
	private enum State
	{
		Hidden,
		Shown,
		Showing,
		Hiding
	}

	public enum AnimationType
	{
		AnimateY,
		AnimateX
	}

	private State _cur_state;

	private int _coord_hidden;

	private int _coord_shown;

	private float _timer;

	private Tweener _tweener;

	public float animation_time = 1f;

	public float stay_shown_time = 3f;

	public AnimationType animation_type;

	protected float coord
	{
		get
		{
			return animation_type switch
			{
				AnimationType.AnimateY => base.transform.localPosition.y, 
				AnimationType.AnimateX => base.transform.localPosition.x, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		set
		{
			Vector3 localPosition = base.transform.localPosition;
			switch (animation_type)
			{
			case AnimationType.AnimateY:
				localPosition.y = value;
				break;
			case AnimationType.AnimateX:
				localPosition.x = value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			base.transform.localPosition = localPosition;
		}
	}

	protected void Init(int coord_hidden, int coord_shown, AnimationType anim_type = AnimationType.AnimateY)
	{
		base.gameObject.SetActive(value: false);
		animation_type = anim_type;
		coord = coord_hidden;
		_coord_hidden = coord_hidden;
		_coord_shown = coord_shown;
	}

	protected void SetVisible(bool vis)
	{
		if (vis && (_cur_state == State.Shown || _cur_state == State.Showing))
		{
			_timer = 0f;
		}
		else
		{
			if (!vis && (_cur_state == State.Hidden || _cur_state == State.Hiding))
			{
				return;
			}
			base.gameObject.SetActive(value: true);
			Redraw();
			if (_tweener != null)
			{
				_tweener.Kill();
			}
			State dest = (vis ? State.Shown : State.Hidden);
			_tweener = DOTween.To(() => coord, delegate(float v)
			{
				coord = v;
			}, vis ? _coord_shown : _coord_hidden, animation_time).OnComplete(delegate
			{
				_tweener = null;
				_cur_state = dest;
				_timer = 0f;
				if (_cur_state == State.Hidden)
				{
					base.gameObject.SetActive(value: false);
				}
			});
			_cur_state = (vis ? State.Showing : State.Hiding);
		}
	}

	public void Show()
	{
		SetVisible(vis: true);
	}

	public virtual void Redraw()
	{
	}

	public void Update()
	{
		if (_cur_state == State.Shown)
		{
			_timer += Time.deltaTime;
			if (_timer > stay_shown_time)
			{
				SetVisible(vis: false);
			}
		}
	}
}
