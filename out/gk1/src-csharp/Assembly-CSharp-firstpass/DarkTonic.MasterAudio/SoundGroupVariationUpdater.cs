using System;
using UnityEngine;

namespace DarkTonic.MasterAudio;

[AudioScriptOrder(-15)]
public class SoundGroupVariationUpdater : MonoBehaviour
{
	private enum WaitForSoundFinishMode
	{
		None,
		Play,
		WaitForEnd,
		StopOrRepeat
	}

	private const float TimeEarlyToScheduleNextClip = 0.1f;

	private const float FakeNegativeFloatValue = -10f;

	private Transform _objectToFollow;

	private GameObject _objectToFollowGo;

	private bool _isFollowing;

	private SoundGroupVariation _variation;

	private float _priorityLastUpdated = -10f;

	private bool _useClipAgePriority;

	private WaitForSoundFinishMode _waitMode;

	private AudioSource _varAudio;

	private MasterAudioGroup _parentGrp;

	private Transform _trans;

	private int _frameNum = -1;

	private bool _inited;

	private float _fadeOutStartTime = -5f;

	private bool _fadeInOutWillFadeOut;

	private bool _hasFadeInOutSetMaxVolume;

	private float _fadeInOutInFactor;

	private float _fadeInOutOutFactor;

	private int _fadeOutEarlyTotalFrames;

	private float _fadeOutEarlyFrameVolChange;

	private int _fadeOutEarlyFrameNumber;

	private float _fadeOutEarlyOrigVol;

	private float _fadeToTargetFrameVolChange;

	private int _fadeToTargetFrameNumber;

	private float _fadeToTargetOrigVol;

	private int _fadeToTargetTotalFrames;

	private float _fadeToTargetVolume;

	private bool _fadeOutStarted;

	private float _lastFrameClipTime = -1f;

	private bool _isPlayingBackward;

	private int _pitchGlideToTargetTotalFrames;

	private float _pitchGlideToTargetFramePitchChange;

	private int _pitchGlideToTargetFrameNumber;

	private float _glideToTargetPitch;

	private float _glideToTargetOrigPitch;

	private Action _glideToPitchCompletionCallback;

	private bool _hasStartedNextInChain;

	private bool _isWaitingForQueuedOcclusionRay;

	private int _framesPlayed;

	private float? _clipStartPosition;

	private float? _clipEndPosition;

	private double? _clipSchedEndTime;

	private bool _hasScheduledNextClip;

	private bool _hasScheduledEndLinkedGroups;

	private int _lastFrameClipPosition = -1;

	private int _timesLooped;

	private bool _isPaused;

	private double _pauseTime;

	private static int _maCachedFromFrame = -1;

	private static MasterAudio _maThisFrame;

	private static Transform _listenerThisFrame;

	public float ClipStartPosition
	{
		get
		{
			if (_clipStartPosition.HasValue)
			{
				return _clipStartPosition.Value;
			}
			if (GrpVariation.useRandomStartTime)
			{
				_clipStartPosition = UnityEngine.Random.Range(GrpVariation.randomStartMinPercent, GrpVariation.randomStartMaxPercent) * 0.01f * VarAudio.clip.length;
			}
			else
			{
				_clipStartPosition = 0f;
			}
			return _clipStartPosition.Value;
		}
	}

	public float ClipEndPosition
	{
		get
		{
			if (_clipEndPosition.HasValue)
			{
				return _clipEndPosition.Value;
			}
			if (GrpVariation.useRandomStartTime)
			{
				_clipEndPosition = GrpVariation.randomEndPercent * 0.01f * VarAudio.clip.length;
			}
			else
			{
				_clipEndPosition = VarAudio.clip.length;
			}
			return _clipEndPosition.Value;
		}
	}

	public int FramesPlayed => _framesPlayed;

	public MasterAudio MAThisFrame => _maThisFrame;

	public float MaxOcclusionFreq
	{
		get
		{
			if (GrpVariation.UsesOcclusion && ParentGroup.willOcclusionOverrideFrequencies)
			{
				return ParentGroup.occlusionMaxCutoffFreq;
			}
			return _maThisFrame.occlusionMaxCutoffFreq;
		}
	}

