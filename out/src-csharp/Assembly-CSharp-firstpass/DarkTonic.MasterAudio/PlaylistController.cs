using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DarkTonic.MasterAudio;

[AudioScriptOrder(-80)]
[RequireComponent(typeof(AudioSource))]
public class PlaylistController : MonoBehaviour
{
	public enum AudioPlayType
	{
		PlayNow,
		Schedule,
		AlreadyScheduled
	}

	public enum PlaylistStates
	{
		NotInScene,
		Stopped,
		Playing,
		Paused,
		Crossfading
	}

	public enum FadeMode
	{
		None,
		GradualFade
	}

	public enum AudioDuckingMode
	{
		NotDucking,
		SetToDuck,
		Ducked
	}

	public delegate void SongChangedEventHandler(string newSongName);

	public delegate void SongEndedEventHandler(string songName);

	public delegate void SongLoopedEventHandler(string songName);

	public const float ScheduledSongMinBadOffset = 0.5f;

	public const int FramesEarlyToTrigger = 2;

	public const int FramesEarlyToBeSyncable = 10;

	private const string NotReadyMessage = "Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.";

	private const float MinSongLength = 0.5f;

	private const float SlowestFrameTimeForCalc = 0.3f;

	public bool startPlaylistOnAwake = true;

	public bool isShuffle;

	public bool isAutoAdvance = true;

	public bool loopPlaylist = true;

	public float _playlistVolume = 1f;

	public bool isMuted;

	public string startPlaylistName = string.Empty;

	public int syncGroupNum = -1;

	public AudioMixerGroup mixerChannel;

	public MasterAudio.ItemSpatialBlendType spatialBlendType;

	public float spatialBlend;

	public bool initializedEventExpanded;

	public string initializedCustomEvent = string.Empty;

	public bool crossfadeStartedExpanded;

	public string crossfadeStartedCustomEvent = string.Empty;

	public bool songChangedEventExpanded;

	public string songChangedCustomEvent = string.Empty;

	public bool songEndedEventExpanded;

	public string songEndedCustomEvent = string.Empty;

	public bool songLoopedEventExpanded;

	public string songLoopedCustomEvent = string.Empty;

	public bool playlistStartedEventExpanded;

	public string playlistStartedCustomEvent = string.Empty;

	public bool playlistEndedEventExpanded;

	public string playlistEndedCustomEvent = string.Empty;

	private AudioSource _activeAudio;

	private AudioSource _transitioningAudio;

	private float _activeAudioEndVolume;

	private float _transitioningAudioStartVolume;

	private float _crossFadeStartTime;

	private readonly List<int> _clipsRemaining = new List<int>(10);

	private int _currentSequentialClipIndex;

	private AudioDuckingMode _duckingMode;

	private float _timeToStartUnducking;

	private float _timeToFinishUnducking;

	private float _originalMusicVolume;

	private float _initialDuckVolume;

	private float _duckRange;

	private MusicSetting _currentSong;

	private GameObject _go;

	private string _name;

	private FadeMode _curFadeMode;

	private float _slowFadeStartTime;

	private float _slowFadeCompletionTime;

	private float _slowFadeStartVolume;

	private float _slowFadeTargetVolume;

	private MasterAudio.Playlist _currentPlaylist;

	private float _lastTimeMissingPlaylistLogged = -5f;

	private Action _fadeCompleteCallback;

	private readonly List<MusicSetting> _queuedSongs = new List<MusicSetting>(5);

	private bool _lostFocus;

	private bool _autoStartedPlaylist;

	private AudioSource _audioClip;

	private AudioSource _transClip;

	private MusicSetting _newSongSetting;

	private bool _nextSongRequested;

	private bool _nextSongScheduled;

	private int _lastRandomClipIndex = -1;

	private float _lastTimeSongRequested = -1f;

	private float _currentDuckVolCut;

	private int? _lastSongPosition;

	private double? _currentSchedSongDspStartTime;

	private double? _currentSchedSongDspEndTime;

	private int _lastFrameSongPosition = -1;

	private readonly Dictionary<AudioSource, double> _scheduledSongOffsetByAudioSource = new Dictionary<AudioSource, double>(2);

	public int _frames;

	private static List<PlaylistController> _instances;

	private int _songsPlayedFromPlaylist;

	private AudioSource _audio1;

	private AudioSource _audio2;

	private Transform _trans;

	private bool _willPersist;

	private double? _songPauseTime;

	private int framesOfSongPlayed;

	private bool SongIsNonAdvancible
	{
		get
		{
			if (CurrentPlaylist != null && CurrentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips)
			{
				return CrossFadeTime > 0f;
			}
			return false;
		}
	}

	private bool ShouldLoadAsync
	{
		get
		{
			if (MasterAudio.Instance.resourceClipsAllLoadAsync)
			{
				return true;
			}
			return CurrentPlaylist.resourceClipsAllLoadAsync;
		}
	}

	public bool ControllerIsReady { get; private set; }

	public PlaylistStates PlaylistState
	{
		get
		{
			if (_activeAudio == null || _transitioningAudio == null)
			{
				return PlaylistStates.NotInScene;
			}
			if (!ActiveAudioSource.isPlaying)
			{
				if (ActiveAudioSource.time != 0f)
				{
					return PlaylistStates.Paused;
				}
				return PlaylistStates.Stopped;
			}
			if (IsCrossFading)
			{
				return PlaylistStates.Crossfading;
			}
			return PlaylistStates.Playing;
		}
	}

	public AudioSource ActiveAudioSource
	{
		get
		{
			if (_activeAudio != null && _activeAudio.clip == null)
			{
				return _transitioningAudio;
			}
			return _activeAudio;
		}
	}

