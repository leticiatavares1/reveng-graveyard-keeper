using System.Collections.Generic;
using UnityEngine;

public class TalkingHeadPlayer
{
	private enum Phase
	{
		None,
		Frame,
		PauseAfter,
		SeriesPause
	}

	private const float MinFrameDuration = 0.001f;

	private readonly AnimationComponent animationComponent;

	private readonly SkinChangerGK2 skinChanger;

	private readonly TalkingHeadPreset preset;

	private bool isPlaying;

	private bool isPaused;

	private bool isSeries;

	private TalkingHeadPreset.TalkingHeadClipData currentClip;

	private int frameIndex;

	private float timer;

	private Phase phase;

	private int currentFrame = 1;

	public bool IsPlaying => isPlaying;

	public TalkingHeadPlayer(AnimationComponent animationComponent, SkinChangerGK2 skinChanger, TalkingHeadPreset preset)
	{
		this.animationComponent = animationComponent;
		this.skinChanger = skinChanger;
		this.preset = preset;
	}

	public void Play(TalkingHeadPreset.TalkingHeadClipData clip)
	{
		if (clip != null && clip.frames != null && clip.frames.Count != 0)
		{
			isPlaying = true;
			isSeries = false;
			BeginClip(clip);
		}
	}

	public void PlaySeries()
	{
		if (!(preset == null) && preset.clips != null && preset.clips.Count != 0)
		{
			isPlaying = true;
			isSeries = true;
			BeginClip(PickRandomClip());
		}
	}

	public void Stop()
	{
		isPlaying = false;
		isPaused = false;
		isSeries = false;
		currentClip = null;
		frameIndex = 0;
		timer = 0f;
		phase = Phase.None;
		currentFrame = 1;
		skinChanger?.SetHeadFrameOverride(1);
	}

	public void Pause()
	{
		if (isPlaying && !isPaused)
		{
			isPaused = true;
			if (currentFrame != 1)
			{
				currentFrame = 1;
				skinChanger?.SetHeadFrameOverride(1);
			}
		}
	}

	public void Resume()
	{
		isPaused = false;
	}

	public void Tick(float deltaTime)
	{
		if (!isPlaying || isPaused)
		{
			return;
		}
		AnimationState state = animationComponent.GetState();
		if ((uint)state > 1u)
		{
			if (currentFrame != 1)
			{
				currentFrame = 1;
				skinChanger.SetHeadFrameOverride(1);
			}
		}
		else
		{
			timer -= deltaTime;
			if (timer > 0f)
			{
				skinChanger?.SetHeadFrameOverride(currentFrame);
			}
			else
			{
				Advance();
			}
		}
	}

	private void BeginClip(TalkingHeadPreset.TalkingHeadClipData clip)
	{
		if (clip == null || clip.frames == null || clip.frames.Count == 0)
		{
			Stop();
			return;
		}
		currentClip = clip;
		frameIndex = 0;
		EnterFrame();
	}

	private void EnterFrame()
	{
		TalkingHeadPreset.TalkingHeadFrameData talkingHeadFrameData = currentClip.frames[frameIndex];
		phase = Phase.Frame;
		currentFrame = (int)talkingHeadFrameData.frame;
		timer = Mathf.Max(talkingHeadFrameData.duration, 0.001f);
		skinChanger?.SetHeadFrameOverride(currentFrame);
	}

	private void Advance()
	{
		if (phase == Phase.SeriesPause)
		{
			BeginClip(PickRandomClip());
			return;
		}
		if (currentClip == null || currentClip.frames == null)
		{
			Stop();
			return;
		}
		TalkingHeadPreset.TalkingHeadFrameData talkingHeadFrameData = currentClip.frames[frameIndex];
		if (phase == Phase.Frame && talkingHeadFrameData.pauseAfter > 0f)
		{
			phase = Phase.PauseAfter;
			currentFrame = 1;
			timer = talkingHeadFrameData.pauseAfter;
			skinChanger?.SetHeadFrameOverride(1);
		}
		else
		{
			frameIndex++;
			if (frameIndex >= currentClip.frames.Count)
			{
				OnClipFinished();
			}
			else
			{
				EnterFrame();
			}
		}
	}

	private void OnClipFinished()
	{
		if (!isSeries)
		{
			Stop();
			return;
		}
		float num = ((preset != null) ? preset.pauseBetweenClips : 0f);
		if (num > 0f)
		{
			phase = Phase.SeriesPause;
			currentFrame = 1;
			timer = num;
			skinChanger?.SetHeadFrameOverride(1);
		}
		else
		{
			BeginClip(PickRandomClip());
		}
	}

	private TalkingHeadPreset.TalkingHeadClipData PickRandomClip()
	{
		List<TalkingHeadPreset.TalkingHeadClipData> clips = preset.clips;
		if (clips == null || clips.Count == 0)
		{
			return null;
		}
		int num = Random.Range(0, clips.Count);
		for (int i = 0; i < clips.Count; i++)
		{
			TalkingHeadPreset.TalkingHeadClipData talkingHeadClipData = clips[(num + i) % clips.Count];
			if (talkingHeadClipData != null && talkingHeadClipData.frames != null && talkingHeadClipData.frames.Count > 0)
			{
				return talkingHeadClipData;
			}
		}
		return null;
	}
}