	public float MinOcclusionFreq
	{
		get
		{
			if (GrpVariation.UsesOcclusion && ParentGroup.willOcclusionOverrideFrequencies)
			{
				return ParentGroup.occlusionMinCutoffFreq;
			}
			return _maThisFrame.occlusionMinCutoffFreq;
		}
	}

	private Transform Trans
	{
		get
		{
			if (_trans != null)
			{
				return _trans;
			}
			_trans = GrpVariation.Trans;
			return _trans;
		}
	}

	private AudioSource VarAudio
	{
		get
		{
			if (_varAudio != null)
			{
				return _varAudio;
			}
			_varAudio = GrpVariation.VarAudio;
			return _varAudio;
		}
	}

	private MasterAudioGroup ParentGroup
	{
		get
		{
			if (_parentGrp != null)
			{
				return _parentGrp;
			}
			_parentGrp = GrpVariation.ParentGroup;
			return _parentGrp;
		}
	}

	private SoundGroupVariation GrpVariation
	{
		get
		{
			if (_variation != null)
			{
				return _variation;
			}
			_variation = GetComponent<SoundGroupVariation>();
			return _variation;
		}
	}

	private float RayCastOriginOffset
	{
		get
		{
			if (GrpVariation.UsesOcclusion && ParentGroup.willOcclusionOverrideRaycastOffset)
			{
				return ParentGroup.occlusionRayCastOffset;
			}
			return _maThisFrame.occlusionRayCastOffset;
		}
	}

	private bool IsOcclusionMeasuringPaused
	{
		get
		{
			if (!_isWaitingForQueuedOcclusionRay)
			{
				return MasterAudio.IsOcclusionFreqencyTransitioning(GrpVariation);
			}
			return true;
		}
	}

