using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TalkingHeadPreset", menuName = "GK2/Animation/TalkingHeadPreset")]
public class TalkingHeadPreset : ScriptableObject
{
	[Serializable]
	public class TalkingHeadClipData
	{
		public List<TalkingHeadFrameData> frames = new List<TalkingHeadFrameData>();

		public float TotalTime => frames?.Sum((TalkingHeadFrameData f) => f.duration + f.pauseAfter) ?? 0f;
	}

	[Serializable]
	public class TalkingHeadFrameData
	{
		public HeadFrame frame = HeadFrame.Frame01;

		[Min(0f)]
		public float duration = 0.08f;

		[Min(0f)]
		[Tooltip("Hold the original frame this long after this frame")]
		public float pauseAfter;
	}

	public enum HeadFrame
	{
		Frame01 = 1,
		Frame02,
		Frame03
	}

	[Min(0f)]
	[Tooltip("Pause between clips when playing a series")]
	public float pauseBetweenClips;

	public List<TalkingHeadClipData> clips = new List<TalkingHeadClipData>();
}
