using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkTonic.MasterAudio;

[RequireComponent(typeof(SoundGroupVariationUpdater))]
[AudioScriptOrder(-40)]
public class SoundGroupVariation : MonoBehaviour
{
	public delegate void SoundFinishedEventHandler();

	public delegate void SoundLoopedEventHandler(int loopNumberStarted);

	public class PlaySoundParams
	{
		public string SoundType;

		public float VolumePercentage;

		public float? Pitch;

		public double? TimeToSchedulePlay;

		public Transform SourceTrans;

		public bool AttachToSource;

		public float DelaySoundTime;

		public bool IsChainLoop;

		public bool IsSingleSubscribedPlay;

		public float GroupCalcVolume;

		public bool IsPlaying;

		public PlaySoundParams(string soundType, float volPercent, float groupCalcVolume, float? pitch, Transform sourceTrans, bool attach, float delaySoundTime, double? timeToSchedulePlay, bool isChainLoop, bool isSingleSubscribedPlay)
		{
			SoundType = soundType;
			VolumePercentage = volPercent;
			GroupCalcVolume = groupCalcVolume;
			Pitch = pitch;
			SourceTrans = sourceTrans;
			AttachToSource = attach;
			DelaySoundTime = delaySoundTime;
			TimeToSchedulePlay = timeToSchedulePlay;
			IsChainLoop = isChainLoop;
			IsSingleSubscribedPlay = isSingleSubscribedPlay;
			IsPlaying = false;
		}
	}

	public enum PitchMode
	{
		None,
		Gliding
	}

	public enum FadeMode
	{
		None,
		FadeInOut,
		FadeOutEarly,
		GradualFade
	}

	public enum RandomPitchMode
	{
		AddToClipPitch,
		IgnoreClipPitch
	}

	public enum RandomVolumeMode
	{
		AddToClipVolume,
		IgnoreClipVolume
	}

	public enum DetectEndMode
	{
		None,
		DetectEnd
	}

	public int weight = 1;

	[Range(0f, 1f)]
	public int probabilityToPlay = 100;

	public bool useLocalization;

	public bool useRandomPitch;

	public RandomPitchMode randomPitchMode;

	public float randomPitchMin;

	public float randomPitchMax;

	public bool useRandomVolume;

	public RandomVolumeMode randomVolumeMode;

	public float randomVolumeMin;

	public float randomVolumeMax;

	public MasterAudio.AudioLocation audLocation;

	public string resourceFileName;

	public string internetFileUrl;

	public MasterAudio.InternetFileLoadStatus internetFileLoadStatus;

	public float original_pitch;

	public float original_volume;

	public bool isExpanded = true;

	public bool isChecked = true;

	public bool useFades;

	public float fadeInTime;

	public float fadeOutTime;

	public bool useCustomLooping;

	public int minCustomLoops = 1;

	public int maxCustomLoops = 5;

	public bool useRandomStartTime;

	public float randomStartMinPercent;

	public float randomStartMaxPercent = 100f;

	public float randomEndPercent = 100f;

	public bool useIntroSilence;

	public float introSilenceMin;

	public float introSilenceMax;

	public float fadeMaxVolume;

	public FadeMode curFadeMode;

	public PitchMode curPitchMode;

	public DetectEndMode curDetectEndMode;

	public int frames;

	private AudioSource _audioSource;

	private readonly PlaySoundParams _playSndParam = new PlaySoundParams(string.Empty, 1f, 1f, 1f, null, attach: false, 0f, null, isChainLoop: false, isSingleSubscribedPlay: false);

	private AudioDistortionFilter _distFilter;

	private AudioEchoFilter _echoFilter;

	private AudioHighPassFilter _hpFilter;

	private AudioLowPassFilter _lpFilter;

	private AudioReverbFilter _reverbFilter;

	private AudioChorusFilter _chorusFilter;

	private float _maxVol = 1f;