	private bool HasEndLinkedGroups
	{
		get
		{
			if (GrpVariation.ParentGroup.endLinkedGroups.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	public void GlidePitch(float targetPitch, float glideTime, Action completionCallback = null)
	{
		GrpVariation.curPitchMode = SoundGroupVariation.PitchMode.Gliding;
		float num = targetPitch - VarAudio.pitch;
		_pitchGlideToTargetTotalFrames = (int)(glideTime / AudioUtil.FrameTime);
		_pitchGlideToTargetFramePitchChange = num / (float)_pitchGlideToTargetTotalFrames;
		_pitchGlideToTargetFrameNumber = 0;
		_glideToTargetPitch = targetPitch;
		_glideToTargetOrigPitch = VarAudio.pitch;
		_glideToPitchCompletionCallback = completionCallback;
	}

	public void FadeOverTimeToVolume(float targetVolume, float fadeTime)
	{
		GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.GradualFade;
		float num = targetVolume - VarAudio.volume;
		float time = VarAudio.time;
		float clipEndPosition = ClipEndPosition;
		if (!VarAudio.loop && VarAudio.clip != null && fadeTime + time > clipEndPosition)
		{
			fadeTime = clipEndPosition - time;
		}
		_fadeToTargetTotalFrames = (int)(fadeTime / AudioUtil.FrameTime);
		_fadeToTargetFrameVolChange = num / (float)_fadeToTargetTotalFrames;
		_fadeToTargetFrameNumber = 0;
		_fadeToTargetOrigVol = VarAudio.volume;
		_fadeToTargetVolume = targetVolume;
	}

	public void FadeOutEarly(float fadeTime)
	{
		GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.FadeOutEarly;
		if (!VarAudio.loop && VarAudio.clip != null && VarAudio.time + fadeTime > ClipEndPosition)
		{
			fadeTime = ClipEndPosition - VarAudio.time;
		}
		float num = AudioUtil.FrameTime;
		if (num == 0f)
		{
			num = AudioUtil.FixedDeltaTime;
		}
		_fadeOutEarlyTotalFrames = (int)(fadeTime / num);
		_fadeOutEarlyFrameVolChange = (0f - VarAudio.volume) / (float)_fadeOutEarlyTotalFrames;
		_fadeOutEarlyFrameNumber = 0;
		_fadeOutEarlyOrigVol = VarAudio.volume;
	}

	public void Initialize()
	{
		if (!_inited)
		{
			_lastFrameClipPosition = -1;
			_timesLooped = 0;
			_isPaused = false;
			_pauseTime = -1.0;
			_clipStartPosition = null;
			_clipEndPosition = null;
			_clipSchedEndTime = null;
			_hasScheduledNextClip = false;
			_hasScheduledEndLinkedGroups = false;
			_inited = true;
		}
	}

	public void FadeInOut()
	{
		GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.FadeInOut;
		_fadeOutStartTime = ClipEndPosition - GrpVariation.fadeOutTime;
		if (GrpVariation.fadeInTime > 0f)
		{
			VarAudio.volume = 0f;
			_fadeInOutInFactor = GrpVariation.fadeMaxVolume / GrpVariation.fadeInTime;
		}
		else
		{
			_fadeInOutInFactor = 0f;
		}
		_fadeInOutWillFadeOut = GrpVariation.fadeOutTime > 0f && !VarAudio.loop;
		if (_fadeInOutWillFadeOut)
		{
			_fadeInOutOutFactor = GrpVariation.fadeMaxVolume / (ClipEndPosition - _fadeOutStartTime);
		}
		else
		{
			_fadeInOutOutFactor = 0f;
		}
	}

	public void FollowObject(bool follow, Transform objToFollow, bool clipAgePriority)
	{
		_isFollowing = follow;
		if (objToFollow != null)
		{
			_objectToFollow = objToFollow;
			_objectToFollowGo = objToFollow.gameObject;
		}
		_useClipAgePriority = clipAgePriority;
		UpdateCachedObjects();
		UpdateAudioLocationAndPriority(rePrioritize: false);
	}

	public void WaitForSoundFinish()
	{
		if (MasterAudio.IsWarming)
		{
			PlaySoundAndWait();
		}
		else
		{
			_waitMode = WaitForSoundFinishMode.Play;
		}
	}

	public void StopPitchGliding()
	{
		GrpVariation.curPitchMode = SoundGroupVariation.PitchMode.None;
		if (_glideToPitchCompletionCallback != null)
		{
			_glideToPitchCompletionCallback();
			_glideToPitchCompletionCallback = null;
		}
		DisableIfFinished();
	}

	public void StopFading()
	{
		GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.None;
		DisableIfFinished();
	}

	public void StopWaitingForFinish()
	{
		_waitMode = WaitForSoundFinishMode.None;
		GrpVariation.curDetectEndMode = SoundGroupVariation.DetectEndMode.None;
		DisableIfFinished();
	}

	public void StopFollowing()
	{
		_isFollowing = false;
		_useClipAgePriority = false;
		_objectToFollow = null;
		_objectToFollowGo = null;
		DisableIfFinished();
	}

	private void DisableIfFinished()
	{
		if (!_isFollowing && GrpVariation.curDetectEndMode != SoundGroupVariation.DetectEndMode.DetectEnd && GrpVariation.curFadeMode == SoundGroupVariation.FadeMode.None && GrpVariation.curPitchMode == SoundGroupVariation.PitchMode.None)
		{
			base.enabled = false;
		}
	}

	private void UpdateAudioLocationAndPriority(bool rePrioritize)
	{
		if (_isFollowing && _objectToFollow != null)
		{
			Trans.position = _objectToFollow.position;
		}
		if (_maThisFrame.prioritizeOnDistance && rePrioritize && !ParentGroup.alwaysHighestPriority && !(Time.realtimeSinceStartup - _priorityLastUpdated <= MasterAudio.ReprioritizeTime))
		{
			AudioPrioritizer.Set3DPriority(GrpVariation, _useClipAgePriority);
			_priorityLastUpdated = AudioUtil.Time;
		}
	}

	private void ResetToNonOcclusionSetting()
	{
		AudioLowPassFilter lowPassFilter = GrpVariation.LowPassFilter;
		if (lowPassFilter != null)
		{
			lowPassFilter.cutoffFrequency = 22000f;
		}
	}

	private void UpdateOcclusion()
	{
		if (!GrpVariation.UsesOcclusion)
		{
			MasterAudio.StopTrackingOcclusionForSource(GrpVariation.GameObj);
			ResetToNonOcclusionSetting();
		}
		else if (!(_listenerThisFrame == null) && !IsOcclusionMeasuringPaused)
		{
			MasterAudio.AddToQueuedOcclusionRays(this);
			_isWaitingForQueuedOcclusionRay = true;
		}
	}

	private void DoneWithOcclusion()
	{
		_isWaitingForQueuedOcclusionRay = false;
		MasterAudio.RemoveFromOcclusionFrequencyTransitioning(GrpVariation);
	}

	public bool RayCastForOcclusion()
	{
		DoneWithOcclusion();
		Vector3 vector = Trans.position;
		float rayCastOriginOffset = RayCastOriginOffset;
		if (rayCastOriginOffset > 0f)
		{
			vector = Vector3.MoveTowards(vector, _listenerThisFrame.position, rayCastOriginOffset);
		}
		Vector3 vector2 = _listenerThisFrame.position - vector;
		float magnitude = vector2.magnitude;
		if (magnitude > VarAudio.maxDistance)
		{
			MasterAudio.AddToOcclusionOutOfRangeSources(GrpVariation.GameObj);
			ResetToNonOcclusionSetting();
			return false;
		}
		MasterAudio.AddToOcclusionInRangeSources(GrpVariation.GameObj);
		bool flag = _maThisFrame.occlusionRaycastMode == MasterAudio.RaycastMode.Physics2D;
		if (GrpVariation.LowPassFilter == null)
		{
			AudioLowPassFilter lowPassFilter = GrpVariation.gameObject.AddComponent<AudioLowPassFilter>();
			GrpVariation.LowPassFilter = lowPassFilter;
		}
		bool queriesStartInColliders = Physics2D.queriesStartInColliders;
		if (flag)
		{
			Physics2D.queriesStartInColliders = _maThisFrame.occlusionIncludeStartRaycast2DCollider;
		}
		bool flag2 = true;
		if (flag)
		{
			flag2 = Physics2D.queriesHitTriggers;
			Physics2D.queriesHitTriggers = _maThisFrame.occlusionRaycastsHitTriggers;
		}
		else
		{
			flag2 = Physics.queriesHitTriggers;
			Physics.queriesHitTriggers = _maThisFrame.occlusionRaycastsHitTriggers;
		}
		Vector3 vector3 = Vector3.zero;
		float? num = null;
		bool flag3 = false;
		if (_maThisFrame.occlusionUseLayerMask)
		{
			switch (_maThisFrame.occlusionRaycastMode)
			{
			case MasterAudio.RaycastMode.Physics3D:
			{
				if (Physics.Raycast(vector, vector2, out var hitInfo, magnitude, _maThisFrame.occlusionLayerMask.value))
				{
					flag3 = true;
					vector3 = hitInfo.point;
					num = hitInfo.distance;
				}
				break;
			}
			case MasterAudio.RaycastMode.Physics2D:
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, vector2, magnitude, _maThisFrame.occlusionLayerMask.value);
				if (raycastHit2D.transform != null)
				{
					flag3 = true;
					vector3 = raycastHit2D.point;
					num = raycastHit2D.distance;
				}
				break;
			}
			}
		}
		else
		{
			switch (_maThisFrame.occlusionRaycastMode)
			{
			case MasterAudio.RaycastMode.Physics3D:
			{
				if (Physics.Raycast(vector, vector2, out var hitInfo2, magnitude))
				{
					flag3 = true;
					vector3 = hitInfo2.point;
					num = hitInfo2.distance;
				}
				break;
			}
			case MasterAudio.RaycastMode.Physics2D:
			{
				RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector, vector2, magnitude);
				if (raycastHit2D2.transform != null)
				{
					flag3 = true;
					vector3 = raycastHit2D2.point;
					num = raycastHit2D2.distance;
				}
				break;
			}
			}
		}
		if (flag)
		{
			Physics2D.queriesStartInColliders = queriesStartInColliders;
			Physics2D.queriesHitTriggers = flag2;
		}
		else
		{
			Physics.queriesHitTriggers = flag2;
		}
		if (_maThisFrame.occlusionShowRaycasts)
		{
			Vector3 end = (flag3 ? vector3 : _listenerThisFrame.position);
			Color color = (flag3 ? Color.red : Color.green);
			Debug.DrawLine(vector, end, color, 0.1f);
		}
		if (!flag3)
		{
			MasterAudio.RemoveFromBlockedOcclusionSources(GrpVariation.GameObj);
			ResetToNonOcclusionSetting();
			return true;
		}
		MasterAudio.AddToBlockedOcclusionSources(GrpVariation.GameObj);
		float occlusionCutoffFrequencyByDistanceRatio = AudioUtil.GetOcclusionCutoffFrequencyByDistanceRatio(num.Value / VarAudio.maxDistance, this);
		float occlusionFreqChangeSeconds = _maThisFrame.occlusionFreqChangeSeconds;
		if (occlusionFreqChangeSeconds <= 0.1f)
		{
			GrpVariation.LowPassFilter.cutoffFrequency = occlusionCutoffFrequencyByDistanceRatio;
			return true;
		}
		MasterAudio.GradualOcclusionFreqChange(GrpVariation, occlusionFreqChangeSeconds, occlusionCutoffFrequencyByDistanceRatio);
		return true;
	}

