using System;
using DG.Tweening;
using UnityEngine;

public class FlyingObject : MonoBehaviour
{
	public SpriteRenderer spr;

	public UI2DSprite gui_spr;

	public Transform dest;

	public float speed = 1f;

	private bool _flying;

	private bool _active;

	private float? _override_duration;

	private object _obj;

	public Action on_reached_dest;

	private Action _on_late_update;

	private const float OVERHEAD_Y_BOUNCE_HEIGHT = 0.043f;

	public static FlyingObject CreateBuffFlyingObject(BuffDefinition buff, Vector2 world_pos, float? duration = null)
	{
		Debug.Log("CreateBuffFlyingObject buff_id = " + ((buff == null) ? "null" : buff.id));
		FlyingObject flyingObject = GUIElements.me.flying_buff_prefab.Copy();
		flyingObject._override_duration = duration;
		Vector2 vector = MainGame.me.world_cam.WorldToScreenPoint(world_pos);
		flyingObject.transform.position = MainGame.me.gui_cam.ScreenToWorldPoint(vector);
		flyingObject.Init(buff, (buff == null) ? string.Empty : buff.GetIconName());
		if (!GUIElements.me.buffs_panel.gameObject.activeSelf)
		{
			GUIElements.me.buffs_panel.gameObject.SetActive(value: true);
			GUIElements.me.buffs_panel.alpha = 0f;
			DOTween.To(() => GUIElements.me.buffs_panel.alpha, delegate(float v)
			{
				GUIElements.me.buffs_panel.alpha = v;
			}, 1f, 0.6f);
		}
		return flyingObject;
	}

	public static FlyingObject CreateFlyingGUISprite(string gui_sprite_name, Transform start_pos)
	{
		FlyingObject flyingObject = GUIElements.me.flying_buff_prefab.Copy();
		Vector2 vector = MainGame.me.world_cam.WorldToScreenPoint(start_pos.position);
		flyingObject.transform.position = MainGame.me.gui_cam.ScreenToWorldPoint(vector);
		flyingObject.Init(null, gui_sprite_name);
		flyingObject.speed = 0.45f;
		return flyingObject;
	}

	public void StartSmoothFly(Transform destination, float fly_duration = 1f)
	{
		_flying = false;
		dest = destination;
		_on_late_update = delegate
		{
			base.transform.DOMove(dest.position, fly_duration).OnComplete(OnReachedDestination);
		};
	}

	public void StartSmoothFlyAndBounce(Transform destination, float fly_duration = 1f)
	{
		_flying = false;
		dest = destination;
		Vector3 dest_pos = dest.position;
		_on_late_update = delegate
		{
			base.transform.DOMove(base.transform.position + new Vector3(0f, 0.043f), 1f).OnComplete(delegate
			{
				Debug.Log("Finished bounce", this);
				base.transform.DOMove(dest_pos, fly_duration).OnComplete(OnReachedDestination);
			}).SetEase(Ease.OutElastic);
		};
	}

	public void StartSmoothFlyAndBounceToAMovingObject(TweenToAMovingTarget.GetTransformDelegate get_target, float fly_duration = 1f)
	{
		_flying = false;
		_on_late_update = delegate
		{
			base.transform.DOMove(base.transform.position + new Vector3(0f, 0.043f), 1f).OnComplete(delegate
			{
				Debug.Log("Finished bounce", this);
				if (!GUIElements.me.hud_enabled)
				{
					OnReachedDestination();
				}
				else
				{
					TweenToAMovingTarget.DoTweenToAMovingTarget(base.gameObject, get_target, 0.1f, fly_duration, OnReachedDestination);
				}
			}).SetEase(Ease.OutElastic);
		};
	}

	private void Init(object o, string sprite_name)
	{
		_active = (_flying = true);
		_obj = o;
		gui_spr.sprite2D = (string.IsNullOrEmpty(sprite_name) ? null : EasySpritesCollection.GetSprite(sprite_name));
		gui_spr.MakePixelPerfect();
	}

	public void Update()
	{
		if (_active && _flying)
		{
			Vector2 vector = (Vector2)dest.position - (Vector2)base.transform.position;
			base.transform.position += (Vector3)vector.normalized * speed * Time.deltaTime;
			Vector2 vector2 = (Vector2)dest.position - (Vector2)base.transform.position;
			if (Mathf.DeltaAngle(vector.normalized.Atan2() * 57.29578f, vector2.normalized.Atan2() * 57.29578f) > 100f)
			{
				OnReachedDestination();
			}
		}
	}

	private void ReallyGiveBuff(BuffDefinition buff)
	{
		BuffsLogics.AddBuff(buff.id, _override_duration);
	}

	private void OnReachedDestination()
	{
		Debug.Log("FlyingObject reached destination", this);
		_flying = false;
		NGUITools.Destroy(base.gameObject);
		_active = false;
		if (_obj != null && _obj is BuffDefinition)
		{
			ReallyGiveBuff((BuffDefinition)_obj);
		}
		on_reached_dest.TryInvoke();
	}

	private void LateUpdate()
	{
		if (_on_late_update != null)
		{
			Action on_late_update = _on_late_update;
			_on_late_update = null;
			on_late_update();
		}
	}
}