	private int _instanceId = -1;

	private bool? _audioLoops;

	private int _maxLoops;

	private SoundGroupVariationUpdater _varUpdater;

	private int _previousSoundFinishedFrame = -1;

	private string _soundGroupName;

	private Transform _trans;

	private GameObject _go;

	private Transform _objectToFollow;

	private Transform _objectToTriggerFrom;

	private MasterAudioGroup _parentGroupScript;

	private bool _attachToSource;

	private string _resFileName = string.Empty;

	private bool _hasStartedEndLinkedGroups;

	public AudioDistortionFilter DistortionFilter
	{
		get
		{
			if (_distFilter != null)
			{
				return _distFilter;
			}
			_distFilter = GetComponent<AudioDistortionFilter>();
			return _distFilter;
		}
	}

	public AudioReverbFilter ReverbFilter
	{
		get
		{
			if (_reverbFilter != null)
			{
				return _reverbFilter;
			}
			_reverbFilter = GetComponent<AudioReverbFilter>();
			return _reverbFilter;
		}
	}

	public AudioChorusFilter ChorusFilter
	{
		get
		{
			if (_chorusFilter != null)
			{
				return _chorusFilter;
			}
			_chorusFilter = GetComponent<AudioChorusFilter>();
			return _chorusFilter;
		}
	}

	public AudioEchoFilter EchoFilter
	{
		get
		{
			if (_echoFilter != null)
			{
				return _echoFilter;
			}
			_echoFilter = GetComponent<AudioEchoFilter>();
			return _echoFilter;
		}
	}

	public AudioLowPassFilter LowPassFilter
	{
		get
		{
			return _lpFilter;
		}
		set
		{
			_lpFilter = value;
		}
	}

	public AudioHighPassFilter HighPassFilter
	{
		get
		{
			if (_hpFilter != null)
			{
				return _hpFilter;
			}
			_hpFilter = GetComponent<AudioHighPassFilter>();
			return _hpFilter;
		}
	}

	public Transform ObjectToFollow
	{
		get
		{
			return _objectToFollow;
		}
		set
		{
			_objectToFollow = value;
			UpdateTransformTracker(value);
		}
	}

	public Transform ObjectToTriggerFrom
	{
		get
		{
			return _objectToTriggerFrom;
		}
		set
		{
			_objectToTriggerFrom = value;
			UpdateTransformTracker(value);
		}
	}

	public bool HasActiveFXFilter
	{
		get
		{
			if (HighPassFilter != null && HighPassFilter.enabled)
			{
				return true;
			}
			if (LowPassFilter != null && LowPassFilter.enabled)
			{
				return true;
			}
			if (ReverbFilter != null && ReverbFilter.enabled)
			{
				return true;
			}
			if (DistortionFilter != null && DistortionFilter.enabled)
			{
				return true;
			}
			if (EchoFilter != null && EchoFilter.enabled)
			{
				return true;
			}
			if (ChorusFilter != null && ChorusFilter.enabled)
			{
				return true;
			}
			return false;
		}
	}

	public MasterAudioGroup ParentGroup
	{
		get
		{
			if (Trans.parent == null)
			{
				return null;
			}
			if (_parentGroupScript == null)
			{
				_parentGroupScript = Trans.parent.GetComponent<MasterAudioGroup>();
			}
			if (_parentGroupScript == null)
			{
				Debug.LogError("The Group that Sound Variation '" + base.name + "' is in does not have a MasterAudioGroup script in it!");
			}
			return _parentGroupScript;
		}
	}

	public float OriginalPitch
	{
		get
		{
			if (original_pitch == 0f)
			{
				original_pitch = VarAudio.pitch;
			}
			return original_pitch;
		}
	}

	public float OriginalVolume
	{
		get
		{
			if (original_volume == 0f)
			{
				original_volume = VarAudio.volume;
			}
			return original_volume;
		}
	}