	private void PlaySoundAndWait()
	{
		if (!(VarAudio.clip == null))
		{
			double num = AudioSettings.dspTime;
			if (GrpVariation.PlaySoundParm.TimeToSchedulePlay.HasValue)
			{
				num = GrpVariation.PlaySoundParm.TimeToSchedulePlay.Value;
			}
			float num2 = 0f;
			if (GrpVariation.useIntroSilence && GrpVariation.introSilenceMax > 0f)
			{
				float num3 = UnityEngine.Random.Range(GrpVariation.introSilenceMin, GrpVariation.introSilenceMax);
				num2 += num3;
			}
			num2 += GrpVariation.PlaySoundParm.DelaySoundTime;
			if (num2 > 0f)
			{
				num += (double)num2;
			}
			VarAudio.PlayScheduled(num);
			AudioUtil.ClipPlayed(VarAudio.clip, GrpVariation.GameObj);
			if (GrpVariation.useRandomStartTime)
			{
				VarAudio.time = ClipStartPosition;
				float num4 = AudioUtil.AdjustAudioClipDurationForPitch(ClipEndPosition - ClipStartPosition, VarAudio);
				_clipSchedEndTime = num + (double)num4;
				VarAudio.SetScheduledEndTime(_clipSchedEndTime.Value);
			}
			GrpVariation.LastTimePlayed = AudioUtil.Time;
			MasterAudio.DuckSoundGroup(ParentGroup.GameObjectName, VarAudio);
			_isPlayingBackward = GrpVariation.OriginalPitch < 0f;
			_lastFrameClipTime = (_isPlayingBackward ? (ClipEndPosition + 1f) : (-1f));
			_waitMode = WaitForSoundFinishMode.WaitForEnd;
		}
	}