	public static List<PlaylistController> Instances
	{
		get
		{
			if (_instances != null)
			{
				return _instances;
			}
			_instances = new List<PlaylistController>();
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(PlaylistController));
			for (int i = 0; i < array.Length; i++)
			{
				_instances.Add(array[i] as PlaylistController);
			}
			return _instances;
		}
		set
		{
			_instances = value;
		}
	}

	public GameObject PlaylistControllerGameObject => _go;

	public AudioSource CurrentPlaylistSource => _activeAudio;

	public AudioClip CurrentPlaylistClip
	{
		get
		{
			if (_activeAudio == null)
			{
				return null;
			}
			return _activeAudio.clip;
		}
	}

	public AudioClip FadingPlaylistClip
	{
		get
		{
			if (!IsCrossFading)
			{
				return null;
			}
			if (_transitioningAudio == null)
			{
				return null;
			}
			return _transitioningAudio.clip;
		}
	}

	public AudioSource FadingSource
	{
		get
		{
			if (!IsCrossFading)
			{
				return null;
			}
			return _transitioningAudio;
		}
	}

	public bool IsCrossFading { get; private set; }

	public bool IsFading
	{
		get
		{
			if (!IsCrossFading)
			{
				return _curFadeMode != FadeMode.None;
			}
			return true;
		}
	}

	public float PlaylistVolume
	{
		get
		{
			return MasterAudio.PlaylistMasterVolume * _playlistVolume;
		}
		set
		{
			_playlistVolume = value;
			UpdateMasterVolume();
		}
	}

	public MasterAudio.Playlist CurrentPlaylist
	{
		get
		{
			if (_currentPlaylist != null || !(Time.realtimeSinceStartup - _lastTimeMissingPlaylistLogged > 2f))
			{
				return _currentPlaylist;
			}
			Debug.LogWarning("Current Playlist is NULL. Subsequent calls will fail.");
			_lastTimeMissingPlaylistLogged = AudioUtil.Time;
			return _currentPlaylist;
		}
	}

	public bool HasPlaylist => _currentPlaylist != null;

	public string PlaylistName
	{
		get
		{
			if (CurrentPlaylist == null)
			{
				return string.Empty;
			}
			return CurrentPlaylist.playlistName;
		}
	}

	private bool IsMuted => isMuted;

	private bool PlaylistIsMuted
	{
		set
		{
			isMuted = value;
			if (Application.isPlaying)
			{
				if (_activeAudio != null)
				{
					_activeAudio.mute = value;
				}
				if (_transitioningAudio != null)
				{
					_transitioningAudio.mute = value;
				}
			}
			else
			{
				AudioSource[] components = GetComponents<AudioSource>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].mute = value;
				}
			}
		}
	}

	private float CrossFadeTime
	{
		get
		{
			if (_currentPlaylist != null)
			{
				if (_currentPlaylist.crossfadeMode != 0)
				{
					return _currentPlaylist.crossFadeTime;
				}
				return MasterAudio.Instance.MasterCrossFadeTime;
			}
			return MasterAudio.Instance.MasterCrossFadeTime;
		}
	}

	private bool IsAutoAdvance
	{
		get
		{
			if (SongIsNonAdvancible)
			{
				return false;
			}
			return isAutoAdvance;
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

	public string ControllerName
	{
		get
		{
			if (_name != null)
			{
				return _name;
			}
			_name = GameObj.name;
			return _name;
		}
	}

	public bool CanSchedule
	{
		get
		{
			if (MasterAudio.Instance.useGaplessPlaylists)
			{
				return IsAutoAdvance;
			}
			return false;
		}
	}

	private bool IsFrameFastEnough => AudioUtil.FrameTime < 0.3f;

	private bool ShouldNotSwitchEarly
	{
		get
		{
			if (CrossFadeTime <= 0f)
			{
				return CanSchedule;
			}
			return false;
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
			_trans = base.transform;
			return _trans;
		}
	}

	public int ClipsRemainingInCurrentPlaylist => _clipsRemaining.Count;

	public event SongChangedEventHandler SongChanged;

	public event SongEndedEventHandler SongEnded;

	public event SongLoopedEventHandler SongLooped;

	private void Awake()
	{
		base.useGUILayout = false;
		if (ControllerIsReady)
		{
			return;
		}
		ControllerIsReady = false;
		PlaylistController[] array = (PlaylistController[])UnityEngine.Object.FindObjectsOfType(typeof(PlaylistController));
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].ControllerName == ControllerName)
			{
				num++;
			}
		}
		if (num > 1)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			Debug.Log("More than one Playlist Controller prefab exists in this Scene with the same name. Destroying the one called '" + ControllerName + "'. You may wish to set up a Bootstrapper Scene so this does not occur.");
			return;
		}
		_autoStartedPlaylist = false;
		_duckingMode = AudioDuckingMode.NotDucking;
		_currentSong = null;
		_songsPlayedFromPlaylist = 0;
		AudioSource[] components = GetComponents<AudioSource>();
		if (components.Length < 2)
		{
			Debug.LogError("This prefab should have exactly two Audio Source components. Please revert it.");
			return;
		}
		MasterAudio safeInstance = MasterAudio.SafeInstance;
		_willPersist = safeInstance != null && safeInstance.persistBetweenScenes;
		_audio1 = components[0];
		_audio2 = components[1];
		_audio1.clip = null;
		_audio2.clip = null;
		if (_audio1.playOnAwake || _audio2.playOnAwake)
		{
			Debug.LogWarning("One or more 'Play on Awake' checkboxes in the Audio Sources on Playlist Controller '" + base.name + "' are checked. These are not used in Master Audio. Make sure to uncheck them before hitting Play next time. For Playlist Controllers, use the similarly named checkbox 'Start Playlist on Awake' in the Playlist Controller's Inspector.");
		}
		_activeAudio = _audio1;
		_transitioningAudio = _audio2;
		_audio1.outputAudioMixerGroup = mixerChannel;
		_audio2.outputAudioMixerGroup = mixerChannel;
		SetSpatialBlend();
		_curFadeMode = FadeMode.None;
		_fadeCompleteCallback = null;
		_lostFocus = false;
	}

	public void SetSpatialBlend()
	{
		if (MasterAudio.SafeInstance == null)
		{
			return;
		}
		switch (MasterAudio.Instance.musicSpatialBlendType)
		{
		case MasterAudio.AllMusicSpatialBlendType.ForceAllTo2D:
			SetAudioSpatialBlend(0f);
			break;
		case MasterAudio.AllMusicSpatialBlendType.ForceAllTo3D:
			SetAudioSpatialBlend(1f);
			break;
		case MasterAudio.AllMusicSpatialBlendType.ForceAllToCustom:
			SetAudioSpatialBlend(MasterAudio.Instance.musicSpatialBlend);
			break;
		case MasterAudio.AllMusicSpatialBlendType.AllowDifferentPerController:
			switch (spatialBlendType)
			{
			case MasterAudio.ItemSpatialBlendType.ForceTo2D:
				SetAudioSpatialBlend(0f);
				break;
			case MasterAudio.ItemSpatialBlendType.ForceTo3D:
				SetAudioSpatialBlend(1f);
				break;
			case MasterAudio.ItemSpatialBlendType.ForceToCustom:
				SetAudioSpatialBlend(spatialBlend);
				break;
			case MasterAudio.ItemSpatialBlendType.UseCurveFromAudioSource:
				break;
			}
			break;
		}
	}

	private void SetAudiosIfEmpty()
	{
		AudioSource[] components = GetComponents<AudioSource>();
		_audio1 = components[0];
		_audio2 = components[1];
	}

	private void SetAudioSpatialBlend(float blend)
	{
		if (_audio1 == null)
		{
			SetAudiosIfEmpty();
		}
		_audio1.spatialBlend = blend;
		_audio2.spatialBlend = blend;
	}

	private void Start()
	{
		if (ControllerIsReady)
		{
			return;
		}
		if (MasterAudio.SafeInstance == null)
		{
			Debug.LogError("No Master Audio game object exists in the Hierarchy. Aborting Playlist Controller setup code.");
			return;
		}
		if (!string.IsNullOrEmpty(startPlaylistName) && _currentPlaylist == null)
		{
			InitializePlaylist();
		}
		ControllerIsReady = true;
		if (initializedEventExpanded && initializedCustomEvent != string.Empty && initializedCustomEvent != "[None]")
		{
			MasterAudio.FireCustomEvent(initializedCustomEvent, Trans, logDupe: false);
		}
		AutoStartPlaylist();
		if (IsMuted)
		{
			MutePlaylist();
		}
	}

	private void AutoStartPlaylist()
	{
		if (_currentPlaylist != null && startPlaylistOnAwake && IsFrameFastEnough && !_autoStartedPlaylist)
		{
			PlayNextOrRandom(AudioPlayType.PlayNow);
			_autoStartedPlaylist = true;
		}
	}

	private void CoUpdate()
	{
		if (MasterAudio.SafeInstance == null || _curFadeMode != FadeMode.GradualFade || _activeAudio == null)
		{
			return;
		}
		float val = 1f - (_slowFadeCompletionTime - AudioUtil.Time) / (_slowFadeCompletionTime - _slowFadeStartTime);
		val = Math.Min(val, 1f);
		val = Math.Max(val, 0f);
		float val2 = _slowFadeStartVolume + (_slowFadeTargetVolume - _slowFadeStartVolume) * val;
		val2 = ((!(_slowFadeTargetVolume > _slowFadeStartVolume)) ? Math.Max(val2, _slowFadeTargetVolume) : Math.Min(val2, _slowFadeTargetVolume));
		_playlistVolume = val2;
		UpdateMasterVolume();
		if (!(AudioUtil.Time < _slowFadeCompletionTime))
		{
			if (MasterAudio.Instance.stopZeroVolumePlaylists && _slowFadeTargetVolume == 0f)
			{
				StopPlaylist();
			}
			if (_fadeCompleteCallback != null)
			{
				_fadeCompleteCallback();
				_fadeCompleteCallback = null;
			}
			_curFadeMode = FadeMode.None;
		}
	}

	private void OnEnable()
	{
		_instances = null;
		MasterAudio.TrackRuntimeAudioSources(new List<AudioSource> { _audio1, _audio2 });
	}

	private void OnDisable()
	{
		_instances = null;
		if (!(MasterAudio.SafeInstance == null) && !MasterAudio.AppIsShuttingDown)
		{
			if (ActiveAudioSource != null && ActiveAudioSource.clip != null && !_willPersist)
			{
				StopPlaylist();
			}
			MasterAudio.StopTrackingRuntimeAudioSources(new List<AudioSource> { _audio1, _audio2 });
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		_lostFocus = pauseStatus;
	}

	private void Update()
	{
		_frames++;
		CoUpdate();
		if (_lostFocus || !ControllerIsReady)
		{
			return;
		}
		AutoStartPlaylist();
		if (_activeAudio.isPlaying)
		{
			framesOfSongPlayed++;
		}
		if (IsCrossFading)
		{
			if (_activeAudio.volume >= _activeAudioEndVolume)
			{
				CeaseAudioSource(_transitioningAudio);
				IsCrossFading = false;
				if (CanSchedule && !_nextSongScheduled)
				{
					PlayNextOrRandom(AudioPlayType.Schedule);
				}
				SetDuckProperties();
			}
			float num = Math.Max(CrossFadeTime, 0.001f);
			float num2 = (Time.realtimeSinceStartup - _crossFadeStartTime) / num;
			_activeAudio.volume = num2 * _activeAudioEndVolume;
			_transitioningAudio.volume = _transitioningAudioStartVolume * (1f - num2);
		}
		if (!_activeAudio.loop && _activeAudio.clip != null)
		{
			if (AudioUtil.IsAudioPaused(_activeAudio))
			{
				goto IL_03ae;
			}
			if (!_activeAudio.isPlaying)
			{
				if (!IsAutoAdvance)
				{
					FirePlaylistEndedEventIfAny();
					CeaseAudioSource(_activeAudio);
					return;
				}
				bool flag = false;
				flag = ((!isShuffle) ? (_currentSequentialClipIndex >= _currentPlaylist.MusicSettings.Count) : (_clipsRemaining.Count == 0));
				if (flag && !_activeAudio.isPlaying)
				{
					FirePlaylistEndedEventIfAny();
					CeaseAudioSource(_activeAudio);
					return;
				}
			}
			bool flag2 = false;
			if (ShouldNotSwitchEarly)
			{
				if (_currentSchedSongDspStartTime.HasValue && AudioSettings.dspTime > _currentSchedSongDspStartTime.Value)
				{
					flag2 = true;
				}
			}
			else if (PlaylistState == PlaylistStates.Stopped)
			{
				flag2 = true;
			}
			else if (IsFrameFastEnough)
			{
				float num3 = _activeAudio.clip.length - _activeAudio.time - AudioUtil.AdjustEndLeadTimeForPitch(CrossFadeTime, _activeAudio);
				float num4 = AudioUtil.AdjustEndLeadTimeForPitch(AudioUtil.FrameTime * 2f, _activeAudio);
				flag2 = num3 <= num4;
			}
			if (flag2)
			{
				if (_currentPlaylist.fadeOutLastSong)
				{
					if (isShuffle)
					{
						if (_clipsRemaining.Count == 0 || !IsAutoAdvance)
						{
							FadeOutPlaylist();
							return;
						}
					}
					else if (_currentSequentialClipIndex >= _currentPlaylist.MusicSettings.Count || _currentPlaylist.MusicSettings.Count == 1 || !IsAutoAdvance)
					{
						FadeOutPlaylist();
						return;
					}
				}
				if (IsAutoAdvance && !_nextSongRequested && _lastTimeSongRequested + 0.5f <= AudioUtil.Time)
				{
					_lastTimeSongRequested = AudioUtil.Time;
					if (CanSchedule)
					{
						_lastSongPosition = null;
						FadeInScheduledSong();
					}
					else
					{
						_lastSongPosition = 0;
						PlayNextOrRandom(AudioPlayType.PlayNow);
					}
				}
			}
		}
		if (_activeAudio.loop && _activeAudio.clip != null)
		{
			if (_activeAudio.timeSamples < _lastFrameSongPosition)
			{
				string text = _activeAudio.clip.name;
				if (this.SongLooped != null && !string.IsNullOrEmpty(text))
				{
					this.SongLooped(text);
				}
				if (songLoopedEventExpanded && songLoopedCustomEvent != string.Empty && songLoopedCustomEvent != "[None]")
				{
					MasterAudio.FireCustomEvent(songLoopedCustomEvent, Trans, logDupe: false);
				}
			}
			_lastFrameSongPosition = _activeAudio.timeSamples;
		}
		goto IL_03ae;
		IL_03ae:
		if (!IsCrossFading)
		{
			AudioDucking();
		}
	}

	public static PlaylistController InstanceByName(string playlistControllerName, bool errorIfNotFound = true)
	{
		PlaylistController playlistController = Instances.Find((PlaylistController obj) => obj != null && obj.ControllerName == playlistControllerName);
		if (playlistController != null)
		{
			return playlistController;
		}
		if (errorIfNotFound)
		{
			Debug.LogError("Could not find Playlist Controller '" + playlistControllerName + "'.");
		}
		return null;
	}

	public bool IsSongPlaying(string songName)
	{
		if (!HasPlaylist)
		{
			return false;
		}
		if (ActiveAudioSource == null || ActiveAudioSource.clip == null)
		{
			return false;
		}
		return ActiveAudioSource.clip.name == songName;
	}

	public void ClearQueue()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else
		{
			_queuedSongs.Clear();
		}
	}

	public void ToggleMutePlaylist()
	{
		if (Application.isPlaying && !ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else if (IsMuted)
		{
			UnmutePlaylist();
		}
		else
		{
			MutePlaylist();
		}
	}

	public void MutePlaylist()
	{
		PlaylistIsMuted = true;
	}

	public void UnmutePlaylist()
	{
		PlaylistIsMuted = false;
	}

	public void PausePlaylist()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else if (!(_activeAudio == null) && !(_transitioningAudio == null))
		{
			if (_activeAudio.clip != null)
			{
				_activeAudio.Pause();
			}
			if (!_songPauseTime.HasValue)
			{
				_songPauseTime = AudioSettings.dspTime;
			}
			if (_transitioningAudio.clip != null)
			{
				_transitioningAudio.Pause();
			}
		}
	}

	public bool UnpausePlaylist()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			_songPauseTime = null;
			return false;
		}
		if (PlaylistState == PlaylistStates.Playing || PlaylistState == PlaylistStates.Crossfading || PlaylistState == PlaylistStates.Stopped)
		{
			_songPauseTime = null;
			return false;
		}
		if (_activeAudio == null || _transitioningAudio == null)
		{
			_songPauseTime = null;
			return false;
		}
		if (_activeAudio.clip == null && _currentPlaylist != null)
		{
			FinishPlaylistInit();
			_songPauseTime = null;
			return true;
		}
		if (_activeAudio.clip == null)
		{
			_songPauseTime = null;
			return false;
		}
		if (!_scheduledSongOffsetByAudioSource.ContainsKey(_activeAudio))
		{
			_activeAudio.Play();
			framesOfSongPlayed = 0;
			AudioUtil.ClipPlayed(_activeAudio.clip, GameObj);
		}
		else if (_songPauseTime.HasValue && _currentSchedSongDspStartTime.HasValue)
		{
			double num = AudioSettings.dspTime - _songPauseTime.Value;
			double scheduledPlayTimeOffset = _currentSchedSongDspStartTime.Value - AudioSettings.dspTime + num;
			_songPauseTime = null;
			_activeAudio.Stop();
			ScheduleClipPlay(scheduledPlayTimeOffset, _activeAudio, calledAfterPause: true);
		}
		if (!_scheduledSongOffsetByAudioSource.ContainsKey(_transitioningAudio))
		{
			_transitioningAudio.Play();
			AudioUtil.ClipPlayed(_transitioningAudio.clip, GameObj);
		}
		else if (_songPauseTime.HasValue && _currentSchedSongDspStartTime.HasValue)
		{
			double num2 = AudioSettings.dspTime - _songPauseTime.Value;
			double scheduledPlayTimeOffset2 = _currentSchedSongDspStartTime.Value - AudioSettings.dspTime + num2;
			_songPauseTime = null;
			_transitioningAudio.Stop();
			ScheduleClipPlay(scheduledPlayTimeOffset2, _transitioningAudio, calledAfterPause: true);
		}
		return true;
	}

	public void StopPlaylist(bool onlyFadingClip = false)
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else
		{
			if (!Application.isPlaying)
			{
				return;
			}
			_currentSchedSongDspStartTime = null;
			_currentSchedSongDspEndTime = null;
			_currentSong = null;
			PlaylistStates playlistState = PlaylistState;
			if ((uint)playlistState > 1u)
			{
				if (!onlyFadingClip)
				{
					CeaseAudioSource(_activeAudio);
				}
				CeaseAudioSource(_transitioningAudio);
			}
		}
	}

	public void FadeToVolume(float targetVolume, float fadeTime, Action callback = null)
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			return;
		}
		if (fadeTime <= 0.1f)
		{
			_playlistVolume = targetVolume;
			UpdateMasterVolume();
			_curFadeMode = FadeMode.None;
			return;
		}
		_curFadeMode = FadeMode.GradualFade;
		if (_duckingMode == AudioDuckingMode.NotDucking)
		{
			_slowFadeStartVolume = _playlistVolume;
		}
		else
		{
			_slowFadeStartVolume = _activeAudio.volume;
		}
		_slowFadeTargetVolume = targetVolume;
		_slowFadeStartTime = AudioUtil.Time;
		_slowFadeCompletionTime = AudioUtil.Time + fadeTime;
		_fadeCompleteCallback = callback;
	}

	public void PlayRandomSong()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else
		{
			PlayARandomSong(AudioPlayType.PlayNow);
		}
	}

	public void PlayARandomSong(AudioPlayType playType)
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else if (_clipsRemaining.Count == 0)
		{
			Debug.LogWarning("There are no clips left in this Playlist. Turn on Loop Playlist if you want to loop the entire song selection.");
		}
		else
		{
			if (IsCrossFading && playType == AudioPlayType.Schedule)
			{
				return;
			}
			if (framesOfSongPlayed > 0)
			{
				_nextSongScheduled = false;
			}
			int num = UnityEngine.Random.Range(0, _clipsRemaining.Count);
			int index = _clipsRemaining[num];
			switch (playType)
			{
			case AudioPlayType.PlayNow:
				RemoveRandomClip(num);
				break;
			case AudioPlayType.Schedule:
				_lastRandomClipIndex = num;
				break;
			case AudioPlayType.AlreadyScheduled:
				if (_lastRandomClipIndex >= 0)
				{
					RemoveRandomClip(_lastRandomClipIndex);
				}
				break;
			}
			PlaySong(_currentPlaylist.MusicSettings[index], playType);
		}
	}

	private void RemoveRandomClip(int randIndex)
	{
		_clipsRemaining.RemoveAt(randIndex);
		if (loopPlaylist && _clipsRemaining.Count == 0)
		{
			FillClips();
		}
	}

	private void PlayFirstQueuedSong(AudioPlayType playType)
	{
		if (_queuedSongs.Count == 0)
		{
			Debug.LogWarning("There are zero queued songs in PlaylistController '" + ControllerName + "'. Cannot play first queued song.");
			return;
		}
		MusicSetting musicSetting = _queuedSongs[0];
		_queuedSongs.RemoveAt(0);
		_currentSequentialClipIndex = musicSetting.songIndex;
		PlaySong(musicSetting, playType);
	}

	public void PlayNextSong()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else
		{
			PlayTheNextSong(AudioPlayType.PlayNow);
		}
	}

	public void PlayTheNextSong(AudioPlayType playType)
	{
		if (_currentPlaylist == null || (IsCrossFading && playType == AudioPlayType.Schedule))
		{
			return;
		}
		if (playType != AudioPlayType.AlreadyScheduled && _songsPlayedFromPlaylist > 0 && !_nextSongScheduled)
		{
			AdvanceSongCounter();
		}
		if (_currentSequentialClipIndex >= _currentPlaylist.MusicSettings.Count)
		{
			Debug.LogWarning("There are no clips left in this Playlist. Turn on Loop Playlist if you want to loop the entire song selection.");
			return;
		}
		if (framesOfSongPlayed > 0)
		{
			_nextSongScheduled = false;
			_lastSongPosition = ActiveAudioSource.timeSamples;
		}
		PlaySong(_currentPlaylist.MusicSettings[_currentSequentialClipIndex], playType);
	}

	private void AdvanceSongCounter()
	{
		_currentSequentialClipIndex++;
		if (_currentSequentialClipIndex >= _currentPlaylist.MusicSettings.Count && loopPlaylist)
		{
			_currentSequentialClipIndex = 0;
		}
	}

	public void StopPlaylistAfterCurrentSong()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			return;
		}
		if (_currentPlaylist == null)
		{
			MasterAudio.LogNoPlaylist(ControllerName, "StopPlaylistAfterCurrentSong");
			return;
		}
		if (!_activeAudio.isPlaying)
		{
			Debug.Log("No song is currently playing.");
			return;
		}
		_activeAudio.loop = false;
		_queuedSongs.Clear();
		isAutoAdvance = false;
		if (_scheduledSongOffsetByAudioSource.ContainsKey(_activeAudio))
		{
			CeaseAudioSource(_activeAudio);
		}
		if (_scheduledSongOffsetByAudioSource.ContainsKey(_transitioningAudio))
		{
			CeaseAudioSource(_transitioningAudio);
		}
	}

	public void StopLoopingCurrentSong()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			return;
		}
		if (_currentPlaylist == null)
		{
			MasterAudio.LogNoPlaylist(ControllerName, "StopLoopingCurrentSong");
			return;
		}
		if (!_activeAudio.isPlaying)
		{
			Debug.Log("No song is currently playing.");
			return;
		}
		_activeAudio.loop = false;
		if (CanSchedule && _queuedSongs.Count == 0)
		{
			ScheduleNextSong();
		}
	}

	public void QueuePlaylistClip(string clipName, bool scheduleNow = true)
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			return;
		}
		if (_currentPlaylist == null)
		{
			MasterAudio.LogNoPlaylist(ControllerName, "QueuePlaylistClip");
			return;
		}
		if (!_activeAudio.isPlaying)
		{
			TriggerPlaylistClip(clipName);
			return;
		}
		MusicSetting musicSetting = _currentPlaylist.MusicSettings.Find((MusicSetting obj) => (obj.audLocation == MasterAudio.AudioLocation.Clip) ? (obj.clip != null && obj.clip.name == clipName) : (obj.resourceFileName == clipName));
		if (musicSetting == null)
		{
			Debug.LogWarning("Could not find clip '" + clipName + "' in current Playlist in '" + ControllerName + "'.");
		}
		else
		{
			_activeAudio.loop = false;
			_queuedSongs.Add(musicSetting);
			if (CanSchedule && scheduleNow)
			{
				PlayNextOrRandom(AudioPlayType.Schedule);
			}
		}
	}

	public bool TriggerPlaylistClip(string clipName)
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			return false;
		}
		if (_currentPlaylist == null)
		{
			MasterAudio.LogNoPlaylist(ControllerName, "TriggerPlaylistClip");
			return false;
		}
		MusicSetting musicSetting = _currentPlaylist.MusicSettings.Find((MusicSetting obj) => obj.alias == clipName);
		if (musicSetting == null)
		{
			musicSetting = _currentPlaylist.MusicSettings.Find(delegate(MusicSetting obj)
			{
				if (obj.audLocation == MasterAudio.AudioLocation.Clip)
				{
					if (obj.clip != null)
					{
						return obj.clip.name == clipName;
					}
					return false;
				}
				return obj.resourceFileName == clipName || obj.songName == clipName;
			});
		}
		if (musicSetting == null)
		{
			Debug.LogWarning("Could not find clip '" + clipName + "' in current Playlist in '" + ControllerName + "'.");
			return false;
		}
		_currentSequentialClipIndex = musicSetting.songIndex;
		PlaySong(musicSetting, AudioPlayType.PlayNow);
		return true;
	}

	public void DuckMusicForTime(float duckLength, float unduckTime, float pitch, float duckedTimePercentage, float duckedVolCut)
	{
		if (MasterAudio.IsWarming)
		{
			return;
		}
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else if (!IsCrossFading)
		{
			float num = AudioUtil.AdjustAudioClipDurationForPitch(duckLength, pitch);
			_currentDuckVolCut = duckedVolCut;
			float num2 = (_initialDuckVolume = AudioUtil.GetFloatVolumeFromDb(AudioUtil.GetDbFromFloatVolume(_originalMusicVolume) + duckedVolCut));
			_duckRange = _originalMusicVolume - num2;
			_duckingMode = AudioDuckingMode.SetToDuck;
			_timeToStartUnducking = AudioUtil.Time + num * duckedTimePercentage;
			float num3 = _timeToStartUnducking + unduckTime;
			if (num3 > AudioUtil.Time + num)
			{
				num3 = AudioUtil.Time + num;
			}
			_timeToFinishUnducking = num3;
		}
	}

	private void InitControllerIfNot()
	{
		if (!ControllerIsReady)
		{
			Awake();
			Start();
		}
	}

	public void UpdateMasterVolume()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		InitControllerIfNot();
		if (_currentSong != null)
		{
			float num = _currentSong.volume * PlaylistVolume;
			if (!IsCrossFading)
			{
				if (_activeAudio != null)
				{
					_activeAudio.volume = num;
				}
				if (_transitioningAudio != null)
				{
					_transitioningAudio.volume = num;
				}
			}
			_activeAudioEndVolume = num;
		}
		SetDuckProperties();
	}

	public void StartPlaylist(string playlistName)
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else if (_currentPlaylist != null && _currentPlaylist.playlistName == playlistName)
		{
			RestartPlaylist();
		}
		else
		{
			ChangePlaylist(playlistName);
		}
	}

	public void ChangePlaylist(string playlistName, bool playFirstClip = true)
	{
		InitControllerIfNot();
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
			return;
		}
		if (_currentPlaylist != null && _currentPlaylist.playlistName == playlistName)
		{
			Debug.LogWarning("The Playlist '" + playlistName + "' is already loaded. Ignoring Change Playlist request.");
			return;
		}
		startPlaylistName = playlistName;
		FinishPlaylistInit(playFirstClip);
	}

	private void FinishPlaylistInit(bool playFirstClip = true)
	{
		if (IsCrossFading)
		{
			StopPlaylist(onlyFadingClip: true);
		}
		InitializePlaylist();
		if (Application.isPlaying)
		{
			_queuedSongs.Clear();
			if (playFirstClip)
			{
				PlayNextOrRandom(AudioPlayType.PlayNow);
			}
		}
	}

	public void RestartPlaylist()
	{
		if (!ControllerIsReady)
		{
			Debug.LogError("Playlist Controller is not initialized yet. It must call its own Awake & Start method before any other methods are called. If you have a script with an Awake or Start event that needs to call it, make sure PlaylistController.cs is set to execute first (Script Execution Order window in Unity). Awake event is still not guaranteed to work, so use Start where possible.");
		}
		else
		{
			FinishPlaylistInit();
		}
	}

	private void CheckIfPlaylistStarted()
	{
		if (_songsPlayedFromPlaylist <= 0 && playlistStartedEventExpanded && playlistStartedCustomEvent != string.Empty && playlistStartedCustomEvent != "[None]")
		{
			MasterAudio.FireCustomEvent(playlistStartedCustomEvent, Trans);
		}
	}

	private PlaylistController FindOtherControllerInSameSyncGroup()
	{
		if (syncGroupNum <= 0 || _currentPlaylist.songTransitionType != MasterAudio.SongFadeInPosition.SynchronizeClips)
		{
			return null;
		}
		return Instances.Find((PlaylistController obj) => obj != this && obj.syncGroupNum == syncGroupNum && obj.ActiveAudioSource != null && obj.ActiveAudioSource.isPlaying);
	}

	private void FadeOutPlaylist()
	{
		if (_curFadeMode != FadeMode.GradualFade)
		{
			float volumeBeforeFade = _playlistVolume;
			FadeToVolume(0f, CrossFadeTime, delegate
			{
				StopPlaylist();
				_playlistVolume = volumeBeforeFade;
			});
		}
	}

	private void InitializePlaylist()
	{
		FillClips();
		_songsPlayedFromPlaylist = 0;
		_currentSequentialClipIndex = 0;
		_nextSongScheduled = false;
		_lastRandomClipIndex = -1;
	}

	private void PlayNextOrRandom(AudioPlayType playType)
	{
		_nextSongRequested = true;
		if (_queuedSongs.Count > 0)
		{
			PlayFirstQueuedSong(playType);
		}
		else if (!isShuffle)
		{
			PlayTheNextSong(playType);
		}
		else
		{
			PlayARandomSong(playType);
		}
	}

	private void FirePlaylistEndedEventIfAny()
	{
		if (playlistEndedEventExpanded && playlistEndedCustomEvent != string.Empty && playlistStartedCustomEvent != "[None]")
		{
			MasterAudio.FireCustomEvent(playlistEndedCustomEvent, Trans);
		}
	}

	private void FillClips()
	{
		_clipsRemaining.Clear();
		if (startPlaylistName == "[No Playlist]")
		{
			return;
		}
		_currentPlaylist = MasterAudio.GrabPlaylist(startPlaylistName);
		if (_currentPlaylist == null)
		{
			return;
		}
		for (int i = 0; i < _currentPlaylist.MusicSettings.Count; i++)
		{
			MusicSetting musicSetting = _currentPlaylist.MusicSettings[i];
			musicSetting.songIndex = i;
			if (musicSetting.audLocation != MasterAudio.AudioLocation.ResourceFile)
			{
				if (musicSetting.clip == null)
				{
					continue;
				}
			}
			else if (string.IsNullOrEmpty(musicSetting.resourceFileName))
			{
				continue;
			}
			_clipsRemaining.Add(i);
		}
	}

	private void PlaySong(MusicSetting setting, AudioPlayType playType)
	{
		_newSongSetting = setting;
		if (_activeAudio == null)
		{
			Debug.LogError("PlaylistController prefab is not in your scene. Cannot play a song.");
			return;
		}
		AudioClip audioClip = null;
		int num;
		if (playType != 0)
		{
			num = ((playType == AudioPlayType.AlreadyScheduled) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0036;
			}
		}
		else
		{
			num = 1;
		}
		_lastFrameSongPosition = -1;
		goto IL_0036;
		IL_0036:
		if (num != 0 && _currentSong != null && !CanSchedule && _currentSong.songChangedEventExpanded && _currentSong.songChangedCustomEvent != string.Empty && _currentSong.songChangedCustomEvent != "[None]")
		{
			MasterAudio.FireCustomEvent(_currentSong.songChangedCustomEvent, Trans);
		}
		if (playType != AudioPlayType.AlreadyScheduled)
		{
			if (_activeAudio.clip != null)
			{
				string value = string.Empty;
				switch (setting.audLocation)
				{
				case MasterAudio.AudioLocation.Clip:
					if (setting.clip != null)
					{
						value = setting.clip.name;
					}
					break;
				case MasterAudio.AudioLocation.ResourceFile:
					value = setting.resourceFileName;
					break;
				}
				if (string.IsNullOrEmpty(value))
				{
					Debug.LogWarning("The next song has no clip or Resource file assigned. Please fix. Ignoring song change request.");
					return;
				}
			}
			if (_activeAudio.clip == null)
			{
				_audioClip = _activeAudio;
				_transClip = _transitioningAudio;
			}
			else if (_transitioningAudio.clip == null)
			{
				_audioClip = _transitioningAudio;
				_transClip = _activeAudio;
			}
			else
			{
				_audioClip = _transitioningAudio;
				_transClip = _activeAudio;
			}
			_audioClip.loop = SongShouldLoop(setting);
			switch (setting.audLocation)
			{
			case MasterAudio.AudioLocation.Clip:
				if (setting.clip == null)
				{
					MasterAudio.LogWarning("MasterAudio will not play empty Playlist clip for PlaylistController '" + ControllerName + "'.");
					return;
				}
				audioClip = setting.clip;
				break;
			case MasterAudio.AudioLocation.ResourceFile:
				if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync)
				{
					StartCoroutine(AudioResourceOptimizer.PopulateResourceSongToPlaylistControllerAsync(setting.resourceFileName, CurrentPlaylist.playlistName, this, playType));
					break;
				}
				audioClip = AudioResourceOptimizer.PopulateResourceSongToPlaylistController(ControllerName, setting.resourceFileName, CurrentPlaylist.playlistName);
				if (audioClip == null)
				{
					return;
				}
				break;
			}
		}
		else
		{
			FinishLoadingNewSong(null, AudioPlayType.AlreadyScheduled);
		}
		if (audioClip != null)
		{
			FinishLoadingNewSong(audioClip, playType);
		}
	}

	public double? ScheduledGaplessNextSongStartTime()
	{
		if (!_scheduledSongOffsetByAudioSource.ContainsKey(_audioClip))
		{
			return null;
		}
		return _scheduledSongOffsetByAudioSource[_audioClip];
	}

	public void FinishLoadingNewSong(AudioClip clipToPlay, AudioPlayType playType)
	{
		_nextSongRequested = false;
		bool flag = playType == AudioPlayType.Schedule;
		bool num = playType == AudioPlayType.PlayNow || flag;
		bool flag2 = playType == AudioPlayType.PlayNow || playType == AudioPlayType.AlreadyScheduled;
		if (num)
		{
			_audioClip.clip = clipToPlay;
			_audioClip.pitch = _newSongSetting.pitch;
		}
		if (_currentSong != null)
		{
			_currentSong.lastKnownTimePoint = _activeAudio.timeSamples;
			_currentSong.wasLastKnownTimePointSet = true;
		}
		if (flag2)
		{
			if (CrossFadeTime == 0f || _transClip.clip == null)
			{
				CeaseAudioSource(_transClip);
				_audioClip.volume = _newSongSetting.volume * PlaylistVolume;
				if (!ActiveAudioSource.isPlaying && _currentPlaylist != null && _currentPlaylist.fadeInFirstSong && CrossFadeTime > 0f)
				{
					CrossFadeNow(_audioClip);
				}
			}
			else
			{
				CrossFadeNow(_audioClip);
			}
			SetDuckProperties();
		}
		switch (playType)
		{
		case AudioPlayType.AlreadyScheduled:
			_nextSongScheduled = false;
			RemoveScheduledClip();
			break;
		case AudioPlayType.PlayNow:
			if (_audioClip.clip != null && _audioClip.timeSamples >= _audioClip.clip.samples - 1)
			{
				_audioClip.timeSamples = 0;
			}
			_audioClip.Play();
			framesOfSongPlayed = 0;
			AudioUtil.ClipPlayed(_activeAudio.clip, GameObj);
			CheckIfPlaylistStarted();
			_songsPlayedFromPlaylist++;
			break;
		case AudioPlayType.Schedule:
		{
			double scheduledPlayTimeOffset = CalculateNextTrackStartTimeOffset();
			ScheduleClipPlay(scheduledPlayTimeOffset, _audioClip, calledAfterPause: false);
			_nextSongScheduled = true;
			CheckIfPlaylistStarted();
			_songsPlayedFromPlaylist++;
			break;
		}
		}
		bool flag3 = false;
		if (playType == AudioPlayType.PlayNow)
		{
			PlaylistController playlistController = FindOtherControllerInSameSyncGroup();
			if (playlistController != null)
			{
				int timeSamples = playlistController._activeAudio.timeSamples;
				float time = playlistController._audioClip.time;
				bool flag4 = Math.Abs(_audioClip.clip.length - time) >= AudioUtil.FrameTime * 10f;
				if (_audioClip.clip != null && timeSamples < _audioClip.clip.samples && flag4)
				{
					_audioClip.timeSamples = timeSamples;
					flag3 = true;
				}
			}
		}
		if (_currentPlaylist != null)
		{
			if (_songsPlayedFromPlaylist <= 1 && !flag3)
			{
				_audioClip.timeSamples = 0;
			}
			else
			{
				switch (_currentPlaylist.songTransitionType)
				{
				case MasterAudio.SongFadeInPosition.SynchronizeClips:
					if (flag3)
					{
						break;
					}
					if (playType == AudioPlayType.PlayNow)
					{
						int num2 = (_lastSongPosition.HasValue ? _lastSongPosition.Value : _activeAudio.timeSamples);
						if (_transitioningAudio.clip != null && num2 >= _transitioningAudio.clip.samples - 1)
						{
							num2 = 0;
						}
						_lastSongPosition = null;
						_transitioningAudio.timeSamples = num2;
					}
					else if (!ShouldNotSwitchEarly)
					{
						_transitioningAudio.timeSamples = 0;
					}
					break;
				case MasterAudio.SongFadeInPosition.NewClipFromLastKnownPosition:
				{
					MusicSetting musicSetting = _currentPlaylist.MusicSettings.Find((MusicSetting obj) => obj == _newSongSetting);
					if (musicSetting != null)
					{
						int num3 = musicSetting.lastKnownTimePoint;
						if (_transitioningAudio.clip != null && num3 >= _transitioningAudio.clip.samples - 1)
						{
							num3 = 0;
						}
						_transitioningAudio.timeSamples = num3;
					}
					break;
				}
				case MasterAudio.SongFadeInPosition.NewClipFromBeginning:
					if (!ShouldNotSwitchEarly)
					{
						_audioClip.timeSamples = 0;
					}
					break;
				}
			}
			bool flag5 = _currentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.NewClipFromLastKnownPosition && !_newSongSetting.wasLastKnownTimePointSet;
			if (_currentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.NewClipFromBeginning || flag5)
			{
				float songStartTime = _newSongSetting.SongStartTime;
				if (songStartTime > 0f)
				{
					_audioClip.timeSamples = (int)(songStartTime * (float)_audioClip.clip.frequency);
				}
			}
		}
		if (flag)
		{
			UpdateMasterVolume();
		}
		if (flag2)
		{
			_activeAudio = _audioClip;
			_transitioningAudio = _transClip;
			if (songChangedCustomEvent != string.Empty && songChangedEventExpanded && songChangedCustomEvent != "[None]")
			{
				MasterAudio.FireCustomEvent(songChangedCustomEvent, Trans);
			}
			if (this.SongChanged != null)
			{
				string empty = string.Empty;
				if (_audioClip != null)
				{
					empty = _audioClip.clip.name;
				}
				this.SongChanged(empty);
			}
		}
		_activeAudioEndVolume = _newSongSetting.volume * PlaylistVolume;
		float volume = _transitioningAudio.volume;
		if (_currentSong != null)
		{
			volume = _currentSong.volume;
		}
		_transitioningAudioStartVolume = volume * PlaylistVolume;
		_currentSong = _newSongSetting;
		if (flag2 && _currentSong.songStartedEventExpanded && _currentSong.songStartedCustomEvent != string.Empty && _currentSong.songStartedCustomEvent != "[None]")
		{
			MasterAudio.FireCustomEvent(_currentSong.songStartedCustomEvent, Trans);
		}
		if (CanSchedule && playType != AudioPlayType.Schedule && !_currentSong.isLoop)
		{
			ScheduleNextSong();
		}
	}

	private void RemoveScheduledClip()
	{
		if (_audioClip != null)
		{
			_scheduledSongOffsetByAudioSource.Remove(_audioClip);
		}
	}

	private void ScheduleNextSong()
	{
		PlayNextOrRandom(AudioPlayType.Schedule);
	}

	private void FadeInScheduledSong()
	{
		PlayNextOrRandom(AudioPlayType.AlreadyScheduled);
	}

	private double CalculateNextTrackStartTimeOffset()
	{
		PlaylistController playlistController = FindOtherControllerInSameSyncGroup();
		if (playlistController != null)
		{
			double? num = playlistController.ScheduledGaplessNextSongStartTime();
			if (num.HasValue)
			{
				return num.Value;
			}
		}
		return GetClipDuration(_activeAudio);
	}

	private double GetClipDuration(AudioSource src)
	{
		return AudioUtil.AdjustAudioClipDurationForPitch(src.clip.length - src.time, src) - CrossFadeTime;
	}

	private void ScheduleClipPlay(double scheduledPlayTimeOffset, AudioSource source, bool calledAfterPause)
	{
		double num = AudioSettings.dspTime + scheduledPlayTimeOffset;
		if (ShouldNotSwitchEarly && _currentSchedSongDspEndTime.HasValue && !calledAfterPause)
		{
			num = _currentSchedSongDspEndTime.Value;
			scheduledPlayTimeOffset = num - AudioSettings.dspTime;
		}
		source.PlayScheduled(num);
		_currentSchedSongDspStartTime = num;
		_currentSchedSongDspEndTime = num + GetClipDuration(source);
		RemoveScheduledClip();
		_scheduledSongOffsetByAudioSource.Add(source, scheduledPlayTimeOffset);
	}

	private void CrossFadeNow(AudioSource audioClip)
	{
		audioClip.volume = 0f;
		IsCrossFading = true;
		_duckingMode = AudioDuckingMode.NotDucking;
		_crossFadeStartTime = AudioUtil.Time;
		if (crossfadeStartedExpanded && crossfadeStartedCustomEvent != string.Empty && crossfadeStartedCustomEvent != "[None]")
		{
			MasterAudio.FireCustomEvent(crossfadeStartedCustomEvent, Trans, logDupe: false);
		}
	}

	private void CeaseAudioSource(AudioSource source)
	{
		if (source == null)
		{
			return;
		}
		if (source == _activeAudio)
		{
			framesOfSongPlayed = 0;
		}
		bool num = source.clip != null;
		string text = ((source.clip == null) ? string.Empty : source.clip.name);
		source.Stop();
		AudioUtil.UnloadNonPreloadedAudioData(source.clip, GameObj);
		AudioResourceOptimizer.UnloadPlaylistSongIfUnused(ControllerName, source.clip);
		source.clip = null;
		RemoveScheduledClip();
		if (num)
		{
			if (!string.IsNullOrEmpty(text) && songEndedEventExpanded && songEndedCustomEvent != string.Empty && songEndedCustomEvent != "[None]")
			{
				MasterAudio.FireCustomEvent(songEndedCustomEvent, Trans, logDupe: false);
			}
			if (this.SongEnded != null && !string.IsNullOrEmpty(text))
			{
				this.SongEnded(text);
			}
		}
	}

	private void SetDuckProperties()
	{
		_originalMusicVolume = ((_activeAudio == null) ? 1f : _activeAudio.volume);
		if (_currentSong != null)
		{
			_originalMusicVolume = _currentSong.volume * PlaylistVolume;
		}
		float floatVolumeFromDb = AudioUtil.GetFloatVolumeFromDb(AudioUtil.GetDbFromFloatVolume(_originalMusicVolume) - _currentDuckVolCut);
		_duckRange = _originalMusicVolume - floatVolumeFromDb;
		_initialDuckVolume = floatVolumeFromDb;
		_duckingMode = AudioDuckingMode.NotDucking;
	}

	private void AudioDucking()
	{
		switch (_duckingMode)
		{
		case AudioDuckingMode.SetToDuck:
			_activeAudio.volume = _initialDuckVolume;
			_duckingMode = AudioDuckingMode.Ducked;
			break;
		case AudioDuckingMode.Ducked:
			if (Time.realtimeSinceStartup >= _timeToFinishUnducking)
			{
				_activeAudio.volume = _originalMusicVolume;
				_duckingMode = AudioDuckingMode.NotDucking;
			}
			else if (Time.realtimeSinceStartup >= _timeToStartUnducking)
			{
				_activeAudio.volume = _initialDuckVolume + (Time.realtimeSinceStartup - _timeToStartUnducking) / (_timeToFinishUnducking - _timeToStartUnducking) * _duckRange;
			}
			break;
		case AudioDuckingMode.NotDucking:
			break;
		}
	}

	private bool SongShouldLoop(MusicSetting setting)
	{
		if (_queuedSongs.Count > 0)
		{
			return false;
		}
		if (SongIsNonAdvancible)
		{
			return true;
		}
		return setting.isLoop;
	}

	public void RouteToMixerChannel(AudioMixerGroup group)
	{
		_activeAudio.outputAudioMixerGroup = group;
		_transitioningAudio.outputAudioMixerGroup = group;
	}
}