	public string SoundGroupName
	{
		get
		{
			if (_soundGroupName != null)
			{
				return _soundGroupName;
			}
			_soundGroupName = ParentGroup.GameObjectName;
			return _soundGroupName;
		}
	}

	public bool IsAvailableToPlay
	{
		get
		{
			if (weight == 0)
			{
				return false;
			}
			if (!_playSndParam.IsPlaying && VarAudio.time == 0f)
			{
				return true;
			}
			return AudioUtil.GetAudioPlayedPercentage(VarAudio) >= (float)ParentGroup.retriggerPercentage;
		}
	}

	public float LastTimePlayed { get; set; }

	public bool IsPlaying => _playSndParam.IsPlaying;

	public int InstanceId
	{
		get
		{
			if (_instanceId < 0)
			{
				_instanceId = GetInstanceID();
			}
			return _instanceId;
		}
	}

	public Transform Trans
	{
		get
		{
			if (_trans != null)
			{
				return _trans;
			}
			_trans = base.transform;
			return _trans;
		}
	}

	public GameObject GameObj
	{
		get
		{
			if (_go != null)
			{
				return _go;
			}
			_go = base.gameObject;
			return _go;
		}
	}

	public AudioSource VarAudio
	{
		get
		{
			if (_audioSource != null)
			{
				return _audioSource;
			}
			_audioSource = GetComponent<AudioSource>();
			return _audioSource;
		}
	}

	public bool AudioLoops
	{
		get
		{
			if (!_audioLoops.HasValue)
			{
				_audioLoops = VarAudio.loop;
			}
			return _audioLoops.Value;
		}
	}

	public string ResFileName
	{
		get
		{
			if (string.IsNullOrEmpty(_resFileName))
			{
				_resFileName = AudioResourceOptimizer.GetLocalizedFileName(useLocalization, resourceFileName);
			}
			return _resFileName;
		}
	}

	public SoundGroupVariationUpdater VariationUpdater
	{
		get
		{
			if (_varUpdater != null)
			{
				return _varUpdater;
			}
			_varUpdater = GetComponent<SoundGroupVariationUpdater>();
			return _varUpdater;
		}
	}

	public PlaySoundParams PlaySoundParm => _playSndParam;

	public float SetGroupVolume
	{
		get
		{
			return _playSndParam.GroupCalcVolume;
		}
		set
		{
			_playSndParam.GroupCalcVolume = value;
		}
	}

	public int MaxLoops => _maxLoops;

	private bool Is2D => VarAudio.spatialBlend <= 0f;

	private bool ShouldLoadAsync
	{
		get
		{
			if (MasterAudio.Instance.resourceClipsAllLoadAsync)
			{
				return true;
			}
			return ParentGroup.resourceClipsAllLoadAsync;
		}
	}

	public bool UsesOcclusion
	{
		get
		{
			if (!VariationUpdater.MAThisFrame.useOcclusion)
			{
				return false;
			}
			if (Is2D)
			{
				return false;
			}
			MasterAudio.OcclusionSelectionType occlusionSelectType = VariationUpdater.MAThisFrame.occlusionSelectType;
			if (occlusionSelectType == MasterAudio.OcclusionSelectionType.AllGroups || occlusionSelectType != MasterAudio.OcclusionSelectionType.TurnOnPerBusOrGroup)
			{
				return true;
			}
			if (ParentGroup.isUsingOcclusion)
			{
				return true;
			}
			GroupBus busForGroup = ParentGroup.BusForGroup;
			if (busForGroup != null && busForGroup.isUsingOcclusion)
			{
				return true;
			}
			return false;
		}
	}

	public event SoundFinishedEventHandler SoundFinished;

	public event SoundLoopedEventHandler SoundLooped;