	private void StopOrChain()
	{
		SoundGroupVariation.PlaySoundParams playSoundParm = GrpVariation.PlaySoundParm;
		bool flag = playSoundParm.IsPlaying && playSoundParm.IsChainLoop;
		if (!VarAudio.loop || flag)
		{
			GrpVariation.Stop();
		}
		if (flag)
		{
			StopWaitingForFinish();
			MaybeChain();
		}
	}

	public void Pause()
	{
		_isPaused = true;
		_pauseTime = AudioSettings.dspTime;
	}

	public void Unpause()
	{
		_isPaused = false;
		if (_clipSchedEndTime.HasValue)
		{
			_clipSchedEndTime += AudioSettings.dspTime - _pauseTime;
			VarAudio.SetScheduledEndTime(_clipSchedEndTime.Value);
		}
	}

	public void MaybeChain()
	{
		if (_hasStartedNextInChain)
		{
			return;
		}
		_hasStartedNextInChain = true;
		SoundGroupVariation.PlaySoundParams playSoundParm = GrpVariation.PlaySoundParm;
		int num = MasterAudio.RemainingClipsInGroup(ParentGroup.GameObjectName);
		int num2 = MasterAudio.VoicesForGroup(ParentGroup.GameObjectName);
		if (num == num2)
		{
			ParentGroup.FireLastVariationFinishedPlay();
		}
		if (ParentGroup.chainLoopMode == MasterAudioGroup.ChainedLoopLoopMode.NumberOfLoops && ParentGroup.ChainLoopCount >= ParentGroup.chainLoopNumLoops)
		{
			return;
		}
		float delaySoundTime = playSoundParm.DelaySoundTime;
		if (ParentGroup.chainLoopDelayMin > 0f || ParentGroup.chainLoopDelayMax > 0f)
		{
			delaySoundTime = UnityEngine.Random.Range(ParentGroup.chainLoopDelayMin, ParentGroup.chainLoopDelayMax);
		}
		if (playSoundParm.AttachToSource || playSoundParm.SourceTrans != null)
		{
			if (playSoundParm.AttachToSource)
			{
				MasterAudio.PlaySound3DFollowTransform(playSoundParm.SoundType, playSoundParm.SourceTrans, playSoundParm.VolumePercentage, playSoundParm.Pitch, delaySoundTime, null, _clipSchedEndTime, isChaining: true);
			}
			else
			{
				MasterAudio.PlaySound3DAtTransform(playSoundParm.SoundType, playSoundParm.SourceTrans, playSoundParm.VolumePercentage, playSoundParm.Pitch, delaySoundTime, null, _clipSchedEndTime, isChaining: true);
			}
		}
		else
		{
			MasterAudio.PlaySound(playSoundParm.SoundType, playSoundParm.VolumePercentage, playSoundParm.Pitch, delaySoundTime, null, _clipSchedEndTime, isChaining: true);
		}
	}

