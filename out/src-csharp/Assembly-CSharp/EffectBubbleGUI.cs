using UnityEngine;

public class EffectBubbleGUI : MonoBehaviour
{
	public UILabel label;

	[HideInInspector]
	public Transform tf;

	private Camera _world_cam;

	private Camera _gui_cam;

	private Vector3 _last_target_pos;

	private bool _inited;

	private void Init()
	{
		tf = base.transform;
		_inited = true;
	}

	public void InitEffect(Vector3 pos, string text, Color color, bool ignore_timescale = true, float custom_time = -1f)
	{
		Init();
		_last_target_pos = pos;
		label.color = color;
		label.text = text;
		HideAll();
		label.Activate();
		TweenAlpha componentInChildren = GetComponentInChildren<TweenAlpha>(includeInactive: true);
		TweenPosition componentInChildren2 = GetComponentInChildren<TweenPosition>(includeInactive: true);
		if (custom_time > 0f)
		{
			componentInChildren2.duration = (componentInChildren.duration = custom_time);
			componentInChildren2.to.y *= custom_time;
		}
		if (!ignore_timescale)
		{
			componentInChildren.duration *= Time.timeScale;
			componentInChildren2.duration *= Time.timeScale;
		}
		componentInChildren.Play(forward: true);
		componentInChildren2.Play(forward: true);
		componentInChildren.SetOnFinished(delegate
		{
			EffectBubblesManager.RemoveBubble(this);
		});
		UpdateBubble();
	}

	public void UpdateBubble()
	{
		if (!_inited)
		{
			Init();
		}
		tf.SetGUIPosToWorldPos(_last_target_pos, MainGame.me.world_cam, MainGame.me.gui_cam);
	}

	public void HideAll()
	{
		label.Deactivate();
	}
}