	private void Awake()
	{
		original_pitch = VarAudio.pitch;
		original_volume = VarAudio.volume;
		_audioLoops = VarAudio.loop;
		AudioClip clip = VarAudio.clip;
		GameObject gameObj = GameObj;
		if (!(clip != null))
		{
			_ = gameObj != null;
		}
		if (VarAudio.playOnAwake)
		{
			Debug.LogWarning("The 'Play on Awake' checkbox in the Audio Source for Sound Group '" + ParentGroup.name + "', Variation '" + base.name + "' is checked. This is not used in Master Audio and can lead to buggy behavior. Make sure to uncheck it before hitting Play next time. To play ambient sounds, use an EventSounds component and activate the Start event to play a Sound Group of your choice.");
		}
	}

	private void Start()
	{
		if (ParentGroup == null)
		{
			Debug.LogError("Sound Variation '" + base.name + "' has no parent!");
			return;
		}
		GameObj.layer = MasterAudio.Instance.gameObject.layer;
		if (audLocation == MasterAudio.AudioLocation.FileOnInternet && internetFileLoadStatus == MasterAudio.InternetFileLoadStatus.Loading)
		{
			LoadInternetFile();
		}
		SetMixerGroup();
		SetSpatialBlend();
		SetPriority();
		SetOcclusion();
		SpatializerHelper.TurnOnSpatializerIfEnabled(VarAudio);
	}

	public void SetMixerGroup()
	{
		GroupBus busForGroup = ParentGroup.BusForGroup;
		if (busForGroup != null)
		{
			VarAudio.outputAudioMixerGroup = busForGroup.mixerChannel;
		}
		else
		{
			VarAudio.outputAudioMixerGroup = null;
		}
	}

	public void SetSpatialBlend()
	{
		float spatialBlendForGroup = ParentGroup.SpatialBlendForGroup;
		if (spatialBlendForGroup != -99f)
		{
			VarAudio.spatialBlend = spatialBlendForGroup;
		}
		GroupBus busForGroup = ParentGroup.BusForGroup;
		if (busForGroup != null && MasterAudio.Instance.mixerSpatialBlendType != 0 && busForGroup.forceTo2D)
		{
			VarAudio.spatialBlend = 0f;
		}
	}

	public void LoadInternetFile()
	{
		StartCoroutine(AudioResourceOptimizer.PopulateSourceWithInternetFile(internetFileUrl, this, InternetFileLoaded, InternetFileFailedToLoad));
	}

	private void SetOcclusion()
	{
		VariationUpdater.UpdateCachedObjects();
		if (!UsesOcclusion)
		{
			return;
		}
		if (LowPassFilter == null)
		{
			_lpFilter = GetComponent<AudioLowPassFilter>();
			if (_lpFilter == null)
			{
				_lpFilter = base.gameObject.AddComponent<AudioLowPassFilter>();
			}
		}
		else
		{
			_lpFilter = GetComponent<AudioLowPassFilter>();
		}
		LowPassFilter.cutoffFrequency = AudioUtil.MinCutoffFreq(VariationUpdater);
	}

	private void SetPriority()
	{
		if (MasterAudio.Instance.prioritizeOnDistance)
		{
			if (ParentGroup.alwaysHighestPriority)
			{
				AudioPrioritizer.Set2DSoundPriority(VarAudio);
			}
			else
			{
				AudioPrioritizer.SetSoundGroupInitialPriority(VarAudio);
			}
		}
	}

	public void DisableUpdater()
	{
		if (!(VariationUpdater == null))
		{
			VariationUpdater.enabled = false;
		}
	}

	private void OnDestroy()
	{
		StopSoundEarly();
	}

	private void OnDisable()
	{
		StopSoundEarly();
	}

	private void StopSoundEarly()
	{
		if (!MasterAudio.AppIsShuttingDown)
		{
			Stop();
		}
	}

	private void OnDrawGizmos()
	{
		if (MasterAudio.Instance.showGizmos && IsPlaying)
		{
			Gizmos.DrawIcon(base.transform.position, "MasterAudio/MasterAudio Icon.png", allowScaling: true);
		}
	}