	private void UpdatePitch()
	{
		SoundGroupVariation.PitchMode curPitchMode = GrpVariation.curPitchMode;
		if (curPitchMode != 0 && curPitchMode == SoundGroupVariation.PitchMode.Gliding && VarAudio.isPlaying)
		{
			_pitchGlideToTargetFrameNumber++;
			if (_pitchGlideToTargetFrameNumber >= _pitchGlideToTargetTotalFrames)
			{
				VarAudio.pitch = _glideToTargetPitch;
				StopPitchGliding();
			}
			else
			{
				VarAudio.pitch = (float)_pitchGlideToTargetFrameNumber * _pitchGlideToTargetFramePitchChange + _glideToTargetOrigPitch;
			}
		}
	}

	private void PerformFading()
	{
		switch (GrpVariation.curFadeMode)
		{
		case SoundGroupVariation.FadeMode.FadeInOut:
		{
			if (!VarAudio.isPlaying)
			{
				break;
			}
			float time = VarAudio.time;
			if (GrpVariation.fadeInTime > 0f && time < GrpVariation.fadeInTime)
			{
				VarAudio.volume = time * _fadeInOutInFactor;
			}
			else if (time >= GrpVariation.fadeInTime && !_hasFadeInOutSetMaxVolume)
			{
				VarAudio.volume = GrpVariation.fadeMaxVolume;
				_hasFadeInOutSetMaxVolume = true;
				if (!_fadeInOutWillFadeOut)
				{
					StopFading();
				}
			}
			else if (_fadeInOutWillFadeOut && time >= _fadeOutStartTime)
			{
				if (GrpVariation.PlaySoundParm.IsChainLoop && !_fadeOutStarted)
				{
					MaybeChain();
					_fadeOutStarted = true;
				}
				VarAudio.volume = (ClipEndPosition - time) * _fadeInOutOutFactor;
			}
			break;
		}
		case SoundGroupVariation.FadeMode.FadeOutEarly:
			if (VarAudio.isPlaying)
			{
				_fadeOutEarlyFrameNumber++;
				VarAudio.volume = (float)_fadeOutEarlyFrameNumber * _fadeOutEarlyFrameVolChange + _fadeOutEarlyOrigVol;
				if (_fadeOutEarlyFrameNumber >= _fadeOutEarlyTotalFrames)
				{
					GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.None;
					GrpVariation.Stop();
				}
			}
			break;
		case SoundGroupVariation.FadeMode.GradualFade:
			if (VarAudio.isPlaying)
			{
				_fadeToTargetFrameNumber++;
				if (_fadeToTargetFrameNumber >= _fadeToTargetTotalFrames)
				{
					VarAudio.volume = _fadeToTargetVolume;
					StopFading();
				}
				else
				{
					VarAudio.volume = (float)_fadeToTargetFrameNumber * _fadeToTargetFrameVolChange + _fadeToTargetOrigVol;
				}
			}
			break;
		case SoundGroupVariation.FadeMode.None:
			break;
		}
	}

