using System;
using UnityEngine;

public class CornerTalkGUI : BaseGUI
{
	public int offscreen_x;

	public float appearing_duration = 0.3f;

	public float appearing_delay = 0.2f;

	public float disappearing_duration = 0.3f;

	public UI2DSprite speaker_gfx;

	[NonSerialized]
	public bool appeared;

	private Transform _corner_point_transform;

	public override void Init()
	{
		appeared = false;
		_corner_point_transform = GetComponentInChildren<BubbleCornerPoint>(includeInactive: true).transform;
		speaker_gfx.transform.localPosition = Vector3.right * offscreen_x;
		base.Init();
	}

	public void Say(string locale, GJCommons.VoidDelegate on_complete, string sprite = "", SmartSpeechEngine.VoiceID voice = SmartSpeechEngine.VoiceID.None)
	{
		if (!base.is_shown)
		{
			Open();
		}
		if (string.IsNullOrEmpty(sprite))
		{
			sprite = "ui_phone_call_red_eye";
		}
		speaker_gfx.sprite2D = EasySpritesCollection.GetSprite(sprite);
		if (appeared)
		{
			ShowMessage(locale, on_complete, voice);
			return;
		}
		Vector3 localPosition = speaker_gfx.transform.localPosition;
		localPosition.x = 0f;
		speaker_gfx.ChangePos(speaker_gfx.transform.localPosition, localPosition, appearing_duration, delegate
		{
			ShowMessage(locale, on_complete, voice);
		}, appearing_delay);
	}

	public override void Hide(bool play_hide_sound = true)
	{
		Vector3 localPosition = speaker_gfx.transform.localPosition;
		localPosition.x = offscreen_x;
		speaker_gfx.ChangePos(speaker_gfx.transform.localPosition, localPosition, disappearing_duration, delegate
		{
			base.Hide(play_hide_sound: false);
		});
	}

	private void ShowMessage(string locale, GJCommons.VoidDelegate on_complete, SmartSpeechEngine.VoiceID voice = SmartSpeechEngine.VoiceID.None)
	{
		SpeechBubbleGUI.ShowMessage(GetInstanceID(), locale, _corner_point_transform, on_complete, show_to_left: true, use_world_cam: false, SpeechBubbleGUI.SpeechBubbleType.Talk, is_player: false, voice);
	}
}