	public void Play(float? pitch, float maxVolume, string gameObjectName, float volPercent, float targetVol, float? targetPitch, Transform sourceTrans, bool attach, float delayTime, double? timeToSchedulePlay, bool isChaining, bool isSingleSubscribedPlay)
	{
		if (!MasterAudio.IsWarming && audLocation == MasterAudio.AudioLocation.FileOnInternet)
		{
			switch (internetFileLoadStatus)
			{
			case MasterAudio.InternetFileLoadStatus.Loading:
				if (ParentGroup.LoggingEnabledForGroup)
				{
					MasterAudio.LogWarning("Cannot play Variation '" + base.name + "' because its Internet file has not been downloaded yet.");
				}
				return;
			case MasterAudio.InternetFileLoadStatus.Failed:
				if (ParentGroup.LoggingEnabledForGroup)
				{
					MasterAudio.LogWarning("Cannot play Variation '" + base.name + "' because its Internet file failed downloading.");
				}
				return;
			}
		}
		MaybeCleanupFinishedDelegate();
		_hasStartedEndLinkedGroups = false;
		SetPlaySoundParams(gameObjectName, volPercent, targetVol, targetPitch, sourceTrans, attach, delayTime, timeToSchedulePlay, isChaining, isSingleSubscribedPlay);
		SetPriority();
		if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync)
		{
			StopAllCoroutines();
		}
		if (pitch.HasValue)
		{
			VarAudio.pitch = pitch.Value;
		}
		else if (useRandomPitch)
		{
			float num = UnityEngine.Random.Range(randomPitchMin, randomPitchMax);
			if (randomPitchMode == RandomPitchMode.AddToClipPitch)
			{
				num += OriginalPitch;
			}
			VarAudio.pitch = num;
		}
		else
		{
			VarAudio.pitch = OriginalPitch;
		}
		SetSpatialBlend();
		curFadeMode = FadeMode.None;
		curPitchMode = PitchMode.None;
		curDetectEndMode = DetectEndMode.DetectEnd;
		_maxVol = maxVolume;
		if (maxCustomLoops == minCustomLoops)
		{
			_maxLoops = minCustomLoops;
		}
		else
		{
			_maxLoops = UnityEngine.Random.Range(minCustomLoops, maxCustomLoops + 1);
		}
		switch (audLocation)
		{
		case MasterAudio.AudioLocation.Clip:
			FinishSetupToPlay();
			break;
		case MasterAudio.AudioLocation.ResourceFile:
			if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync)
			{
				StartCoroutine(AudioResourceOptimizer.PopulateSourcesWithResourceClipAsync(ResFileName, this, FinishSetupToPlay, ResourceFailedToLoad));
			}
			else if (AudioResourceOptimizer.PopulateSourcesWithResourceClip(ResFileName, this))
			{
				FinishSetupToPlay();
			}
			break;
		case MasterAudio.AudioLocation.FileOnInternet:
			FinishSetupToPlay();
			break;
		}
	}

	public void SetPlaySoundParams(string gameObjectName, float volPercent, float targetVol, float? targetPitch, Transform sourceTrans, bool attach, float delayTime, double? timeToSchedulePlay, bool isChaining, bool isSingleSubscribedPlay)
	{
		_playSndParam.SoundType = gameObjectName;
		_playSndParam.VolumePercentage = volPercent;
		_playSndParam.GroupCalcVolume = targetVol;
		_playSndParam.Pitch = targetPitch;
		_playSndParam.SourceTrans = sourceTrans;
		_playSndParam.AttachToSource = attach;
		_playSndParam.DelaySoundTime = delayTime;
		_playSndParam.TimeToSchedulePlay = timeToSchedulePlay;
		_playSndParam.IsChainLoop = isChaining || ParentGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
		_playSndParam.IsSingleSubscribedPlay = isSingleSubscribedPlay;
		_playSndParam.IsPlaying = true;
	}

	private void InternetFileFailedToLoad()
	{
		internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Failed;
	}

	private void MaybeCleanupFinishedDelegate()
	{
		if (ParentGroup.willCleanUpDelegatesAfterStop)
		{
			ClearSubscribers();
		}
	}

	private void InternetFileLoaded()
	{
		if (ParentGroup.LoggingEnabledForGroup)
		{
			MasterAudio.LogWarning("Internet file: '" + internetFileUrl + "' loaded successfully.");
		}
		internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
	}

	private void ResourceFailedToLoad()
	{
		Stop();
	}

	private void FinishSetupToPlay()
	{
		if ((VarAudio.isPlaying || !(VarAudio.time > 0f)) && useFades && (fadeInTime > 0f || fadeOutTime > 0f))
		{
			fadeMaxVolume = _maxVol;
			if (fadeInTime > 0f)
			{
				VarAudio.volume = 0f;
			}
			if (VariationUpdater != null)
			{
				EnableUpdater(waitForSoundFinish: false);
				VariationUpdater.FadeInOut();
			}
		}
		VarAudio.loop = AudioLoops;
		if (_playSndParam.IsPlaying && (_playSndParam.IsChainLoop || _playSndParam.IsSingleSubscribedPlay || useRandomStartTime))
		{
			VarAudio.loop = false;
		}
		if (!_playSndParam.IsPlaying)
		{
			return;
		}
		ParentGroup.AddActiveAudioSourceId(InstanceId);
		EnableUpdater();
		_attachToSource = false;
		bool flag = MasterAudio.Instance.prioritizeOnDistance && (MasterAudio.Instance.useClipAgePriority || ParentGroup.useClipAgePriority);
		if (_playSndParam.AttachToSource || flag)
		{
			_attachToSource = _playSndParam.AttachToSource;
			if (VariationUpdater != null)
			{
				VariationUpdater.FollowObject(_attachToSource, ObjectToFollow, flag);
			}
		}
	}

	public void JumpToTime(float timeToJumpTo)
	{
		if (_playSndParam.IsPlaying)
		{
			VarAudio.time = timeToJumpTo;
		}
	}

	public void GlideByPitch(float pitchAddition, float glideTime, Action completionCallback = null)
	{
		if (pitchAddition == 0f)
		{
			completionCallback?.Invoke();
			return;
		}
		float num = VarAudio.pitch + pitchAddition;
		if (num < -3f)
		{
			num = -3f;
		}
		if (num > 3f)
		{
			num = 3f;
		}
		if (!VarAudio.clip.IsClipReadyToPlay())
		{
			if (ParentGroup.LoggingEnabledForGroup)
			{
				MasterAudio.LogWarning("Cannot GlideToPitch Variation '" + base.name + "' because it is still loading.");
			}
		}
		else if (glideTime <= 0.1f)
		{
			if (VarAudio.pitch != num)
			{
				VarAudio.pitch = num;
			}
			completionCallback?.Invoke();
		}
		else if (VariationUpdater != null)
		{
			VariationUpdater.GlidePitch(num, glideTime, completionCallback);
		}
	}

	public void AdjustVolume(float volumePercentage)
	{
		if (!_playSndParam.IsPlaying)
		{
			return;
		}
		float volume = _playSndParam.GroupCalcVolume * volumePercentage;
		VarAudio.volume = volume;
		_playSndParam.VolumePercentage = volumePercentage;
		List<MasterAudio.AudioInfo> allVariationsOfGroup = MasterAudio.GetAllVariationsOfGroup(ParentGroup.name);
		for (int i = 0; i < allVariationsOfGroup.Count; i++)
		{
			MasterAudio.AudioInfo audioInfo = allVariationsOfGroup[i];
			if (!(audioInfo.Variation != this))
			{
				audioInfo.LastPercentageVolume = volumePercentage;
				break;
			}
		}
	}

	public void Pause()
	{
		if (!MasterAudio.Instance.resourceClipsPauseDoNotUnload)
		{
			switch (audLocation)
			{
			case MasterAudio.AudioLocation.ResourceFile:
				Stop();
				return;
			case MasterAudio.AudioLocation.Clip:
				if (!AudioUtil.AudioClipWillPreload(VarAudio.clip))
				{
					Stop();
					return;
				}
				break;
			}
		}
		VarAudio.Pause();
		if (VariationUpdater.enabled)
		{
			VariationUpdater.Pause();
		}
		curFadeMode = FadeMode.None;
		curPitchMode = PitchMode.None;
	}

	public void DoNextChain(float volumePercentage, float? pitch, Transform transActor, bool attach)
	{
		EnableUpdater(waitForSoundFinish: false);
		SetPlaySoundParams(ParentGroup.GameObjectName, volumePercentage, volumePercentage, pitch, transActor, attach, 0f, null, isChaining: true, isSingleSubscribedPlay: false);
		VariationUpdater.MaybeChain();
		VariationUpdater.StopWaitingForFinish();
	}

	public void PlayEndLinkedGroups(double? timeToPlayClip = null)
	{
		if (MasterAudio.AppIsShuttingDown || MasterAudio.IsWarming || ParentGroup.endLinkedGroups.Count == 0 || _hasStartedEndLinkedGroups)
		{
			return;
		}
		_hasStartedEndLinkedGroups = true;
		if (VariationUpdater == null || VariationUpdater.FramesPlayed == 0)
		{
			return;
		}
		switch (ParentGroup.linkedStopGroupSelectionType)
		{
		case MasterAudio.LinkedGroupSelectionType.All:
		{
			for (int i = 0; i < ParentGroup.endLinkedGroups.Count; i++)
			{
				PlayEndLinkedGroup(ParentGroup.endLinkedGroups[i], timeToPlayClip);
			}
			break;
		}
		case MasterAudio.LinkedGroupSelectionType.OneAtRandom:
		{
			int index = UnityEngine.Random.Range(0, ParentGroup.endLinkedGroups.Count);
			PlayEndLinkedGroup(ParentGroup.endLinkedGroups[index], timeToPlayClip);
			break;
		}
		}
	}

	private void EnableUpdater(bool waitForSoundFinish = true)
	{
		if (VariationUpdater != null)
		{
			VariationUpdater.enabled = true;
			VariationUpdater.Initialize();
			if (waitForSoundFinish)
			{
				VariationUpdater.WaitForSoundFinish();
			}
		}
	}

	private void MaybeUnloadClip()
	{
		if (audLocation == MasterAudio.AudioLocation.ResourceFile)
		{
			AudioResourceOptimizer.UnloadClipIfUnused(_resFileName);
		}
		AudioUtil.UnloadNonPreloadedAudioData(VarAudio.clip, GameObj);
	}

	private void PlayEndLinkedGroup(string sType, double? timeToPlayClip = null)
	{
		if (_playSndParam.AttachToSource && _playSndParam.SourceTrans != null)
		{
			MasterAudio.PlaySound3DFollowTransformAndForget(sType, _playSndParam.SourceTrans, _playSndParam.VolumePercentage, _playSndParam.Pitch, 0f, null, timeToPlayClip);
		}
		else if (_playSndParam.SourceTrans != null)
		{
			MasterAudio.PlaySound3DAtTransformAndForget(sType, _playSndParam.SourceTrans, _playSndParam.VolumePercentage, _playSndParam.Pitch, 0f, null, timeToPlayClip);
		}
		else
		{
			MasterAudio.PlaySound3DAtVector3AndForget(sType, Trans.position, _playSndParam.VolumePercentage, _playSndParam.Pitch, 0f, null, timeToPlayClip);
		}
	}

	public void Stop(bool stopEndDetection = false, bool skipLinked = false)
	{
		bool flag = false;
		if (stopEndDetection && VariationUpdater != null)
		{
			VariationUpdater.StopWaitingForFinish();
			flag = true;
		}
		if (!skipLinked)
		{
			PlayEndLinkedGroups();
		}
		_objectToFollow = null;
		_objectToTriggerFrom = null;
		VarAudio.pitch = OriginalPitch;
		ParentGroup.RemoveActiveAudioSourceId(InstanceId);
		MasterAudio.StopTrackingOcclusionForSource(GameObj);
		VarAudio.Stop();
		VarAudio.time = 0f;
		if (VariationUpdater != null)
		{
			VariationUpdater.StopFollowing();
			VariationUpdater.StopFading();
			VariationUpdater.StopPitchGliding();
		}
		if (!flag && VariationUpdater != null)
		{
			VariationUpdater.StopWaitingForFinish();
		}
		_playSndParam.IsPlaying = false;
		if (this.SoundFinished != null)
		{
			bool num = _previousSoundFinishedFrame == AudioUtil.FrameCount;
			_previousSoundFinishedFrame = AudioUtil.FrameCount;
			if (!num)
			{
				this.SoundFinished();
			}
			MaybeCleanupFinishedDelegate();
		}
		Trans.localPosition = Vector3.zero;
		MaybeUnloadClip();
	}

	public void FadeToVolume(float newVolume, float fadeTime)
	{
		if (newVolume < 0f || newVolume > 1f)
		{
			Debug.LogError("Illegal volume passed to FadeToVolume: '" + newVolume + "'. Legal volumes are between 0 and 1.");
		}
		else if (!VarAudio.clip.IsClipReadyToPlay())
		{
			if (ParentGroup.LoggingEnabledForGroup)
			{
				MasterAudio.LogWarning("Cannot Fade Variation '" + base.name + "' because it is still loading.");
			}
		}
		else if (fadeTime <= 0.1f)
		{
			VarAudio.volume = newVolume;
			if (VarAudio.volume <= 0f)
			{
				Stop();
			}
		}
		else if (VariationUpdater != null)
		{
			VariationUpdater.FadeOverTimeToVolume(newVolume, fadeTime);
		}
	}

	public void FadeOutNow()
	{
		if (!MasterAudio.AppIsShuttingDown && IsPlaying && useFades && VariationUpdater != null)
		{
			VariationUpdater.FadeOutEarly(fadeOutTime);
		}
	}

	public void FadeOutNow(float fadeTime)
	{
		if (!MasterAudio.AppIsShuttingDown && IsPlaying && VariationUpdater != null)
		{
			VariationUpdater.FadeOutEarly(fadeTime);
		}
	}

	public bool WasTriggeredFromTransform(Transform trans)
	{
		if (ObjectToFollow == trans || ObjectToTriggerFrom == trans)
		{
			return true;
		}
		return false;
	}

	public bool WasTriggeredFromAnyOfTransformMap(HashSet<Transform> transMap)
	{
		if (ObjectToFollow != null && transMap.Contains(ObjectToFollow))
		{
			return true;
		}
		if (ObjectToTriggerFrom != null && transMap.Contains(ObjectToTriggerFrom))
		{
			return true;
		}
		return false;
	}

	public void UpdateTransformTracker(Transform sourceTrans)
	{
		if (!(sourceTrans == null) && Application.isEditor && !MasterAudio.IsWarming && sourceTrans.GetComponent<AudioTransformTracker>() == null)
		{
			sourceTrans.gameObject.AddComponent<AudioTransformTracker>();
		}
	}

	public void SoundLoopStarted(int numberOfLoops)
	{
		if (this.SoundLooped != null)
		{
			this.SoundLooped(numberOfLoops);
		}
	}

	public void ClearSubscribers()
	{
		this.SoundFinished = null;
		this.SoundLooped = null;
	}
}