	private void OnEnable()
	{
		_inited = false;
		_fadeInOutWillFadeOut = false;
		_hasFadeInOutSetMaxVolume = false;
		_fadeOutStarted = false;
		_hasStartedNextInChain = false;
		_framesPlayed = 0;
		_clipStartPosition = null;
		_clipEndPosition = null;
		DoneWithOcclusion();
		MasterAudio.RegisterUpdaterForUpdates(this);
	}

	private void OnDisable()
	{
		if (!MasterAudio.AppIsShuttingDown)
		{
			_framesPlayed = 0;
			DoneWithOcclusion();
			MasterAudio.UnregisterUpdaterForUpdates(this);
		}
	}

	public void UpdateCachedObjects()
	{
		_frameNum = AudioUtil.FrameCount;
		if (_maCachedFromFrame < _frameNum)
		{
			_maCachedFromFrame = _frameNum;
			_maThisFrame = MasterAudio.Instance;
			_listenerThisFrame = MasterAudio.ListenerTrans;
		}
	}

	public void ManualUpdate()
	{
		UpdateCachedObjects();
		_framesPlayed++;
		if (VarAudio.loop)
		{
			if (VarAudio.timeSamples < _lastFrameClipPosition)
			{
				_timesLooped++;
				if (VarAudio.loop && GrpVariation.useCustomLooping && _timesLooped >= GrpVariation.MaxLoops)
				{
					GrpVariation.Stop();
				}
				else
				{
					GrpVariation.SoundLoopStarted(_timesLooped);
				}
			}
			_lastFrameClipPosition = VarAudio.timeSamples;
		}
		if (_isFollowing && ParentGroup.targetDespawnedBehavior != 0 && (_objectToFollowGo == null || !DTMonoHelper.IsActive(_objectToFollowGo)))
		{
			switch (ParentGroup.targetDespawnedBehavior)
			{
			case MasterAudioGroup.TargetDespawnedBehavior.Stop:
				GrpVariation.Stop();
				break;
			case MasterAudioGroup.TargetDespawnedBehavior.FadeOut:
				GrpVariation.FadeOutNow(ParentGroup.despawnFadeTime);
				break;
			}
			StopFollowing();
		}
		PerformFading();
		UpdateAudioLocationAndPriority(rePrioritize: true);
		UpdateOcclusion();
		UpdatePitch();
		switch (_waitMode)
		{
		case WaitForSoundFinishMode.Play:
			PlaySoundAndWait();
			break;
		case WaitForSoundFinishMode.WaitForEnd:
		{
			if (_isPaused)
			{
				break;
			}
			if (_clipSchedEndTime.HasValue && AudioSettings.dspTime + 0.10000000149011612 >= _clipSchedEndTime.Value)
			{
				if (GrpVariation.PlaySoundParm.IsChainLoop && !_hasScheduledNextClip)
				{
					MaybeChain();
					_hasScheduledNextClip = true;
				}
				if (HasEndLinkedGroups && !_hasScheduledEndLinkedGroups)
				{
					GrpVariation.PlayEndLinkedGroups(_clipSchedEndTime.Value);
					_hasScheduledEndLinkedGroups = true;
				}
			}
			bool flag = false;
			if (_isPlayingBackward)
			{
				if (VarAudio.time > _lastFrameClipTime)
				{
					flag = true;
				}
			}
			else if (VarAudio.time < _lastFrameClipTime)
			{
				flag = true;
			}
			_lastFrameClipTime = VarAudio.time;
			if (flag)
			{
				_waitMode = WaitForSoundFinishMode.StopOrRepeat;
			}
			break;
		}
		case WaitForSoundFinishMode.StopOrRepeat:
			StopOrChain();
			break;
		case WaitForSoundFinishMode.None:
			break;
		}
	}
}
