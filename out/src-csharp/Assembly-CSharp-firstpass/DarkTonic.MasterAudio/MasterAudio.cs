using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DarkTonic.MasterAudio;

[AudioScriptOrder(-50)]
public class MasterAudio : MonoBehaviour
{
	public enum VariationFollowerType
	{
		LateUpdate,
		FixedUpdate
	}

	public enum LinkedGroupSelectionType
	{
		All,
		OneAtRandom
	}

	public enum OcclusionSelectionType
	{
		AllGroups,
		TurnOnPerBusOrGroup
	}

	public enum RaycastMode
	{
		Physics3D,
		Physics2D
	}

	public enum AllMusicSpatialBlendType
	{
		ForceAllTo2D,
		ForceAllTo3D,
		ForceAllToCustom,
		AllowDifferentPerController
	}

	public enum AllMixerSpatialBlendType
	{
		ForceAllTo2D,
		ForceAllTo3D,
		ForceAllToCustom,
		AllowDifferentPerGroup
	}

	public enum ItemSpatialBlendType
	{
		ForceTo2D,
		ForceTo3D,
		ForceToCustom,
		UseCurveFromAudioSource
	}

	public enum InternetFileLoadStatus
	{
		Loading,
		Loaded,
		Failed
	}

	public enum MixerWidthMode
	{
		Narrow,
		Normal,
		Wide
	}

	public enum CustomEventReceiveMode
	{
		Always,
		WhenDistanceLessThan,
		WhenDistanceMoreThan,
		Never,
		OnSameGameObject,
		OnChildGameObject,
		OnParentGameObject,
		OnSameOrChildGameObject,
		OnSameOrParentGameObject
	}

	public enum EventReceiveFilter
	{
		All,
		Closest,
		Random
	}

	public enum AudioLocation
	{
		Clip,
		ResourceFile,
		FileOnInternet
	}

	public enum CustomSongStartTimeMode
	{
		Beginning,
		SpecificTime,
		RandomTime
	}

	public enum BusCommand
	{
		None,
		FadeToVolume,
		Mute,
		Pause,
		Solo,
		Unmute,
		Unpause,
		Unsolo,
		Stop,
		ChangePitch,
		ToggleMute,
		StopBusOfTransform,
		PauseBusOfTransform,
		UnpauseBusOfTransform,
		GlideByPitch
	}

	public enum DragGroupMode
	{
		OneGroupPerClip,
		OneGroupWithVariations
	}

	public enum EventSoundFunctionType
	{
		PlaySound,
		GroupControl,
		BusControl,
		PlaylistControl,
		CustomEventControl,
		GlobalControl,
		UnityMixerControl,
		PersistentSettingsControl
	}

	public enum LanguageMode
	{
		UseDeviceSetting,
		SpecificLanguage,
		DynamicallySet
	}

	public enum UnityMixerCommand
	{
		None,
		TransitionToSnapshot,
		TransitionToSnapshotBlend
	}

	public enum PlaylistCommand
	{
		None,
		ChangePlaylist,
		FadeToVolume,
		PlaySong,
		PlayRandomSong,
		PlayNextSong,
		Pause,
		Resume,
		Stop,
		Mute,
		Unmute,
		ToggleMute,
		Restart,
		Start,
		StopLoopingCurrentSong,
		StopPlaylistAfterCurrentSong,
		AddSongToQueue
	}

	public enum CustomEventCommand
	{
		None,
		FireEvent
	}

	public enum GlobalCommand
	{
		None,
		PauseMixer,
		UnpauseMixer,
		StopMixer,
		StopEverything,
		PauseEverything,
		UnpauseEverything,
		MuteEverything,
		UnmuteEverything,
		SetMasterMixerVolume,
		SetMasterPlaylistVolume
	}

	public enum SoundGroupCommand
	{
		None,
		FadeToVolume,
		FadeOutAllOfSound,
		Mute,
		Pause,
		Solo,
		StopAllOfSound,
		Unmute,
		Unpause,
		Unsolo,
		StopAllSoundsOfTransform,
		PauseAllSoundsOfTransform,
		UnpauseAllSoundsOfTransform,
		StopSoundGroupOfTransform,
		PauseSoundGroupOfTransform,
		UnpauseSoundGroupOfTransform,
		FadeOutSoundGroupOfTransform,
		RefillSoundGroupPool,
		RouteToBus,
		GlideByPitch,
		ToggleSoundGroup,
		ToggleSoundGroupOfTransform,
		FadeOutAllSoundsOfTransform
	}

	public enum PersistentSettingsCommand
	{
		None,
		SetBusVolume,
		SetGroupVolume,
		SetMixerVolume,
		SetMusicVolume,
		MixerMuteToggle,
		MusicMuteToggle
	}

	public enum SongFadeInPosition
	{
		NewClipFromBeginning = 1,
		NewClipFromLastKnownPosition = 3,
		SynchronizeClips = 5
	}

	public enum SoundSpawnLocationMode
	{
		MasterAudioLocation,
		CallerLocation,
		AttachToCaller
	}

	public enum VariationCommand
	{
		None,
		Stop,
		Pause,
		Unpause
	}

	public struct CustomEventCandidate
	{
		public float DistanceAway;

		public ICustomEventReceiver Receiver;

		public Transform Trans;

		public int RandomId;

		public CustomEventCandidate(float distance, ICustomEventReceiver rec, Transform trans, int randomId)
		{
			DistanceAway = distance;
			Receiver = rec;
			Trans = trans;
			RandomId = randomId;
		}
	}

	[Serializable]
	public class AudioGroupInfo
	{
		public List<AudioInfo> Sources;

		public int LastFramePlayed;

		public float LastTimePlayed;

		public MasterAudioGroup Group;

		public bool PlayedForWarming;

		public AudioGroupInfo(List<AudioInfo> sources, MasterAudioGroup groupScript)
		{
			Sources = sources;
			LastFramePlayed = -50;
			LastTimePlayed = -50f;
			Group = groupScript;
			PlayedForWarming = false;
		}
	}

	[Serializable]
	public class AudioInfo
	{
		public AudioSource Source;

		public float OriginalVolume;

		public float LastPercentageVolume;

		public float LastRandomVolume;

		public SoundGroupVariation Variation;

		public AudioInfo(SoundGroupVariation variation, AudioSource source, float origVol)
		{
			Variation = variation;
			Source = source;
			OriginalVolume = origVol;
			LastPercentageVolume = 1f;
			LastRandomVolume = 0f;
		}
	}

	[Serializable]
	public class Playlist
	{
		public enum CrossfadeTimeMode
		{
			UseMasterSetting,
			Override
		}

		public bool isExpanded = true;

		public string playlistName = "new playlist";

		public SongFadeInPosition songTransitionType = SongFadeInPosition.NewClipFromBeginning;

		public List<MusicSetting> MusicSettings;

		public AudioLocation bulkLocationMode;

		public CrossfadeTimeMode crossfadeMode;

		public float crossFadeTime = 1f;

		public bool fadeInFirstSong;

		public bool fadeOutLastSong;

		public bool resourceClipsAllLoadAsync = true;

		public bool isTemporary;

		public Playlist()
		{
			MusicSettings = new List<MusicSetting>();
		}
	}

	[Serializable]
	public class SoundGroupRefillInfo
	{
		public float LastTimePlayed;

		public float InactivePeriodSeconds;

		public SoundGroupRefillInfo(float lastTimePlayed, float inactivePeriodSeconds)
		{
			LastTimePlayed = lastTimePlayed;
			InactivePeriodSeconds = inactivePeriodSeconds;
		}
	}

	public const string MasterAudioDefaultFolder = "Assets/Plugins/DarkTonic/MasterAudio";

	public const string PreviewText = "Random delay, custom fading & start/end position settings are ignored by preview in edit mode.";

	public const string LoopDisabledLoopedChain = "Loop Clip is always OFF for Looped Chain Groups";

	public const string LoopDisabledCustomStartEnd = "Loop Clip is always OFF when using Custom Start/End Position";

	public const string DragAudioTip = "Drag Audio clips or a folder containing some here";

	public const string NoCategory = "[Uncategorized]";

	public const float SemiTonePitchFactor = 1.05946f;

	public const float SpatialBlend_2DValue = 0f;

	public const float SpatialBlend_3DValue = 1f;

	public const float MaxCrossFadeTimeSeconds = 120f;

	public const float DefaultDuckVolCut = -6f;

	public const string StoredLanguageNameKey = "~MA_Language_Key~";

	public static readonly YieldInstruction EndOfFrameDelay = new WaitForEndOfFrame();

	public static readonly List<string> ExemptChildNames = new List<string> { "_Followers" };

	public static Action NumberOfAudioSourcesChanged;

	public const string GizmoFileName = "MasterAudio/MasterAudio Icon.png";

	public const int HardCodedBusOptions = 2;

	public const string AllBusesName = "[All]";

	public const string NoGroupName = "[None]";

	public const string DynamicGroupName = "[Type In]";

	public const string NoPlaylistName = "[No Playlist]";

	public const string NoVoiceLimitName = "[NO LMT]";

	public const string OnlyPlaylistControllerName = "~only~";

	public const float InnerLoopCheckInterval = 0.1f;

	private const int MaxComponents = 20;

	public AudioLocation bulkLocationMode;

	public string groupTemplateName = "Default Single";

	public string audioSourceTemplateName = "Max Distance 500";

	public bool showGroupCreation = true;

	public bool useGroupTemplates;

	public DragGroupMode curDragGroupMode;

	public List<GameObject> groupTemplates = new List<GameObject>(10);

	public List<GameObject> audioSourceTemplates = new List<GameObject>(10);

	public bool mixerMuted;

	public bool playlistsMuted;

	public LanguageMode langMode;

	public SystemLanguage testLanguage = SystemLanguage.English;

	public SystemLanguage defaultLanguage = SystemLanguage.English;

	public List<SystemLanguage> supportedLanguages = new List<SystemLanguage> { SystemLanguage.English };

	public string busFilter = string.Empty;

	public bool useTextGroupFilter;

	public string textGroupFilter = string.Empty;

	public bool resourceClipsPauseDoNotUnload;

	public bool resourceClipsAllLoadAsync = true;

	public Transform playlistControllerPrefab;

	public bool persistBetweenScenes;

	public bool areGroupsExpanded = true;

	public Transform soundGroupTemplate;

	public Transform soundGroupVariationTemplate;

	public List<GroupBus> groupBuses = new List<GroupBus>();

	public bool groupByBus = true;

	public bool showGizmos = true;

	public bool showAdvancedSettings = true;

	public bool showLocalization = true;

	public bool playListExpanded = true;

	public bool playlistsExpanded = true;

	public AllMusicSpatialBlendType musicSpatialBlendType;

	public float musicSpatialBlend;

	public AllMixerSpatialBlendType mixerSpatialBlendType = AllMixerSpatialBlendType.ForceAllTo3D;

	public float mixerSpatialBlend = 1f;

	public ItemSpatialBlendType newGroupSpatialType = ItemSpatialBlendType.ForceTo3D;

	public float newGroupSpatialBlend = 1f;

	public List<Playlist> musicPlaylists = new List<Playlist>
	{
		new Playlist()
	};

	public float _masterAudioVolume = 1f;

	public bool useSpatializer;

	public bool ignoreTimeScale;

	public bool useGaplessPlaylists;

	public bool saveRuntimeChanges;

	public bool prioritizeOnDistance;

	public int rePrioritizeEverySecIndex = 1;

	public bool useOcclusion;

	public float occlusionMaxCutoffFreq;

	public float occlusionMinCutoffFreq = 22000f;

	public float occlusionFreqChangeSeconds;

	public OcclusionSelectionType occlusionSelectType;

	public int occlusionMaxRayCastsPerFrame = 4;

	public float occlusionRayCastOffset;

	public bool occlusionUseLayerMask;

	public LayerMask occlusionLayerMask;

	public bool occlusionShowRaycasts = true;

	public bool occlusionShowCategories;

	public RaycastMode occlusionRaycastMode;

	public bool occlusionIncludeStartRaycast2DCollider = true;

	public bool occlusionRaycastsHitTriggers = true;

	public bool ambientAdvancedExpanded;

	public int ambientMaxRecalcsPerFrame = 4;

	public bool visualAdvancedExpanded = true;

	public bool logAdvancedExpanded = true;

	public bool listenerAdvancedExpanded;

	public bool listenerFollowerHasRigidBody = true;

	public VariationFollowerType variationFollowerType;

	public bool showFadingSettings;

	public bool stopZeroVolumeGroups;

	public bool stopZeroVolumeBuses;

	public bool stopZeroVolumePlaylists;

	public float stopOldestBusFadeTime = 0.3f;

	public bool resourceAdvancedExpanded = true;

	public bool useClipAgePriority;

	public bool logOutOfVoices = true;

	public bool LogSounds;

	public bool logCustomEvents;

	public bool disableLogging;

	public bool showMusicDucking;

	public bool enableMusicDucking = true;

	public List<DuckGroupInfo> musicDuckingSounds = new List<DuckGroupInfo>();

	public float defaultRiseVolStart = 0.5f;

	public float defaultUnduckTime = 1f;

	public float defaultDuckedVolumeCut = -6f;

	public float crossFadeTime = 1f;

	public float _masterPlaylistVolume = 1f;

	public bool showGroupSelect;

	public bool hideGroupsWithNoActiveVars;

	public string newEventName = "my event";

	public bool showCustomEvents = true;

	public string newCustomEventCategoryName = "New Category";

	public string addToCustomEventCategoryName = "New Category";

	public List<CustomEvent> customEvents = new List<CustomEvent>();

	public List<CustomEventCategory> customEventCategories = new List<CustomEventCategory>
	{
		new CustomEventCategory()
	};

	public Dictionary<string, DuckGroupInfo> duckingBySoundType = new Dictionary<string, DuckGroupInfo>(StringComparer.OrdinalIgnoreCase);

	public int frames;

	public bool showUnityMixerGroupAssignment = true;

	public static readonly PlaySoundResult AndForgetSuccessResult = new PlaySoundResult
	{
		SoundPlayed = true
	};

	private readonly Dictionary<string, AudioGroupInfo> AudioSourcesBySoundType = new Dictionary<string, AudioGroupInfo>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, List<int>> _randomizer = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, List<int>> _randomizerOrigin = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, List<int>> _randomizerLeftovers = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, List<int>> _clipsPlayedBySoundTypeOldestFirst = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

	private readonly List<SoundGroupVariationUpdater> ActiveVariationUpdaters = new List<SoundGroupVariationUpdater>(32);

	private readonly List<SoundGroupVariationUpdater> ActiveUpdatersToRemove = new List<SoundGroupVariationUpdater>();

	private readonly List<CustomEventCandidate> ValidReceivers = new List<CustomEventCandidate>(10);

	private readonly List<MasterAudioGroup> SoloedGroups = new List<MasterAudioGroup>();

	private readonly Queue<CustomEventToFireInfo> CustomEventsToFire = new Queue<CustomEventToFireInfo>(32);

	private readonly Queue<TransformFollower> TransFollowerColliderPositionRecalcs = new Queue<TransformFollower>(32);

	private readonly List<TransformFollower> ProcessedColliderPositionRecalcs = new List<TransformFollower>(32);

	private readonly List<BusFadeInfo> BusFades = new List<BusFadeInfo>();

	private readonly List<GroupFadeInfo> GroupFades = new List<GroupFadeInfo>();

	private readonly List<GroupPitchGlideInfo> GroupPitchGlides = new List<GroupPitchGlideInfo>();

	private readonly List<BusPitchGlideInfo> BusPitchGlides = new List<BusPitchGlideInfo>();

	private readonly List<OcclusionFreqChangeInfo> VariationOcclusionFreqChanges = new List<OcclusionFreqChangeInfo>();

	private readonly List<AudioSource> AllAudioSources = new List<AudioSource>(100);

	private readonly Dictionary<string, Dictionary<ICustomEventReceiver, Transform>> ReceiversByEventName = new Dictionary<string, Dictionary<ICustomEventReceiver, Transform>>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, PlaylistController> PlaylistControllersByName = new Dictionary<string, PlaylistController>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, SoundGroupRefillInfo> LastTimeSoundGroupPlayed = new Dictionary<string, SoundGroupRefillInfo>(StringComparer.OrdinalIgnoreCase);

	private readonly List<GameObject> OcclusionSourcesInRange = new List<GameObject>(32);

	private readonly List<GameObject> OcclusionSourcesOutOfRange = new List<GameObject>(32);

	private readonly List<GameObject> OcclusionSourcesBlocked = new List<GameObject>(32);

	private readonly Queue<SoundGroupVariationUpdater> QueuedOcclusionRays = new Queue<SoundGroupVariationUpdater>(32);

	private readonly List<SoundGroupVariation> VariationsStartedDuringMultiStop = new List<SoundGroupVariation>(16);

	private bool _isStoppingMultiple;

	private float _repriTime = -1f;

	private List<string> _groupsToRemove;

	private bool _mustRescanGroups;

	private Transform _trans;

	private bool _soundsLoaded;

	private bool _warming;

	private static MasterAudio _instance;

	private static string _prospectiveMAFolder = string.Empty;

	private static Transform _listenerTrans;

	public static readonly List<SoundGroupCommand> GroupCommandsWithNoGroupSelector = new List<SoundGroupCommand>
	{
		SoundGroupCommand.None,
		SoundGroupCommand.PauseAllSoundsOfTransform,
		SoundGroupCommand.StopAllSoundsOfTransform,
		SoundGroupCommand.UnpauseAllSoundsOfTransform,
		SoundGroupCommand.FadeOutAllSoundsOfTransform
	};

	public static readonly List<SoundGroupCommand> GroupCommandsWithNoAllGroupSelector = new List<SoundGroupCommand>
	{
		SoundGroupCommand.None,
		SoundGroupCommand.FadeOutSoundGroupOfTransform,
		SoundGroupCommand.PauseSoundGroupOfTransform,
		SoundGroupCommand.UnpauseSoundGroupOfTransform,
		SoundGroupCommand.StopSoundGroupOfTransform,
		SoundGroupCommand.ToggleSoundGroupOfTransform,
		SoundGroupCommand.ToggleSoundGroup,
		SoundGroupCommand.FadeOutAllSoundsOfTransform
	};

	public static float PlaylistMasterVolume
	{
		get
		{
			return Instance._masterPlaylistVolume;
		}
		set
		{
			Instance._masterPlaylistVolume = value;
			List<PlaylistController> instances = PlaylistController.Instances;
			for (int i = 0; i < instances.Count; i++)
			{
				instances[i].UpdateMasterVolume();
			}
		}
	}

	public static bool LogSoundsEnabled
	{
		get
		{
			return Instance.LogSounds;
		}
		set
		{
			Instance.LogSounds = value;
		}
	}

	public static bool LogOutOfVoices
	{
		get
		{
			return Instance.logOutOfVoices;
		}
		set
		{
			Instance.logOutOfVoices = value;
		}
	}

	public static List<AudioSource> MasterAudioSources => Instance.AllAudioSources;

	public static Transform ListenerTrans
	{
		get
		{
			if (_listenerTrans == null || !DTMonoHelper.IsActive(_listenerTrans.gameObject))
			{
				_listenerTrans = null;
				AudioListener[] array = UnityEngine.Object.FindObjectsOfType<AudioListener>();
				foreach (AudioListener audioListener in array)
				{
					if (DTMonoHelper.IsActive(audioListener.gameObject))
					{
						_listenerTrans = audioListener.transform;
					}
				}
			}
			return _listenerTrans;
		}
	}

	public static PlaylistController OnlyPlaylistController
	{
		get
		{
			List<PlaylistController> instances = PlaylistController.Instances;
			if (instances.Count != 0)
			{
				return instances[0];
			}
			Debug.LogError("There are no Playlist Controller in this Scene.");
			return null;
		}
	}

	public static bool IsWarming
	{
		get
		{
			if (SafeInstance != null)
			{
				return Instance._warming;
			}
			return false;
		}
	}

	public static bool MixerMuted
	{
		get
		{
			return Instance.mixerMuted;
		}
		set
		{
			Instance.mixerMuted = value;
			if (value)
			{
				foreach (string key in Instance.AudioSourcesBySoundType.Keys)
				{
					MuteGroup(Instance.AudioSourcesBySoundType[key].Group.GameObjectName, shouldCheckMuteStatus: false);
				}
			}
			else
			{
				foreach (string key2 in Instance.AudioSourcesBySoundType.Keys)
				{
					UnmuteGroup(Instance.AudioSourcesBySoundType[key2].Group.GameObjectName, shouldCheckMuteStatus: false);
				}
			}
			if (Application.isPlaying)
			{
				SilenceOrUnsilenceGroupsFromSoloChange();
			}
		}
	}

	public static bool PlaylistsMuted
	{
		get
		{
			return Instance.playlistsMuted;
		}
		set
		{
			Instance.playlistsMuted = value;
			List<PlaylistController> instances = PlaylistController.Instances;
			for (int i = 0; i < instances.Count; i++)
			{
				if (value)
				{
					instances[i].MutePlaylist();
				}
				else
				{
					instances[i].UnmutePlaylist();
				}
			}
		}
	}

	public bool EnableMusicDucking
	{
		get
		{
			return enableMusicDucking;
		}
		set
		{
			enableMusicDucking = value;
		}
	}

	public float MasterCrossFadeTime => crossFadeTime;

	public static List<Playlist> MusicPlaylists => Instance.musicPlaylists;

	public static List<GroupBus> GroupBuses => Instance.groupBuses;

	public static List<string> RuntimeSoundGroupNames
	{
		get
		{
			if (!Application.isPlaying)
			{
				return new List<string>();
			}
			return new List<string>(Instance.AudioSourcesBySoundType.Keys);
		}
	}

	public static List<string> RuntimeBusNames
	{
		get
		{
			if (!Application.isPlaying)
			{
				return new List<string>();
			}
			List<string> list = new List<string>();
			for (int i = 0; i < Instance.groupBuses.Count; i++)
			{
				list.Add(Instance.groupBuses[i].busName);
			}
			return list;
		}
	}

	public static MasterAudio SafeInstance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			_instance = (MasterAudio)UnityEngine.Object.FindObjectOfType(typeof(MasterAudio));
			return _instance;
		}
	}

	public static MasterAudio Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			_instance = (MasterAudio)UnityEngine.Object.FindObjectOfType(typeof(MasterAudio));
			if (_instance == null && Application.isPlaying)
			{
				Debug.LogError("There is no Master Audio prefab in this Scene. Subsequent method calls will fail.");
			}
			return _instance;
		}
		set
		{
			_instance = null;
		}
	}

	public static bool SoundsReady
	{
		get
		{
			if (Instance != null)
			{
				return Instance._soundsLoaded;
			}
			return false;
		}
	}

	public static bool AppIsShuttingDown { get; set; }

	public List<string> GroupNames
	{
		get
		{
			List<string> soundGroupHardCodedNames = SoundGroupHardCodedNames;
			List<string> list = new List<string>(Trans.childCount);
			for (int i = 0; i < Trans.childCount; i++)
			{
				string item = Trans.GetChild(i).name;
				if (!ArrayListUtil.IsExcludedChildName(item))
				{
					list.Add(item);
				}
			}
			DynamicSoundGroupCreator[] array = UnityEngine.Object.FindObjectsOfType(typeof(DynamicSoundGroupCreator)) as DynamicSoundGroupCreator[];
			for (int j = 0; j < array.Length; j++)
			{
				Transform transform = array[j].transform;
				for (int k = 0; k < transform.childCount; k++)
				{
					DynamicSoundGroup component = transform.GetChild(k).GetComponent<DynamicSoundGroup>();
					if (!(component == null) && !list.Contains(component.name))
					{
						list.Add(component.name);
					}
				}
			}
			list.Sort();
			soundGroupHardCodedNames.AddRange(list);
			return soundGroupHardCodedNames;
		}
	}

	public static List<string> SoundGroupHardCodedNames => new List<string> { "[Type In]", "[None]" };

	public List<string> BusNames
	{
		get
		{
			List<string> list = new List<string> { "[Type In]", "[None]" };
			for (int i = 0; i < groupBuses.Count; i++)
			{
				list.Add(groupBuses[i].busName);
			}
			return list;
		}
	}

	public List<string> PlaylistNames
	{
		get
		{
			List<string> list = new List<string> { "[Type In]", "[No Playlist]" };
			for (int i = 0; i < musicPlaylists.Count; i++)
			{
				list.Add(musicPlaylists[i].playlistName);
			}
			return list;
		}
	}

	public List<string> PlaylistNamesOnly
	{
		get
		{
			List<string> list = new List<string>(musicPlaylists.Count);
			for (int i = 0; i < musicPlaylists.Count; i++)
			{
				list.Add(musicPlaylists[i].playlistName);
			}
			return list;
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
			_trans = GetComponent<Transform>();
			return _trans;
		}
	}

	public bool ShouldShowUnityAudioMixerGroupAssignments => showUnityMixerGroupAssignment;

	public List<string> CustomEventNames
	{
		get
		{
			List<string> customEventHardCodedNames = CustomEventHardCodedNames;
			List<CustomEvent> list = Instance.customEvents;
			for (int i = 0; i < list.Count; i++)
			{
				customEventHardCodedNames.Add(list[i].EventName);
			}
			return customEventHardCodedNames;
		}
	}

	public List<string> CustomEventNamesOnly
	{
		get
		{
			List<string> list = new List<string>(customEvents.Count);
			List<CustomEvent> list2 = Instance.customEvents;
			for (int i = 0; i < list2.Count; i++)
			{
				list.Add(list2[i].EventName);
			}
			return list;
		}
	}

	public static List<string> CustomEventHardCodedNames => new List<string> { "[Type In]", "[None]" };

	public static float MasterVolumeLevel
	{
		get
		{
			return Instance._masterAudioVolume;
		}
		set
		{
			Instance._masterAudioVolume = value;
			if (Application.isPlaying)
			{
				Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
				while (enumerator.MoveNext())
				{
					MasterAudioGroup group = enumerator.Current.Value.Group;
					SetGroupVolume(group.GameObjectName, group.groupMasterVolume);
				}
			}
		}
	}

	private static bool SceneHasMasterAudio => Instance != null;

	public static bool IgnoreTimeScale => Instance.ignoreTimeScale;

	public static SystemLanguage DynamicLanguage
	{
		get
		{
			if (!PlayerPrefs.HasKey("~MA_Language_Key~") || string.IsNullOrEmpty(PlayerPrefs.GetString("~MA_Language_Key~")))
			{
				PlayerPrefs.SetString("~MA_Language_Key~", SystemLanguage.Unknown.ToString());
			}
			return (SystemLanguage)Enum.Parse(typeof(SystemLanguage), PlayerPrefs.GetString("~MA_Language_Key~"));
		}
		set
		{
			PlayerPrefs.SetString("~MA_Language_Key~", value.ToString());
			AudioResourceOptimizer.ClearSupportLanguageFolder();
		}
	}

	public static float ReprioritizeTime
	{
		get
		{
			if (Instance._repriTime < 0f)
			{
				Instance._repriTime = (float)(Instance.rePrioritizeEverySecIndex + 1) * 0.1f;
			}
			return Instance._repriTime;
		}
	}

	public static bool ShouldRescanGroups
	{
		get
		{
			if (SafeInstance == null)
			{
				return false;
			}
			return Instance._mustRescanGroups;
		}
	}

	public static string ProspectiveMAPath
	{
		get
		{
			return _prospectiveMAFolder;
		}
		set
		{
			_prospectiveMAFolder = value;
		}
	}

	private void Awake()
	{
		if (UnityEngine.Object.FindObjectsOfType(typeof(MasterAudio)).Length > 1)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			Debug.Log("More than one Master Audio prefab exists in this Scene. Destroying the newer one called '" + base.name + "'. You may wish to set up a Bootstrapper Scene so this does not occur.");
			return;
		}
		base.useGUILayout = false;
		_soundsLoaded = false;
		_mustRescanGroups = false;
		Transform listenerTrans = ListenerTrans;
		if (listenerTrans != null)
		{
			AudioSource component = listenerTrans.GetComponent<AudioSource>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
		AmbientUtil.InitFollowerHolder();
		AudioSourcesBySoundType.Clear();
		PlaylistControllersByName.Clear();
		LastTimeSoundGroupPlayed.Clear();
		AllAudioSources.Clear();
		OcclusionSourcesInRange.Clear();
		OcclusionSourcesOutOfRange.Clear();
		OcclusionSourcesBlocked.Clear();
		QueuedOcclusionRays.Clear();
		TransFollowerColliderPositionRecalcs.Clear();
		ProcessedColliderPositionRecalcs.Clear();
		ActiveVariationUpdaters.Clear();
		ActiveUpdatersToRemove.Clear();
		List<string> list = new List<string>();
		AudioResourceOptimizer.ClearAudioClips();
		PlaylistController.Instances = null;
		List<PlaylistController> instances = PlaylistController.Instances;
		for (int i = 0; i < instances.Count; i++)
		{
			PlaylistController playlistController = instances[i];
			if (list.Contains(playlistController.name))
			{
				Debug.LogError("You have more than 1 Playlist Controller with the name '" + playlistController.name + "'. You must name them all uniquely or the same-named ones will be deleted once they awake.");
				continue;
			}
			list.Add(playlistController.name);
			PlaylistControllersByName.Add(playlistController.name, playlistController);
			if (persistBetweenScenes)
			{
				UnityEngine.Object.DontDestroyOnLoad(playlistController);
			}
		}
		if (persistBetweenScenes)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		List<int> list2 = new List<int>();
		_randomizer = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
		_randomizerOrigin = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
		_randomizerLeftovers = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
		_clipsPlayedBySoundTypeOldestFirst = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
		string text = string.Empty;
		List<SoundGroupVariation> list3 = new List<SoundGroupVariation>();
		_groupsToRemove = new List<string>(Trans.childCount);
		List<string> list4 = new List<string>();
		for (int j = 0; j < Trans.childCount; j++)
		{
			Transform child = Trans.GetChild(j);
			List<AudioInfo> list5 = new List<AudioInfo>();
			MasterAudioGroup component2 = child.GetComponent<MasterAudioGroup>();
			if (component2 == null)
			{
				if (!ArrayListUtil.IsExcludedChildName(child.name))
				{
					Debug.LogError("MasterAudio could not find 'MasterAudioGroup' script for group '" + child.name + "'. Skipping this group.");
				}
				continue;
			}
			string text2 = child.name;
			if (string.IsNullOrEmpty(text))
			{
				text = text2;
			}
			List<Transform> list6 = new List<Transform>();
			List<int> list7 = new List<int>();
			for (int k = 0; k < child.childCount; k++)
			{
				Transform child2 = child.GetChild(k);
				SoundGroupVariation component3 = child2.GetComponent<SoundGroupVariation>();
				AudioSource component4 = child2.GetComponent<AudioSource>();
				int weight = component3.weight;
				for (int l = 0; l < weight; l++)
				{
					if (l > 0)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(child2.gameObject, child.transform.position, Quaternion.identity);
						gameObject.transform.name = child2.gameObject.name;
						SoundGroupVariation component5 = gameObject.GetComponent<SoundGroupVariation>();
						component5.weight = 1;
						list6.Add(gameObject.transform);
						component4 = gameObject.GetComponent<AudioSource>();
						list5.Add(new AudioInfo(component5, component4, component4.volume));
						list3.Add(component5);
						switch (component5.audLocation)
						{
						case AudioLocation.ResourceFile:
							AudioResourceOptimizer.AddTargetForClip(component5.resourceFileName, component4);
							break;
						case AudioLocation.FileOnInternet:
							AudioResourceOptimizer.AddTargetForClip(component5.internetFileUrl, component4);
							break;
						}
					}
					else
					{
						list5.Add(new AudioInfo(component3, component4, component4.volume));
						list3.Add(component3);
						switch (component3.audLocation)
						{
						case AudioLocation.ResourceFile:
							AudioResourceOptimizer.AddTargetForClip(AudioResourceOptimizer.GetLocalizedFileName(component3.useLocalization, component3.resourceFileName), component4);
							break;
						case AudioLocation.FileOnInternet:
							AudioResourceOptimizer.AddTargetForClip(component3.internetFileUrl, component4);
							break;
						}
					}
				}
			}
			for (int m = 0; m < list6.Count; m++)
			{
				list6[m].parent = child;
			}
			AudioGroupInfo audioGroupInfo = new AudioGroupInfo(list5, component2);
			if (component2.isSoloed)
			{
				SoloedGroups.Add(component2);
			}
			if (component2.isMuted)
			{
				if (list4.Contains(component2.name))
				{
					continue;
				}
				list4.Add(component2.name);
			}
			if (AudioSourcesBySoundType.ContainsKey(text2))
			{
				Debug.LogError("You have more than one SoundGroup named '" + text2 + "'. Ignoring the 2nd one. Please rename it.");
				continue;
			}
			audioGroupInfo.Group.OriginalVolume = audioGroupInfo.Group.groupMasterVolume;
			float? groupVolume = PersistentAudioSettings.GetGroupVolume(text2);
			if (groupVolume.HasValue)
			{
				audioGroupInfo.Group.groupMasterVolume = groupVolume.Value;
			}
			AddRuntimeGroupInfo(text2, audioGroupInfo);
			for (int n = 0; n < list5.Count; n++)
			{
				list2.Add(n);
			}
			if (audioGroupInfo.Group.curVariationSequence == MasterAudioGroup.VariationSequence.Randomized)
			{
				ArrayListUtil.SortIntArray(ref list2);
			}
			_randomizer.Add(text2, list2);
			list7.Clear();
			list7.AddRange(list2);
			_randomizerOrigin.Add(text2, list7);
			_randomizerLeftovers.Add(text2, new List<int>(list2.Count));
			_randomizerLeftovers[text2].AddRange(list2);
			_clipsPlayedBySoundTypeOldestFirst.Add(text2, new List<int>());
			list2 = new List<int>();
		}
		GroupFades.Clear();
		BusFades.Clear();
		GroupPitchGlides.Clear();
		BusPitchGlides.Clear();
		VariationOcclusionFreqChanges.Clear();
		for (int num = 0; num < groupBuses.Count; num++)
		{
			GroupBus groupBus = groupBuses[num];
			groupBus.OriginalVolume = groupBus.volume;
			string busName = groupBus.busName;
			float? busVolume = PersistentAudioSettings.GetBusVolume(busName);
			if (busVolume.HasValue)
			{
				SetBusVolumeByName(busName, busVolume.Value);
			}
		}
		duckingBySoundType.Clear();
		for (int num2 = 0; num2 < musicDuckingSounds.Count; num2++)
		{
			DuckGroupInfo duckGroupInfo = musicDuckingSounds[num2];
			if (duckingBySoundType.ContainsKey(duckGroupInfo.soundType))
			{
				Debug.LogWarning("You have more than one Duck Group set up with the Sound Group '" + duckGroupInfo.soundType + "'. Please delete the duplicates before running again.");
			}
			else
			{
				duckingBySoundType.Add(duckGroupInfo.soundType, duckGroupInfo);
			}
		}
		_soundsLoaded = true;
		_warming = true;
		if (!string.IsNullOrEmpty(text))
		{
			PlaySoundResult playSoundResult = PlaySound3DFollowTransform(text, Trans, 0f);
			if (playSoundResult != null && playSoundResult.SoundPlayed)
			{
				playSoundResult.ActingVariation.Stop();
			}
		}
		FireCustomEvent("FakeEvent", _trans);
		for (int num3 = 0; num3 < customEvents.Count; num3++)
		{
			customEvents[num3].frameLastFired = -1;
		}
		frames = 0;
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(EventSounds));
		if (array.Length != 0)
		{
			EventSounds obj = array[0] as EventSounds;
			obj.PlaySounds(obj.particleCollisionSound, EventSounds.EventType.UserDefinedEvent);
		}
		for (int num4 = 0; num4 < list4.Count; num4++)
		{
			MuteGroup(list4[num4], shouldCheckMuteStatus: false);
		}
		_warming = false;
		for (int num5 = 0; num5 < list3.Count; num5++)
		{
			list3[num5].DisableUpdater();
		}
		AmbientUtil.InitListenerFollower();
		PersistentAudioSettings.RestoreMasterSettings();
	}

	private void Start()
	{
		if (musicPlaylists.Count > 0 && musicPlaylists[0].MusicSettings != null && musicPlaylists[0].MusicSettings.Count > 0 && musicPlaylists[0].MusicSettings[0].clip != null && PlaylistControllersByName.Count == 0)
		{
			Debug.Log("No Playlist Controllers exist in the Scene. Music will not play.");
		}
	}

	private void OnDisable()
	{
		StopTrackingRuntimeAudioSources(GetComponentsInChildren<AudioSource>().ToList());
	}

	private void Update()
	{
		frames++;
		PerformOcclusionFrequencyChanges();
		PerformBusFades();
		PerformBusPitchGlides();
		PerformGroupFades();
		PerformGroupPitchGlides();
		RefillInactiveGroupPools();
		FireCustomEventsWaiting();
		RecalcClosestColliderPositions();
	}

	private void LateUpdate()
	{
		if (variationFollowerType == VariationFollowerType.LateUpdate)
		{
			UpdateActiveVariations();
		}
	}

	private void FixedUpdate()
	{
		if (variationFollowerType == VariationFollowerType.FixedUpdate)
		{
			UpdateActiveVariations();
		}
	}

	public static void RegisterUpdaterForUpdates(SoundGroupVariationUpdater updater)
	{
		if (!Instance.ActiveVariationUpdaters.Contains(updater))
		{
			Instance.ActiveVariationUpdaters.Add(updater);
		}
	}

	public static void UnregisterUpdaterForUpdates(SoundGroupVariationUpdater updater)
	{
		Instance.ActiveVariationUpdaters.Remove(updater);
	}

	private void UpdateActiveVariations()
	{
		ActiveUpdatersToRemove.Clear();
		for (int i = 0; i < ActiveVariationUpdaters.Count; i++)
		{
			SoundGroupVariationUpdater soundGroupVariationUpdater = ActiveVariationUpdaters[i];
			if (soundGroupVariationUpdater == null || !soundGroupVariationUpdater.enabled)
			{
				ActiveUpdatersToRemove.Add(soundGroupVariationUpdater);
			}
			else
			{
				soundGroupVariationUpdater.ManualUpdate();
			}
		}
		for (int j = 0; j < ActiveUpdatersToRemove.Count; j++)
		{
			ActiveVariationUpdaters.Remove(ActiveUpdatersToRemove[j]);
		}
	}

	private static void UpdateRefillTime(string sType, float inactivePeriodSeconds)
	{
		if (!Instance.LastTimeSoundGroupPlayed.ContainsKey(sType))
		{
			Instance.LastTimeSoundGroupPlayed.Add(sType, new SoundGroupRefillInfo(Time.realtimeSinceStartup, inactivePeriodSeconds));
		}
		else
		{
			Instance.LastTimeSoundGroupPlayed[sType].LastTimePlayed = AudioUtil.Time;
		}
	}

	private static void RecalcClosestColliderPositions()
	{
		if (!AmbientUtil.HasListenerFollower)
		{
			AmbientUtil.InitListenerFollower();
		}
		Instance.ProcessedColliderPositionRecalcs.Clear();
		int num = 0;
		while (num < Instance.TransFollowerColliderPositionRecalcs.Count && Instance.TransFollowerColliderPositionRecalcs.Count != 0)
		{
			TransformFollower transformFollower = Instance.TransFollowerColliderPositionRecalcs.Dequeue();
			if (!(transformFollower == null) && transformFollower.enabled)
			{
				bool num2 = transformFollower.RecalcClosestColliderPosition();
				Instance.ProcessedColliderPositionRecalcs.Add(transformFollower);
				if (num2)
				{
					num++;
				}
			}
		}
		for (int i = 0; i < Instance.ProcessedColliderPositionRecalcs.Count; i++)
		{
			Instance.TransFollowerColliderPositionRecalcs.Enqueue(Instance.ProcessedColliderPositionRecalcs[i]);
		}
	}

	private static void FireCustomEventsWaiting()
	{
		while (Instance.CustomEventsToFire.Count > 0)
		{
			CustomEventToFireInfo customEventToFireInfo = Instance.CustomEventsToFire.Dequeue();
			FireCustomEvent(customEventToFireInfo.eventName, customEventToFireInfo.eventOrigin);
		}
	}

	private static void RefillInactiveGroupPools()
	{
		Dictionary<string, SoundGroupRefillInfo>.Enumerator enumerator = Instance.LastTimeSoundGroupPlayed.GetEnumerator();
		if (Instance._groupsToRemove == null)
		{
			Instance._groupsToRemove = new List<string>();
		}
		Instance._groupsToRemove.Clear();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, SoundGroupRefillInfo> current = enumerator.Current;
			if (current.Value.LastTimePlayed + current.Value.InactivePeriodSeconds < AudioUtil.Time)
			{
				RefillSoundGroupPool(current.Key);
				Instance._groupsToRemove.Add(current.Key);
			}
		}
		for (int i = 0; i < Instance._groupsToRemove.Count; i++)
		{
			Instance.LastTimeSoundGroupPlayed.Remove(Instance._groupsToRemove[i]);
		}
	}

	private static void PerformOcclusionFrequencyChanges()
	{
		if (!AmbientUtil.HasListenerFollower)
		{
			AmbientUtil.InitListenerFollower();
		}
		for (int i = 0; i < Instance.VariationOcclusionFreqChanges.Count; i++)
		{
			OcclusionFreqChangeInfo occlusionFreqChangeInfo = Instance.VariationOcclusionFreqChanges[i];
			if (occlusionFreqChangeInfo.IsActive)
			{
				float val = 1f - (occlusionFreqChangeInfo.CompletionTime - AudioUtil.Time) / (occlusionFreqChangeInfo.CompletionTime - occlusionFreqChangeInfo.StartTime);
				val = Math.Min(val, 1f);
				val = Math.Max(val, 0f);
				float val2 = occlusionFreqChangeInfo.StartFrequency + (occlusionFreqChangeInfo.TargetFrequency - occlusionFreqChangeInfo.StartFrequency) * val;
				val2 = ((!(occlusionFreqChangeInfo.TargetFrequency > occlusionFreqChangeInfo.StartFrequency)) ? Math.Max(val2, occlusionFreqChangeInfo.TargetFrequency) : Math.Min(val2, occlusionFreqChangeInfo.TargetFrequency));
				occlusionFreqChangeInfo.ActingVariation.LowPassFilter.cutoffFrequency = val2;
				if (!(AudioUtil.Time < occlusionFreqChangeInfo.CompletionTime))
				{
					occlusionFreqChangeInfo.IsActive = false;
				}
			}
		}
		Instance.VariationOcclusionFreqChanges.RemoveAll((OcclusionFreqChangeInfo obj) => !obj.IsActive);
	}

	private void PerformBusFades()
	{
		for (int i = 0; i < BusFades.Count; i++)
		{
			BusFadeInfo busFadeInfo = BusFades[i];
			if (!busFadeInfo.IsActive)
			{
				continue;
			}
			GroupBus actingBus = busFadeInfo.ActingBus;
			if (actingBus == null)
			{
				busFadeInfo.IsActive = false;
				continue;
			}
			float val = 1f - (busFadeInfo.CompletionTime - AudioUtil.Time) / (busFadeInfo.CompletionTime - busFadeInfo.StartTime);
			val = Math.Min(val, 1f);
			val = Math.Max(val, 0f);
			float val2 = busFadeInfo.StartVolume + (busFadeInfo.TargetVolume - busFadeInfo.StartVolume) * val;
			SetBusVolumeByName(newVolume: (!(busFadeInfo.TargetVolume > busFadeInfo.StartVolume)) ? Math.Max(val2, busFadeInfo.TargetVolume) : Math.Min(val2, busFadeInfo.TargetVolume), busName: actingBus.busName);
			if (!(AudioUtil.Time < busFadeInfo.CompletionTime))
			{
				busFadeInfo.IsActive = false;
				if (stopZeroVolumeBuses && busFadeInfo.TargetVolume == 0f)
				{
					StopBus(busFadeInfo.NameOfBus);
				}
				else if (busFadeInfo.WillStopGroupAfterFade)
				{
					StopBus(busFadeInfo.NameOfBus);
				}
				if (busFadeInfo.WillResetVolumeAfterFade)
				{
					SetBusVolumeByName(actingBus.busName, busFadeInfo.StartVolume);
				}
				if (busFadeInfo.completionAction != null)
				{
					busFadeInfo.completionAction();
				}
			}
		}
		BusFades.RemoveAll((BusFadeInfo obj) => !obj.IsActive);
	}

	private void PerformGroupFades()
	{
		for (int i = 0; i < GroupFades.Count; i++)
		{
			GroupFadeInfo groupFadeInfo = GroupFades[i];
			if (!groupFadeInfo.IsActive)
			{
				continue;
			}
			MasterAudioGroup actingGroup = groupFadeInfo.ActingGroup;
			if (actingGroup == null)
			{
				groupFadeInfo.IsActive = false;
				continue;
			}
			float val = 1f - (groupFadeInfo.CompletionTime - AudioUtil.Time) / (groupFadeInfo.CompletionTime - groupFadeInfo.StartTime);
			val = Math.Min(val, 1f);
			val = Math.Max(val, 0f);
			float val2 = groupFadeInfo.StartVolume + (groupFadeInfo.TargetVolume - groupFadeInfo.StartVolume) * val;
			SetGroupVolume(volumeLevel: (!(groupFadeInfo.TargetVolume > groupFadeInfo.StartVolume)) ? Math.Max(val2, groupFadeInfo.TargetVolume) : Math.Min(val2, groupFadeInfo.TargetVolume), sType: actingGroup.GameObjectName);
			if (!(AudioUtil.Time < groupFadeInfo.CompletionTime))
			{
				groupFadeInfo.IsActive = false;
				if (groupFadeInfo.completionAction != null)
				{
					groupFadeInfo.completionAction();
				}
				if (stopZeroVolumeGroups && groupFadeInfo.TargetVolume == 0f)
				{
					StopAllOfSound(groupFadeInfo.NameOfGroup);
				}
				else if (groupFadeInfo.WillStopGroupAfterFade)
				{
					StopAllOfSound(groupFadeInfo.NameOfGroup);
				}
				if (groupFadeInfo.WillResetVolumeAfterFade)
				{
					SetGroupVolume(actingGroup.GameObjectName, groupFadeInfo.StartVolume);
				}
			}
		}
		GroupFades.RemoveAll((GroupFadeInfo obj) => !obj.IsActive);
	}

	private void PerformGroupPitchGlides()
	{
		for (int i = 0; i < GroupPitchGlides.Count; i++)
		{
			GroupPitchGlideInfo groupPitchGlideInfo = GroupPitchGlides[i];
			if (!groupPitchGlideInfo.IsActive)
			{
				continue;
			}
			if (groupPitchGlideInfo.ActingGroup == null)
			{
				groupPitchGlideInfo.IsActive = false;
			}
			else if (!(AudioUtil.Time < groupPitchGlideInfo.CompletionTime))
			{
				groupPitchGlideInfo.IsActive = false;
				if (groupPitchGlideInfo.completionAction != null)
				{
					groupPitchGlideInfo.completionAction();
					groupPitchGlideInfo.completionAction = null;
				}
			}
		}
		GroupPitchGlides.RemoveAll((GroupPitchGlideInfo obj) => !obj.IsActive);
	}

	private void PerformBusPitchGlides()
	{
		for (int i = 0; i < BusPitchGlides.Count; i++)
		{
			BusPitchGlideInfo busPitchGlideInfo = BusPitchGlides[i];
			if (!busPitchGlideInfo.IsActive)
			{
				continue;
			}
			if (GetBusIndex(busPitchGlideInfo.NameOfBus, alertMissing: true) < 0)
			{
				busPitchGlideInfo.IsActive = false;
			}
			else if (!(AudioUtil.Time < busPitchGlideInfo.CompletionTime))
			{
				busPitchGlideInfo.IsActive = false;
				if (busPitchGlideInfo.completionAction != null)
				{
					busPitchGlideInfo.completionAction();
					busPitchGlideInfo.completionAction = null;
				}
			}
		}
		BusPitchGlides.RemoveAll((BusPitchGlideInfo obj) => !obj.IsActive);
	}

	private void OnApplicationQuit()
	{
		AppIsShuttingDown = true;
	}

	public static bool PlaySoundAndForget(string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null)
	{
		if (!SceneHasMasterAudio)
		{
			return false;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			return false;
		}
		return PSRAsSuccessBool(PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, timeToSchedulePlay, pitch, null, variationName, attachToSource: false, delaySoundTime));
	}

	public static PlaySoundResult PlaySound(string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isChaining = false, bool isSingleSubscribedPlay = false)
	{
		if (!SceneHasMasterAudio)
		{
			return null;
		}
		if (SoundsReady)
		{
			return PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, timeToSchedulePlay, pitch, null, variationName, attachToSource: false, delaySoundTime, useVector3: false, makePlaySoundResult: true, isChaining, isSingleSubscribedPlay);
		}
		Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
		return null;
	}

	public static bool PlaySound3DAtVector3AndForget(string sType, Vector3 sourcePosition, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null)
	{
		if (!SceneHasMasterAudio)
		{
			return false;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			return false;
		}
		return PSRAsSuccessBool(PlaySoundAtVolume(sType, volumePercentage, sourcePosition, timeToSchedulePlay, pitch, null, variationName, attachToSource: false, delaySoundTime, useVector3: true));
	}

	public static PlaySoundResult PlaySound3DAtVector3(string sType, Vector3 sourcePosition, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null)
	{
		if (!SceneHasMasterAudio)
		{
			return null;
		}
		if (SoundsReady)
		{
			return PlaySoundAtVolume(sType, volumePercentage, sourcePosition, timeToSchedulePlay, pitch, null, variationName, attachToSource: false, delaySoundTime, useVector3: true, makePlaySoundResult: true);
		}
		Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
		return null;
	}

	public static bool PlaySound3DAtTransformAndForget(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null)
	{
		if (!SceneHasMasterAudio)
		{
			return false;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			return false;
		}
		return PSRAsSuccessBool(PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, timeToSchedulePlay, pitch, sourceTrans, variationName, attachToSource: false, delaySoundTime));
	}

	public static PlaySoundResult PlaySound3DAtTransform(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isChaining = false, bool isSingleSubscribedPlay = false)
	{
		if (!SceneHasMasterAudio)
		{
			return null;
		}
		if (SoundsReady)
		{
			return PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, timeToSchedulePlay, pitch, sourceTrans, variationName, attachToSource: false, delaySoundTime, useVector3: false, makePlaySoundResult: true, isChaining, isSingleSubscribedPlay);
		}
		Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
		return null;
	}

	public static bool PlaySound3DFollowTransformAndForget(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null)
	{
		if (!SceneHasMasterAudio)
		{
			return false;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			return false;
		}
		return PSRAsSuccessBool(PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, timeToSchedulePlay, pitch, sourceTrans, variationName, attachToSource: true, delaySoundTime));
	}

	public static PlaySoundResult PlaySound3DFollowTransform(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isChaining = false, bool isSingleSubscribedPlay = false)
	{
		if (!SceneHasMasterAudio)
		{
			return null;
		}
		if (SoundsReady)
		{
			return PlaySoundAtVolume(sType, volumePercentage, Vector3.zero, timeToSchedulePlay, pitch, sourceTrans, variationName, attachToSource: true, delaySoundTime, useVector3: false, makePlaySoundResult: true, isChaining, isSingleSubscribedPlay);
		}
		Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
		return null;
	}

	public static IEnumerator PlaySoundAndWaitUntilFinished(string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, Action completedAction = null)
	{
		if (!SceneHasMasterAudio)
		{
			yield break;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			yield break;
		}
		PlaySoundResult playSoundResult = PlaySound(sType, volumePercentage, pitch, delaySoundTime, variationName, null, isChaining: false, isSingleSubscribedPlay: true);
		bool done = false;
		if (playSoundResult != null && !(playSoundResult.ActingVariation == null))
		{
			playSoundResult.ActingVariation.SoundFinished += delegate
			{
				done = true;
			};
			while (!done)
			{
				yield return EndOfFrameDelay;
			}
			completedAction?.Invoke();
		}
	}

	public static IEnumerator PlaySound3DAtTransformAndWaitUntilFinished(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, Action completedAction = null)
	{
		if (!SceneHasMasterAudio)
		{
			yield break;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			yield break;
		}
		PlaySoundResult playSoundResult = PlaySound3DAtTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay, isChaining: false, isSingleSubscribedPlay: true);
		bool done = false;
		if (playSoundResult != null && !(playSoundResult.ActingVariation == null))
		{
			playSoundResult.ActingVariation.SoundFinished += delegate
			{
				done = true;
			};
			while (!done)
			{
				yield return EndOfFrameDelay;
			}
			completedAction?.Invoke();
		}
	}

	public static IEnumerator PlaySound3DFollowTransformAndWaitUntilFinished(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, Action completedAction = null)
	{
		if (!SceneHasMasterAudio)
		{
			yield break;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot play: " + sType);
			yield break;
		}
		PlaySoundResult playSoundResult = PlaySound3DFollowTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay, isChaining: false, isSingleSubscribedPlay: true);
		bool done = false;
		if (playSoundResult != null && !(playSoundResult.ActingVariation == null))
		{
			playSoundResult.ActingVariation.SoundFinished += delegate
			{
				done = true;
			};
			while (!done)
			{
				yield return EndOfFrameDelay;
			}
			completedAction?.Invoke();
		}
	}

	public static bool PSRAsSuccessBool(PlaySoundResult psr)
	{
		if (psr != null)
		{
			if (!psr.SoundPlayed)
			{
				return psr.SoundScheduled;
			}
			return true;
		}
		return false;
	}

	private static PlaySoundResult PlaySoundAtVolume(string sType, float volumePercentage, Vector3 sourcePosition, double? timeToSchedulePlay, float? pitch = null, Transform sourceTrans = null, string variationName = null, bool attachToSource = false, float delaySoundTime = 0f, bool useVector3 = false, bool makePlaySoundResult = false, bool isChaining = false, bool isSingleSubscribedPlay = false, bool triggeredAsChildGroup = false)
	{
		if (!SceneHasMasterAudio)
		{
			return null;
		}
		if (!SoundsReady || sType == string.Empty || sType == "[None]")
		{
			return null;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			string text = "MasterAudio could not find sound: " + sType + ". If your Scene just changed, this could happen when an OnDisable or OnInvisible event sound happened to a per-scene sound, which is expected.";
			if (sourceTrans != null)
			{
				text = text + " Triggered by prefab: " + sourceTrans.name;
			}
			LogWarning(text);
			return null;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		MasterAudioGroup group = audioGroupInfo.Group;
		bool flag = LoggingEnabledForGroup(group);
		if (Instance.mixerMuted)
		{
			if (flag)
			{
				LogMessage("MasterAudio playing sound: " + sType + " silently because the Mixer is muted.");
			}
		}
		else if (group.isMuted && flag)
		{
			LogMessage("MasterAudio playing sound: " + sType + " silently because the Group is muted.");
		}
		if (Instance.SoloedGroups.Count > 0 && !Instance.SoloedGroups.Contains(group) && flag)
		{
			LogMessage("MasterAudio playing sound: " + sType + " silently because there are one or more Groups soloed. This one is not.");
		}
		audioGroupInfo.PlayedForWarming = IsWarming;
		if (group.curVariationMode == MasterAudioGroup.VariationMode.Normal)
		{
			switch (group.limitMode)
			{
			case MasterAudioGroup.LimitMode.TimeBased:
				if (group.minimumTimeBetween > 0f && Time.realtimeSinceStartup < audioGroupInfo.LastTimePlayed + group.minimumTimeBetween)
				{
					if (flag)
					{
						LogMessage("MasterAudio skipped playing sound: " + sType + " due to Group's Min Seconds Between setting.");
					}
					return null;
				}
				break;
			case MasterAudioGroup.LimitMode.FrameBased:
				if (Time.frameCount - audioGroupInfo.LastFramePlayed < group.limitPerXFrames)
				{
					if (flag)
					{
						LogMessage("Master Audio skipped playing sound: " + sType + " due to Group's Per Frame Limit.");
					}
					return null;
				}
				break;
			}
		}
		SetLastPlayed(audioGroupInfo);
		List<AudioInfo> sources = audioGroupInfo.Sources;
		bool flag2 = string.IsNullOrEmpty(variationName);
		if (sources.Count == 0)
		{
			if (flag)
			{
				LogMessage("Sound Group {" + sType + "} has no active Variations.");
			}
			return null;
		}
		if (group.curVariationMode == MasterAudioGroup.VariationMode.Normal && audioGroupInfo.Group.limitPolyphony)
		{
			int voiceLimitCount = audioGroupInfo.Group.voiceLimitCount;
			int num = 0;
			for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
			{
				if (audioGroupInfo.Sources[i].Source == null || !audioGroupInfo.Sources[i].Source.isPlaying)
				{
					continue;
				}
				num++;
				if (num >= voiceLimitCount)
				{
					if (flag || LogOutOfVoices)
					{
						LogMessage("Polyphony limit of group: " + audioGroupInfo.Group.GameObjectName + " exceeded. Will not play this sound for this instance.");
					}
					return null;
				}
			}
		}
		GroupBus busForGroup = audioGroupInfo.Group.BusForGroup;
		if (busForGroup != null && busForGroup.BusVoiceLimitReached)
		{
			if (!busForGroup.stopOldest)
			{
				if (flag || LogOutOfVoices)
				{
					LogMessage("Bus voice limit has been reached. Cannot play the sound: " + audioGroupInfo.Group.GameObjectName + " until one voice has stopped playing. You can turn on the 'Stop Oldest' option for the bus to change ");
				}
				return null;
			}
			StopOldestSoundOnBus(busForGroup);
		}
		AudioInfo audioInfo = null;
		bool isSingleVarLoop = false;
		if (sources.Count == 1)
		{
			if (flag)
			{
				LogMessage("Cueing only child of " + sType);
			}
			audioInfo = sources[0];
			if (group.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain)
			{
				isSingleVarLoop = true;
			}
		}
		List<int> list = null;
		int? randomIndex = null;
		List<int> list2 = null;
		int num2 = -1;
		if (audioInfo == null)
		{
			if (!Instance._randomizer.ContainsKey(sType))
			{
				Debug.Log("Sound Group {" + sType + "} has no active Variations.");
				return null;
			}
			if (flag2)
			{
				list = Instance._randomizer[sType];
				randomIndex = 0;
				num2 = list[randomIndex.Value];
				audioInfo = sources[num2];
				list2 = Instance._randomizerLeftovers[sType];
				list2.Remove(num2);
				if (flag)
				{
					LogMessage($"Cueing child {list[randomIndex.Value]} of {sType}");
				}
			}
			else
			{
				bool flag3 = false;
				int num3 = 0;
				for (int j = 0; j < sources.Count; j++)
				{
					AudioInfo audioInfo2 = sources[j];
					if (!(audioInfo2.Source.name != variationName))
					{
						num3++;
						if (audioInfo2.Variation.IsAvailableToPlay)
						{
							audioInfo = audioInfo2;
							flag3 = true;
							num2 = j;
							break;
						}
					}
				}
				if (!flag3)
				{
					if (num3 == 0)
					{
						if (flag)
						{
							LogMessage("Can't find variation {" + variationName + "} of " + sType);
						}
					}
					else if (flag || LogOutOfVoices)
					{
						LogMessage("Can't find non-busy variation {" + variationName + "} of " + sType);
					}
					return null;
				}
				if (flag)
				{
					LogMessage($"Cueing child named '{variationName}' of {sType}");
				}
			}
		}
		if (audioInfo.Variation == null)
		{
			if (AppIsShuttingDown || audioInfo.Source == null)
			{
				return null;
			}
			SoundGroupVariation component = audioInfo.Source.GetComponent<SoundGroupVariation>();
			if (component == null)
			{
				return null;
			}
			audioInfo.Variation = component;
		}
		if (audioInfo.Variation.audLocation == AudioLocation.Clip && audioInfo.Variation.VarAudio.clip == null)
		{
			if (flag)
			{
				LogMessage($"Child named '{audioInfo.Variation.name}' of {sType} has no audio assigned to it so nothing will be played.");
			}
			RemoveClipAndRefillIfEmpty(audioGroupInfo, flag2, randomIndex, list, sType, num2, flag, isSingleVarLoop: false);
			MaybeChainNextVar(isChaining, audioInfo.Variation, volumePercentage, pitch, sourceTrans, attachToSource);
			return null;
		}
		if (audioInfo.Variation.probabilityToPlay < 100 && UnityEngine.Random.Range(0, 100) >= audioInfo.Variation.probabilityToPlay)
		{
			if (flag)
			{
				LogMessage($"Child named '{audioInfo.Variation.name}' of {sType} failed its Random number check for 'Probability to Play' to it so nothing will be played this time.");
			}
			RemoveClipAndRefillIfEmpty(audioGroupInfo, flag2, randomIndex, list, sType, num2, flag, isSingleVarLoop: false);
			MaybeChainNextVar(isChaining, audioInfo.Variation, volumePercentage, pitch, sourceTrans, attachToSource);
			return null;
		}
		if (audioGroupInfo.Group.curVariationMode == MasterAudioGroup.VariationMode.Dialog)
		{
			if (audioGroupInfo.Group.useDialogFadeOut)
			{
				FadeOutAllOfSound(audioGroupInfo.Group.GameObjectName, audioGroupInfo.Group.dialogFadeOutTime);
			}
			else
			{
				StopAllOfSound(audioGroupInfo.Group.GameObjectName);
			}
		}
		bool flag4 = false;
		bool forgetSoundPlayed = false;
		bool flag5 = false;
		bool flag6;
		PlaySoundResult playSoundResult;
		bool flag8;
		do
		{
			flag6 = false;
			playSoundResult = PlaySoundIfAvailable(audioInfo, sourcePosition, volumePercentage, ref forgetSoundPlayed, pitch, audioGroupInfo, sourceTrans, attachToSource, delaySoundTime, useVector3, makePlaySoundResult, timeToSchedulePlay, isChaining, isSingleSubscribedPlay);
			bool num4 = makePlaySoundResult && playSoundResult != null && (playSoundResult.SoundPlayed || playSoundResult.SoundScheduled);
			bool flag7 = !makePlaySoundResult && forgetSoundPlayed;
			flag8 = num4 || flag7;
			if (flag8)
			{
				flag4 = true;
				if (!IsWarming)
				{
					RemoveClipAndRefillIfEmpty(audioGroupInfo, flag2, randomIndex, list, sType, num2, flag, isSingleVarLoop);
				}
			}
			else if (flag2)
			{
				if (list2 == null)
				{
					continue;
				}
				if (list2.Count <= 0)
				{
					if (flag5)
					{
						continue;
					}
					RefillSoundGroupPool(sType);
					flag5 = true;
					list2.Clear();
					list2.AddRange(list);
				}
				audioInfo = sources[list2[0]];
				if (audioInfo.Variation == null)
				{
					SoundGroupVariation component2 = audioInfo.Source.GetComponent<SoundGroupVariation>();
					if (component2 == null)
					{
						break;
					}
					audioInfo.Variation = component2;
				}
				if (flag)
				{
					LogMessage("Child was busy. Cueing child {" + audioInfo.Source.name + "} of " + sType);
				}
				list2.RemoveAt(0);
				if (flag5 && list2.Count == 0)
				{
					flag6 = true;
				}
			}
			else
			{
				if (flag)
				{
					LogMessage("Child was busy. Since you wanted a named Variation, no others to try. Aborting.");
				}
				list2?.Clear();
			}
		}
		while (!flag4 && list2 != null && (list2.Count > 0 || !flag5 || flag6));
		if (!flag8)
		{
			if (flag || LogOutOfVoices)
			{
				LogMessage("All children of " + sType + " were busy. Will not play this sound for this instance.");
			}
		}
		else
		{
			if (!triggeredAsChildGroup && !IsWarming)
			{
				switch (audioGroupInfo.Group.linkedStartGroupSelectionType)
				{
				case LinkedGroupSelectionType.All:
				{
					for (int k = 0; k < audioGroupInfo.Group.childSoundGroups.Count; k++)
					{
						PlaySoundAtVolume(audioGroupInfo.Group.childSoundGroups[k], volumePercentage, sourcePosition, timeToSchedulePlay, pitch, sourceTrans, null, attachToSource, delaySoundTime, useVector3, makePlaySoundResult: false, isChaining: false, isSingleSubscribedPlay: false, triggeredAsChildGroup: true);
					}
					break;
				}
				case LinkedGroupSelectionType.OneAtRandom:
				{
					int index = UnityEngine.Random.Range(0, audioGroupInfo.Group.childSoundGroups.Count);
					PlaySoundAtVolume(audioGroupInfo.Group.childSoundGroups[index], volumePercentage, sourcePosition, timeToSchedulePlay, pitch, sourceTrans, null, attachToSource, delaySoundTime, useVector3, makePlaySoundResult: false, isChaining: false, isSingleSubscribedPlay: false, triggeredAsChildGroup: true);
					break;
				}
				}
			}
			if (audioGroupInfo.Group.soundPlayedEventActive)
			{
				FireCustomEvent(audioGroupInfo.Group.soundPlayedCustomEvent, Instance._trans);
			}
		}
		if (!makePlaySoundResult && flag8)
		{
			return AndForgetSuccessResult;
		}
		return playSoundResult;
	}

	private static void MaybeChainNextVar(bool isChaining, SoundGroupVariation variation, float volumePercentage, float? pitch, Transform sourceTrans, bool attachToSource)
	{
		if (isChaining)
		{
			variation.DoNextChain(volumePercentage, pitch, sourceTrans, attachToSource);
		}
	}

	private static void SetLastPlayed(AudioGroupInfo grp)
	{
		grp.LastTimePlayed = AudioUtil.Time;
		grp.LastFramePlayed = AudioUtil.FrameCount;
	}

	private static void RemoveClipAndRefillIfEmpty(AudioGroupInfo grp, bool isNonSpecific, int? randomIndex, List<int> choices, string sType, int pickedChoice, bool loggingEnabledForGrp, bool isSingleVarLoop)
	{
		if (isSingleVarLoop)
		{
			grp.Group.ChainLoopCount++;
			return;
		}
		if (isNonSpecific && randomIndex.HasValue)
		{
			choices.RemoveAt(randomIndex.Value);
			Instance._clipsPlayedBySoundTypeOldestFirst[sType].Add(pickedChoice);
			if (choices.Count == 0)
			{
				if (loggingEnabledForGrp)
				{
					LogMessage("Refilling Variation pool: " + sType);
				}
				RefillSoundGroupPool(sType);
			}
		}
		if (grp.Group.curVariationSequence == MasterAudioGroup.VariationSequence.TopToBottom && grp.Group.useInactivePeriodPoolRefill)
		{
			UpdateRefillTime(sType, grp.Group.inactivePeriodSeconds);
		}
	}

	private static PlaySoundResult PlaySoundIfAvailable(AudioInfo info, Vector3 sourcePosition, float volumePercentage, ref bool forgetSoundPlayed, float? pitch = null, AudioGroupInfo audioGroup = null, Transform sourceTrans = null, bool attachToSource = false, float delaySoundTime = 0f, bool useVector3 = false, bool makePlaySoundResult = false, double? timeToSchedulePlay = null, bool isChaining = false, bool isSingleSubscribedPlay = false)
	{
		if (info.Source == null)
		{
			return null;
		}
		MasterAudioGroup group = audioGroup.Group;
		if (group.curVariationMode == MasterAudioGroup.VariationMode.Normal && info.Source.isPlaying)
		{
			float audioPlayedPercentage = AudioUtil.GetAudioPlayedPercentage(info.Source);
			int retriggerPercentage = group.retriggerPercentage;
			if (audioPlayedPercentage < (float)retriggerPercentage)
			{
				return null;
			}
		}
		info.Variation.Stop(stopEndDetection: false, skipLinked: true);
		info.Variation.ObjectToFollow = null;
		bool flag = Instance.prioritizeOnDistance && (Instance.useClipAgePriority || info.Variation.ParentGroup.useClipAgePriority);
		if (useVector3)
		{
			info.Source.transform.position = sourcePosition;
			if (Instance.prioritizeOnDistance)
			{
				AudioPrioritizer.Set3DPriority(info.Variation, flag);
			}
		}
		else if (sourceTrans != null)
		{
			if (attachToSource)
			{
				info.Variation.ObjectToFollow = sourceTrans;
			}
			else
			{
				info.Source.transform.position = sourceTrans.position;
				info.Variation.ObjectToTriggerFrom = sourceTrans;
			}
			if (Instance.prioritizeOnDistance)
			{
				AudioPrioritizer.Set3DPriority(info.Variation, flag);
			}
		}
		else
		{
			if (Instance.prioritizeOnDistance)
			{
				AudioPrioritizer.Set2DSoundPriority(info.Source);
			}
			info.Source.transform.localPosition = Vector3.zero;
		}
		float groupMasterVolume = group.groupMasterVolume;
		float busVolume = GetBusVolume(group);
		float num = info.OriginalVolume;
		float num2 = 0f;
		if (info.Variation.useRandomVolume)
		{
			num2 = UnityEngine.Random.Range(info.Variation.randomVolumeMin, info.Variation.randomVolumeMax);
			switch (info.Variation.randomVolumeMode)
			{
			case SoundGroupVariation.RandomVolumeMode.AddToClipVolume:
				num += num2;
				break;
			case SoundGroupVariation.RandomVolumeMode.IgnoreClipVolume:
				num = num2;
				break;
			}
		}
		float num3 = num * groupMasterVolume * busVolume * Instance._masterAudioVolume;
		float num4 = num3 * volumePercentage;
		info.Source.volume = num4;
		info.LastPercentageVolume = volumePercentage;
		info.LastRandomVolume = num2;
		if (!info.Variation.GameObj.activeInHierarchy)
		{
			return null;
		}
		PlaySoundResult playSoundResult = null;
		if (makePlaySoundResult)
		{
			playSoundResult = new PlaySoundResult
			{
				ActingVariation = info.Variation
			};
			if (delaySoundTime > 0f)
			{
				playSoundResult.SoundScheduled = true;
			}
			else
			{
				playSoundResult.SoundPlayed = true;
			}
		}
		else
		{
			forgetSoundPlayed = true;
		}
		string gameObjectName = group.GameObjectName;
		if (group.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain)
		{
			if (!isChaining)
			{
				group.ChainLoopCount = 0;
			}
			Transform objectToFollow = info.Variation.ObjectToFollow;
			if (group.ActiveVoices > 0 && !isChaining)
			{
				StopAllOfSound(gameObjectName);
			}
			info.Variation.ObjectToFollow = objectToFollow;
		}
		info.Variation.Play(pitch, num4, gameObjectName, volumePercentage, num3, pitch, sourceTrans, attachToSource, delaySoundTime, timeToSchedulePlay, isChaining, isSingleSubscribedPlay);
		if (Instance._isStoppingMultiple)
		{
			Instance.VariationsStartedDuringMultiStop.Add(info.Variation);
		}
		return playSoundResult;
	}

	public static void DuckSoundGroup(string soundGroupName, AudioSource aSource)
	{
		MasterAudio instance = Instance;
		if (instance.EnableMusicDucking && instance.duckingBySoundType.ContainsKey(soundGroupName) && !(aSource.clip == null))
		{
			DuckGroupInfo duckGroupInfo = instance.duckingBySoundType[soundGroupName];
			float length = aSource.clip.length;
			float pitch = aSource.pitch;
			List<PlaylistController> instances = PlaylistController.Instances;
			for (int i = 0; i < instances.Count; i++)
			{
				instances[i].DuckMusicForTime(length, duckGroupInfo.unduckTime, pitch, duckGroupInfo.riseVolStart, duckGroupInfo.duckedVolumeCut);
			}
		}
	}

	private static void StopPauseOrUnpauseSoundsOfTransform(Transform trans, List<AudioInfo> varList, VariationCommand varCmd)
	{
		MasterAudioGroup masterAudioGroup = null;
		for (int i = 0; i < varList.Count; i++)
		{
			SoundGroupVariation variation = varList[i].Variation;
			if (!variation.WasTriggeredFromTransform(trans))
			{
				continue;
			}
			if (masterAudioGroup == null)
			{
				masterAudioGroup = GrabGroup(variation.ParentGroup.GameObjectName);
			}
			bool stopEndDetection = masterAudioGroup != null && masterAudioGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
			switch (varCmd)
			{
			case VariationCommand.Stop:
				variation.Stop(stopEndDetection);
				break;
			case VariationCommand.Pause:
				variation.Pause();
				break;
			case VariationCommand.Unpause:
				if (AudioUtil.IsAudioPaused(variation.VarAudio))
				{
					variation.VarAudio.Play();
				}
				break;
			}
		}
	}

	public static void StopAllSoundsOfTransform(Transform sourceTrans)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		Instance.VariationsStartedDuringMultiStop.Clear();
		Instance._isStoppingMultiple = true;
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			StopPauseOrUnpauseSoundsOfTransform(sourceTrans, sources, VariationCommand.Stop);
		}
		Instance._isStoppingMultiple = false;
	}

	public static void StopSoundGroupOfTransform(Transform sourceTrans, string sType)
	{
		if (SceneHasMasterAudio)
		{
			if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
			{
				Debug.LogWarning("Could not locate group '" + sType + "'.");
				return;
			}
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
			StopPauseOrUnpauseSoundsOfTransform(sourceTrans, sources, VariationCommand.Stop);
		}
	}

	public static void PauseAllSoundsOfTransform(Transform sourceTrans)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			StopPauseOrUnpauseSoundsOfTransform(sourceTrans, sources, VariationCommand.Pause);
		}
	}

	public static void PauseSoundGroupOfTransform(Transform sourceTrans, string sType)
	{
		if (SceneHasMasterAudio)
		{
			if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
			{
				Debug.LogWarning("Could not locate group '" + sType + "'.");
				return;
			}
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
			StopPauseOrUnpauseSoundsOfTransform(sourceTrans, sources, VariationCommand.Pause);
		}
	}

	public static void UnpauseAllSoundsOfTransform(Transform sourceTrans)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			StopPauseOrUnpauseSoundsOfTransform(sourceTrans, sources, VariationCommand.Unpause);
		}
	}

	public static void UnpauseSoundGroupOfTransform(Transform sourceTrans, string sType)
	{
		if (SceneHasMasterAudio)
		{
			if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
			{
				Debug.LogWarning("Could not locate group '" + sType + "'.");
				return;
			}
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
			StopPauseOrUnpauseSoundsOfTransform(sourceTrans, sources, VariationCommand.Unpause);
		}
	}

	public static void FadeOutAllSoundsOfTransform(Transform sourceTrans, float fadeTime)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		List<SoundGroupVariation> allPlayingVariationsOfTransform = GetAllPlayingVariationsOfTransform(sourceTrans);
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < allPlayingVariationsOfTransform.Count; i++)
		{
			string text = allPlayingVariationsOfTransform[i].ParentGroup.name;
			if (!hashSet.Contains(text))
			{
				hashSet.Add(text);
				FadeOutSoundGroupOfTransform(sourceTrans, text, fadeTime);
			}
		}
	}

	public static void FadeOutSoundGroupOfTransform(Transform sourceTrans, string sType, float fadeTime)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
		for (int i = 0; i < sources.Count; i++)
		{
			SoundGroupVariation variation = sources[i].Variation;
			if (variation.WasTriggeredFromTransform(sourceTrans))
			{
				variation.FadeOutNow(fadeTime);
			}
		}
	}

	public static void StopAllOfSound(string sType)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		bool stopEndDetection = masterAudioGroup != null && masterAudioGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
		foreach (AudioInfo item in sources)
		{
			if (!(item.Variation == null) && !IsLinkedGroupPlay(item.Variation))
			{
				item.Variation.Stop(stopEndDetection);
			}
		}
	}

	public static void FadeOutAllOfSound(string sType, float fadeTime)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		foreach (AudioInfo source in Instance.AudioSourcesBySoundType[sType].Sources)
		{
			source.Variation.FadeOutNow(fadeTime);
		}
	}

	public static List<SoundGroupVariation> GetAllPlayingVariations()
	{
		List<SoundGroupVariation> list = new List<SoundGroupVariation>(32);
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			for (int i = 0; i < sources.Count; i++)
			{
				SoundGroupVariation variation = sources[i].Variation;
				if (variation.IsPlaying)
				{
					list.Add(variation);
				}
			}
		}
		return list;
	}

	public static List<SoundGroupVariation> GetAllPlayingVariationsOfTransform(Transform sourceTrans)
	{
		List<SoundGroupVariation> list = new List<SoundGroupVariation>(32);
		if (!SceneHasMasterAudio)
		{
			return list;
		}
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			for (int i = 0; i < sources.Count; i++)
			{
				SoundGroupVariation variation = sources[i].Variation;
				if (variation.WasTriggeredFromTransform(sourceTrans))
				{
					list.Add(variation);
				}
			}
		}
		return list;
	}

	public static List<SoundGroupVariation> GetAllPlayingVariationsOfTransformList(List<Transform> sourceTransList)
	{
		List<SoundGroupVariation> list = new List<SoundGroupVariation>(32);
		if (!SceneHasMasterAudio)
		{
			return list;
		}
		HashSet<Transform> hashSet = new HashSet<Transform>();
		for (int i = 0; i < sourceTransList.Count; i++)
		{
			hashSet.Add(sourceTransList[i]);
		}
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			for (int j = 0; j < sources.Count; j++)
			{
				SoundGroupVariation variation = sources[j].Variation;
				if (variation.WasTriggeredFromAnyOfTransformMap(hashSet))
				{
					list.Add(variation);
				}
			}
		}
		return list;
	}

	public static List<SoundGroupVariation> GetAllPlayingVariationsInBus(string busName)
	{
		List<SoundGroupVariation> list = new List<SoundGroupVariation>(32);
		int busIndex = GetBusIndex(busName, alertMissing: false);
		if (busIndex < 0)
		{
			return list;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AudioGroupInfo value = enumerator.Current.Value;
			if (value.Group.busIndex != busIndex)
			{
				continue;
			}
			for (int i = 0; i < value.Sources.Count; i++)
			{
				SoundGroupVariation variation = value.Sources[i].Variation;
				if (variation.IsPlaying)
				{
					list.Add(variation);
				}
			}
		}
		return list;
	}

	public static void DeleteGroupVariation(string sType, string variationName)
	{
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot delete Variation clip yet.");
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		List<AudioInfo> list = new List<AudioInfo>();
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			AudioInfo audioInfo = audioGroupInfo.Sources[i];
			if (!(audioInfo.Variation.name != variationName))
			{
				list.Add(audioInfo);
			}
		}
		if (list.Count == 0)
		{
			LogWarning("Could not find Variation for '" + sType + "' Group named '" + variationName + "'.\nWill not delete any Variations.");
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			AudioInfo audioInfo2 = list[j];
			SoundGroupVariation variation = audioInfo2.Variation;
			variation.Stop();
			variation.DisableUpdater();
			if (variation.audLocation == AudioLocation.ResourceFile)
			{
				AudioResourceOptimizer.DeleteAudioSourceFromList((variation.VarAudio.clip == null) ? string.Empty : variation.VarAudio.clip.name, variation.VarAudio);
			}
			int num = audioGroupInfo.Sources.IndexOf(audioInfo2);
			if (num >= 0)
			{
				Instance._randomizer[sType].Remove(num);
				for (int k = 0; k < Instance._randomizer[sType].Count; k++)
				{
					if (Instance._randomizer[sType][k] > num)
					{
						Instance._randomizer[sType][k]--;
					}
				}
				Instance._randomizerOrigin[sType].Remove(num);
				for (int l = 0; l < Instance._randomizerOrigin[sType].Count; l++)
				{
					if (Instance._randomizerOrigin[sType][l] > num)
					{
						Instance._randomizerOrigin[sType][l]--;
					}
				}
				Instance._randomizerLeftovers[sType].Remove(num);
				for (int m = 0; m < Instance._randomizerLeftovers[sType].Count; m++)
				{
					if (Instance._randomizerLeftovers[sType][m] > num)
					{
						Instance._randomizerLeftovers[sType][m]--;
					}
				}
				Instance._clipsPlayedBySoundTypeOldestFirst[sType].Remove(num);
				audioGroupInfo.Sources.RemoveAt(num);
			}
			Instance.OcclusionSourcesInRange.Remove(variation.GameObj);
			Instance.OcclusionSourcesOutOfRange.Remove(variation.GameObj);
			Instance.OcclusionSourcesBlocked.Remove(variation.GameObj);
			RemoveFromOcclusionFrequencyTransitioning(variation);
			Instance.AllAudioSources.Remove(variation.VarAudio);
			UnityEngine.Object.Destroy(variation.GameObj);
		}
	}

	public static void CreateGroupVariationFromClip(string sType, AudioClip clip, string variationName, float volume = 1f, float pitch = 1f)
	{
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot create change variation clip yet.");
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		bool flag = false;
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			if (!(audioGroupInfo.Sources[i].Variation.name != variationName))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			LogWarning("You already have a Variation for this Group named '" + variationName + "'. \n\nPlease rename these Variations when finished to be unique, or you may not be able to play them by name if you have a need to.");
		}
		GameObject obj = UnityEngine.Object.Instantiate(Instance.soundGroupVariationTemplate.gameObject, audioGroupInfo.Group.transform.position, Quaternion.identity);
		obj.transform.name = variationName;
		obj.transform.parent = audioGroupInfo.Group.transform;
		AudioSource component = obj.GetComponent<AudioSource>();
		component.clip = clip;
		component.pitch = pitch;
		Instance.AllAudioSources.Add(component);
		SoundGroupVariation component2 = obj.GetComponent<SoundGroupVariation>();
		component2.DisableUpdater();
		AudioInfo item = new AudioInfo(component2, component2.VarAudio, volume);
		audioGroupInfo.Sources.Add(item);
		if (Instance._randomizer.ContainsKey(sType))
		{
			int item2 = audioGroupInfo.Sources.Count - 1;
			Instance._randomizer[sType].Add(item2);
			Instance._randomizerOrigin[sType].Add(item2);
			Instance._randomizerLeftovers[sType].Add(audioGroupInfo.Sources.Count - 1);
		}
	}

	public static void ChangeVariationPitch(string sType, bool changeAllVariations, string variationName, float pitch)
	{
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot change variation clip yet.");
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		int num = 0;
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			AudioInfo audioInfo = audioGroupInfo.Sources[i];
			if (changeAllVariations || !(audioInfo.Source.transform.name != variationName))
			{
				audioInfo.Variation.original_pitch = pitch;
				AudioSource varAudio = audioInfo.Variation.VarAudio;
				if (varAudio != null)
				{
					varAudio.pitch = pitch;
				}
				num++;
			}
		}
		if (num == 0 && !changeAllVariations)
		{
			Debug.Log("Could not find any matching variations of Sound Group '" + sType + "' to change the pitch of.");
		}
	}

	public static void ChangeVariationVolume(string sType, bool changeAllVariations, string variationName, float volume)
	{
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot change variation clip yet.");
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		int num = 0;
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			AudioInfo audioInfo = audioGroupInfo.Sources[i];
			if (changeAllVariations || !(audioInfo.Source.transform.name != variationName))
			{
				audioInfo.OriginalVolume = volume;
				num++;
			}
		}
		if (num == 0 && !changeAllVariations)
		{
			Debug.Log("Could not find any matching variations of Sound Group '" + sType + "' to change the volume of.");
		}
	}

	public static void ChangeVariationClipFromResources(string sType, bool changeAllVariations, string variationName, string resourceFileName)
	{
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot create change variation clip yet.");
			return;
		}
		AudioClip audioClip = Resources.Load(resourceFileName) as AudioClip;
		if (audioClip == null)
		{
			LogWarning("Resource file '" + resourceFileName + "' could not be located.");
		}
		else
		{
			ChangeVariationClip(sType, changeAllVariations, variationName, audioClip);
		}
	}

	public static void ChangeVariationClip(string sType, bool changeAllVariations, string variationName, AudioClip clip)
	{
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot create change variation clip yet.");
			return;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			AudioInfo audioInfo = audioGroupInfo.Sources[i];
			if (changeAllVariations || audioInfo.Source.transform.name == variationName)
			{
				if (audioInfo.Variation.IsPlaying)
				{
					audioInfo.Variation.Stop();
				}
				audioInfo.Source.clip = clip;
			}
		}
	}

	public static void GradualOcclusionFreqChange(SoundGroupVariation variation, float fadeTime, float newCutoffFreq)
	{
		if (IsOcclusionFreqencyTransitioning(variation))
		{
			LogWarning("Occlusion is already fading for: " + variation.name + ". This is a bug.");
			return;
		}
		OcclusionFreqChangeInfo item = new OcclusionFreqChangeInfo
		{
			ActingVariation = variation,
			CompletionTime = Time.realtimeSinceStartup + fadeTime,
			IsActive = true,
			StartFrequency = variation.LowPassFilter.cutoffFrequency,
			StartTime = Time.realtimeSinceStartup,
			TargetFrequency = newCutoffFreq
		};
		Instance.VariationOcclusionFreqChanges.Add(item);
	}

	public static bool IsSoundGroupPlaying(string sType)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType, logIfMissing: false);
		if (masterAudioGroup == null || AppIsShuttingDown)
		{
			return false;
		}
		return masterAudioGroup.ActiveVoices > 0;
	}

	public static bool IsTransformPlayingSoundGroup(string sType, Transform sourceTrans)
	{
		if (!SceneHasMasterAudio)
		{
			return false;
		}
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogWarning("Could not locate group '" + sType + "'.");
			return false;
		}
		List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].Variation.WasTriggeredFromTransform(sourceTrans))
			{
				return true;
			}
		}
		return false;
	}

	public static void RouteGroupToBus(string sType, string busName)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (masterAudioGroup == null)
		{
			LogError("Could not find Sound Group '" + sType + "'");
			return;
		}
		int num = 0;
		if (busName != null)
		{
			int num2 = GroupBuses.FindIndex((GroupBus x) => x.busName == busName);
			if (num2 < 0)
			{
				LogError("Could not find bus '" + busName + "' to assign to Sound Group '" + sType + "'");
				return;
			}
			num = 2 + num2;
		}
		GroupBus busByIndex = GetBusByIndex(masterAudioGroup.busIndex);
		masterAudioGroup.busIndex = num;
		GroupBus groupBus = null;
		bool flag = false;
		if (num > 0)
		{
			groupBus = GroupBuses.Find((GroupBus x) => x.busName == busName);
			if (groupBus.isMuted)
			{
				MuteGroup(masterAudioGroup.name, shouldCheckMuteStatus: false);
				flag = true;
			}
			else if (groupBus.isSoloed)
			{
				SoloGroup(masterAudioGroup.name, shouldCheckMuteStatus: false);
				flag = true;
			}
		}
		bool flag2 = false;
		List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
		for (int i = 0; i < sources.Count; i++)
		{
			SoundGroupVariation variation = sources[i].Variation;
			variation.SetMixerGroup();
			variation.SetSpatialBlend();
			if (variation.IsPlaying)
			{
				groupBus?.AddActiveAudioSourceId(variation.InstanceId);
				busByIndex?.RemoveActiveAudioSourceId(variation.InstanceId);
				flag2 = true;
			}
		}
		if (flag2)
		{
			SetBusVolume(groupBus, groupBus?.volume ?? 0f);
		}
		if (Application.isPlaying && flag)
		{
			SilenceOrUnsilenceGroupsFromSoloChange();
		}
	}

	public static float GetVariationLength(string sType, string variationName)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (masterAudioGroup == null)
		{
			return -1f;
		}
		SoundGroupVariation soundGroupVariation = null;
		foreach (SoundGroupVariation groupVariation in masterAudioGroup.groupVariations)
		{
			if (!(groupVariation.name != variationName))
			{
				soundGroupVariation = groupVariation;
				break;
			}
		}
		if (soundGroupVariation == null)
		{
			LogError("Could not find Variation '" + variationName + "' in Sound Group '" + sType + "'.");
			return -1f;
		}
		if (soundGroupVariation.audLocation == AudioLocation.ResourceFile)
		{
			LogError("Variation '" + variationName + "' in Sound Group '" + sType + "' length cannot be determined because it's a Resource Files.");
			return -1f;
		}
		if (soundGroupVariation.audLocation == AudioLocation.FileOnInternet)
		{
			LogError("Variation '" + variationName + "' in Sound Group '" + sType + "' length cannot be determined because it's an Internet File.");
			return -1f;
		}
		AudioClip clip = soundGroupVariation.VarAudio.clip;
		if (clip == null)
		{
			LogError("Variation '" + variationName + "' in Sound Group '" + sType + "' has no Audio Clip.");
			return -1f;
		}
		if (!(soundGroupVariation.VarAudio.pitch <= 0f))
		{
			return AudioUtil.AdjustAudioClipDurationForPitch(clip.length, soundGroupVariation.VarAudio);
		}
		LogError("Variation '" + variationName + "' in Sound Group '" + sType + "' has negative or zero pitch. Cannot compute length.");
		return -1f;
	}

	public static void RefillSoundGroupPool(string sType)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType, logIfMissing: false);
		if (masterAudioGroup == null)
		{
			return;
		}
		List<int> list = Instance._randomizer[sType];
		List<int> list2 = Instance._clipsPlayedBySoundTypeOldestFirst[sType];
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int item = list[i];
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
			}
		}
		List<int> list3 = Instance._randomizerOrigin[sType];
		if (list2.Count < list3.Count)
		{
			for (int j = 0; j < list3.Count; j++)
			{
				int item2 = list3[j];
				if (!list2.Contains(item2))
				{
					list2.Add(item2);
				}
			}
		}
		list.Clear();
		if (masterAudioGroup.curVariationSequence == MasterAudioGroup.VariationSequence.Randomized)
		{
			int? num = null;
			if (masterAudioGroup.UsesNoRepeat && list2.Count > 0)
			{
				num = list2[list2.Count - 1];
			}
			ArrayListUtil.SortIntArray(ref list2);
			if (num.HasValue && num.Value == list2[0])
			{
				int item3 = list2[0];
				list2.RemoveAt(0);
				list2.Insert(UnityEngine.Random.Range(1, list2.Count), item3);
			}
		}
		list.AddRange(list2);
		Instance._randomizerLeftovers[sType].AddRange(list2);
		list2.Clear();
		if (masterAudioGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain)
		{
			masterAudioGroup.ChainLoopCount++;
		}
	}

	public static bool SoundGroupExists(string sType)
	{
		return GrabGroup(sType, logIfMissing: false) != null;
	}

	public static void PauseSoundGroup(string sType)
	{
		if (!(GrabGroup(sType) == null))
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
			for (int i = 0; i < sources.Count; i++)
			{
				sources[i].Variation.Pause();
			}
		}
	}

	public static void SetGroupSpatialBlend(string sType)
	{
		if (!(GrabGroup(sType) == null))
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
			for (int i = 0; i < sources.Count; i++)
			{
				sources[i].Variation.SetSpatialBlend();
			}
		}
	}

	public static void RouteGroupToUnityMixerGroup(string sType, AudioMixerGroup mixerGroup)
	{
		if (Application.isPlaying && !(GrabGroup(sType, logIfMissing: false) == null))
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
			for (int i = 0; i < sources.Count; i++)
			{
				sources[i].Variation.VarAudio.outputAudioMixerGroup = mixerGroup;
			}
		}
	}

	public static void UnpauseSoundGroup(string sType)
	{
		if (GrabGroup(sType) == null)
		{
			return;
		}
		List<AudioInfo> sources = Instance.AudioSourcesBySoundType[sType].Sources;
		for (int i = 0; i < sources.Count; i++)
		{
			SoundGroupVariation variation = sources[i].Variation;
			if (AudioUtil.IsAudioPaused(variation.VarAudio))
			{
				variation.VarAudio.Play();
				if (variation.VariationUpdater != null)
				{
					variation.VariationUpdater.enabled = true;
					variation.VariationUpdater.Unpause();
				}
			}
		}
	}

	public static void FadeSoundGroupToVolume(string sType, float newVolume, float fadeTime, Action completionCallback = null, bool willStopAfterFade = false, bool willResetVolumeAfterFade = false)
	{
		if (newVolume < 0f || newVolume > 1f)
		{
			Debug.LogError("Illegal volume passed to FadeSoundGroupToVolume: '" + newVolume + "'. Legal volumes are between 0 and 1");
			return;
		}
		if (fadeTime <= 0.1f)
		{
			SetGroupVolume(sType, newVolume);
			completionCallback?.Invoke();
			if (willStopAfterFade)
			{
				StopAllOfSound(sType);
			}
			return;
		}
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (masterAudioGroup == null)
		{
			return;
		}
		if (newVolume < 0f || newVolume > 1f)
		{
			Debug.Log("Cannot fade Sound Group '" + sType + "'. Invalid volume specified. Volume should be between 0 and 1.");
			return;
		}
		GroupFadeInfo groupFadeInfo = Instance.GroupFades.Find((GroupFadeInfo obj) => obj.NameOfGroup == sType);
		if (groupFadeInfo != null)
		{
			groupFadeInfo.IsActive = false;
		}
		GroupFadeInfo groupFadeInfo2 = new GroupFadeInfo
		{
			NameOfGroup = sType,
			ActingGroup = masterAudioGroup,
			StartTime = AudioUtil.Time,
			CompletionTime = AudioUtil.Time + fadeTime,
			StartVolume = masterAudioGroup.groupMasterVolume,
			TargetVolume = newVolume,
			WillStopGroupAfterFade = willStopAfterFade,
			WillResetVolumeAfterFade = willResetVolumeAfterFade
		};
		if (completionCallback != null)
		{
			groupFadeInfo2.completionAction = completionCallback;
		}
		Instance.GroupFades.Add(groupFadeInfo2);
	}

	public static void GlideSoundGroupByPitch(string sType, float pitchAddition, float glideTime, Action completionCallback = null)
	{
		if (pitchAddition < -3f || pitchAddition > 3f)
		{
			Debug.LogError("Illegal pitch passed to GlideSoundGroupByPitch: '" + pitchAddition + "'. Legal pitches are between -3 and 3");
			return;
		}
		if (pitchAddition == 0f)
		{
			completionCallback?.Invoke();
			return;
		}
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (masterAudioGroup == null)
		{
			return;
		}
		GroupPitchGlideInfo groupPitchGlideInfo = Instance.GroupPitchGlides.Find((GroupPitchGlideInfo obj) => obj.NameOfGroup == sType);
		if (groupPitchGlideInfo != null)
		{
			groupPitchGlideInfo.IsActive = false;
			if (groupPitchGlideInfo.completionAction != null)
			{
				groupPitchGlideInfo.completionAction();
			}
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		if (glideTime <= 0.1f)
		{
			for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
			{
				audioGroupInfo.Sources[i].Variation.GlideByPitch(pitchAddition, 0f);
			}
			completionCallback?.Invoke();
			return;
		}
		List<SoundGroupVariation> list = new List<SoundGroupVariation>(audioGroupInfo.Sources.Count);
		for (int j = 0; j < audioGroupInfo.Sources.Count; j++)
		{
			SoundGroupVariation variation = audioGroupInfo.Sources[j].Variation;
			if (variation.IsPlaying)
			{
				if (variation.curPitchMode == SoundGroupVariation.PitchMode.Gliding)
				{
					variation.VariationUpdater.StopPitchGliding();
				}
				variation.GlideByPitch(pitchAddition, glideTime);
				list.Add(variation);
			}
		}
		if (list.Count == 0 || completionCallback == null)
		{
			completionCallback?.Invoke();
			return;
		}
		GroupPitchGlideInfo groupPitchGlideInfo2 = new GroupPitchGlideInfo
		{
			NameOfGroup = sType,
			ActingGroup = masterAudioGroup,
			CompletionTime = AudioUtil.Time + glideTime,
			GlidingVariations = list
		};
		if (completionCallback != null)
		{
			groupPitchGlideInfo2.completionAction = completionCallback;
		}
		Instance.GroupPitchGlides.Add(groupPitchGlideInfo2);
	}

	public static void DeleteSoundGroup(string sType)
	{
		if (SafeInstance == null)
		{
			return;
		}
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (masterAudioGroup == null)
		{
			return;
		}
		StopAllOfSound(sType);
		Transform transform = masterAudioGroup.transform;
		MasterAudio instance = Instance;
		if (instance.duckingBySoundType.ContainsKey(sType))
		{
			instance.duckingBySoundType.Remove(sType);
		}
		Instance._randomizer.Remove(sType);
		Instance._randomizerLeftovers.Remove(sType);
		Instance._randomizerOrigin.Remove(sType);
		Instance._clipsPlayedBySoundTypeOldestFirst.Remove(sType);
		RemoveRuntimeGroupInfo(sType);
		Instance.LastTimeSoundGroupPlayed.Remove(sType);
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			AudioSource component = child.GetComponent<AudioSource>();
			SoundGroupVariation component2 = child.GetComponent<SoundGroupVariation>();
			switch (component2.audLocation)
			{
			case AudioLocation.ResourceFile:
				AudioResourceOptimizer.DeleteAudioSourceFromList(AudioResourceOptimizer.GetLocalizedFileName(component2.useLocalization, component2.resourceFileName), component);
				break;
			case AudioLocation.FileOnInternet:
				AudioResourceOptimizer.DeleteAudioSourceFromList(component2.internetFileUrl, component);
				break;
			}
		}
		transform.parent = null;
		UnityEngine.Object.Destroy(transform.gameObject);
		RescanGroupsNow();
	}

	public static Transform CreateSoundGroup(DynamicSoundGroup aGroup, string creatorObjectName, bool errorOnExisting = true)
	{
		if (!SceneHasMasterAudio)
		{
			return null;
		}
		if (!SoundsReady)
		{
			Debug.LogError("MasterAudio not finished initializing sounds. Cannot create new group yet.");
			return null;
		}
		string text = aGroup.transform.name;
		MasterAudio instance = Instance;
		if (Instance.Trans.GetChildTransform(text) != null)
		{
			if (errorOnExisting)
			{
				Debug.LogError("Cannot add a new Sound Group named '" + text + "' because there is already a Sound Group of that name.");
			}
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(instance.soundGroupTemplate.gameObject, instance.Trans.position, Quaternion.identity);
		Transform transform = gameObject.transform;
		transform.name = UtilStrings.TrimSpace(text);
		transform.parent = Instance.Trans;
		transform.gameObject.layer = Instance.gameObject.layer;
		for (int i = 0; i < aGroup.groupVariations.Count; i++)
		{
			DynamicGroupVariation dynamicGroupVariation = aGroup.groupVariations[i];
			for (int j = 0; j < dynamicGroupVariation.weight; j++)
			{
				GameObject obj = UnityEngine.Object.Instantiate(dynamicGroupVariation.gameObject, transform.position, Quaternion.identity);
				obj.transform.parent = transform;
				obj.transform.gameObject.layer = transform.gameObject.layer;
				UnityEngine.Object.Destroy(obj.GetComponent<DynamicGroupVariation>());
				obj.AddComponent<SoundGroupVariation>();
				SoundGroupVariation component = obj.GetComponent<SoundGroupVariation>();
				string text2 = component.name;
				int num = text2.IndexOf("(Clone)");
				if (num >= 0)
				{
					text2 = text2.Substring(0, num);
				}
				AudioSource component2 = dynamicGroupVariation.GetComponent<AudioSource>();
				switch (dynamicGroupVariation.audLocation)
				{
				case AudioLocation.Clip:
				{
					AudioClip clip = component2.clip;
					component.VarAudio.clip = clip;
					break;
				}
				case AudioLocation.ResourceFile:
					AudioResourceOptimizer.AddTargetForClip(AudioResourceOptimizer.GetLocalizedFileName(dynamicGroupVariation.useLocalization, dynamicGroupVariation.resourceFileName), component.VarAudio);
					component.resourceFileName = dynamicGroupVariation.resourceFileName;
					component.useLocalization = dynamicGroupVariation.useLocalization;
					break;
				case AudioLocation.FileOnInternet:
					AudioResourceOptimizer.AddTargetForClip(dynamicGroupVariation.internetFileUrl, component.VarAudio);
					component.internetFileUrl = dynamicGroupVariation.internetFileUrl;
					break;
				}
				component.audLocation = dynamicGroupVariation.audLocation;
				component.original_pitch = component2.pitch;
				component.transform.name = text2;
				component.isExpanded = dynamicGroupVariation.isExpanded;
				component.probabilityToPlay = dynamicGroupVariation.probabilityToPlay;
				component.useRandomPitch = dynamicGroupVariation.useRandomPitch;
				component.randomPitchMode = dynamicGroupVariation.randomPitchMode;
				component.randomPitchMin = dynamicGroupVariation.randomPitchMin;
				component.randomPitchMax = dynamicGroupVariation.randomPitchMax;
				component.useRandomVolume = dynamicGroupVariation.useRandomVolume;
				component.randomVolumeMode = dynamicGroupVariation.randomVolumeMode;
				component.randomVolumeMin = dynamicGroupVariation.randomVolumeMin;
				component.randomVolumeMax = dynamicGroupVariation.randomVolumeMax;
				component.useCustomLooping = dynamicGroupVariation.useCustomLooping;
				component.minCustomLoops = dynamicGroupVariation.minCustomLoops;
				component.maxCustomLoops = dynamicGroupVariation.maxCustomLoops;
				component.useFades = dynamicGroupVariation.useFades;
				component.fadeInTime = dynamicGroupVariation.fadeInTime;
				component.fadeOutTime = dynamicGroupVariation.fadeOutTime;
				component.useIntroSilence = dynamicGroupVariation.useIntroSilence;
				component.introSilenceMin = dynamicGroupVariation.introSilenceMin;
				component.introSilenceMax = dynamicGroupVariation.introSilenceMax;
				component.useRandomStartTime = dynamicGroupVariation.useRandomStartTime;
				component.randomStartMinPercent = dynamicGroupVariation.randomStartMinPercent;
				component.randomStartMaxPercent = dynamicGroupVariation.randomStartMaxPercent;
				component.randomEndPercent = dynamicGroupVariation.randomEndPercent;
				if (component.LowPassFilter != null && !component.LowPassFilter.enabled)
				{
					UnityEngine.Object.Destroy(component.LowPassFilter);
				}
				if (component.HighPassFilter != null && !component.HighPassFilter.enabled)
				{
					UnityEngine.Object.Destroy(component.HighPassFilter);
				}
				if (component.DistortionFilter != null && !component.DistortionFilter.enabled)
				{
					UnityEngine.Object.Destroy(component.DistortionFilter);
				}
				if (component.ChorusFilter != null && !component.ChorusFilter.enabled)
				{
					UnityEngine.Object.Destroy(component.ChorusFilter);
				}
				if (component.EchoFilter != null && !component.EchoFilter.enabled)
				{
					UnityEngine.Object.Destroy(component.EchoFilter);
				}
				if (component.ReverbFilter != null && !component.ReverbFilter.enabled)
				{
					UnityEngine.Object.Destroy(component.ReverbFilter);
				}
			}
		}
		MasterAudioGroup component3 = gameObject.GetComponent<MasterAudioGroup>();
		component3.retriggerPercentage = aGroup.retriggerPercentage;
		float? groupVolume = PersistentAudioSettings.GetGroupVolume(aGroup.name);
		component3.OriginalVolume = aGroup.groupMasterVolume;
		if (groupVolume.HasValue)
		{
			component3.groupMasterVolume = groupVolume.Value;
		}
		else
		{
			component3.groupMasterVolume = aGroup.groupMasterVolume;
		}
		component3.limitMode = aGroup.limitMode;
		component3.limitPerXFrames = aGroup.limitPerXFrames;
		component3.minimumTimeBetween = aGroup.minimumTimeBetween;
		component3.limitPolyphony = aGroup.limitPolyphony;
		component3.voiceLimitCount = aGroup.voiceLimitCount;
		component3.curVariationSequence = aGroup.curVariationSequence;
		component3.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
		component3.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
		component3.curVariationMode = aGroup.curVariationMode;
		component3.useNoRepeatRefill = aGroup.useNoRepeatRefill;
		component3.useDialogFadeOut = aGroup.useDialogFadeOut;
		component3.dialogFadeOutTime = aGroup.dialogFadeOutTime;
		component3.isUsingOcclusion = aGroup.isUsingOcclusion;
		component3.willOcclusionOverrideRaycastOffset = aGroup.willOcclusionOverrideRaycastOffset;
		component3.occlusionRayCastOffset = aGroup.occlusionRayCastOffset;
		component3.willOcclusionOverrideFrequencies = aGroup.willOcclusionOverrideFrequencies;
		component3.occlusionMaxCutoffFreq = aGroup.occlusionMaxCutoffFreq;
		component3.occlusionMinCutoffFreq = aGroup.occlusionMinCutoffFreq;
		component3.chainLoopDelayMin = aGroup.chainLoopDelayMin;
		component3.chainLoopDelayMax = aGroup.chainLoopDelayMax;
		component3.chainLoopMode = aGroup.chainLoopMode;
		component3.chainLoopNumLoops = aGroup.chainLoopNumLoops;
		component3.expandLinkedGroups = aGroup.expandLinkedGroups;
		component3.childSoundGroups = aGroup.childSoundGroups;
		component3.endLinkedGroups = aGroup.endLinkedGroups;
		component3.linkedStartGroupSelectionType = aGroup.linkedStartGroupSelectionType;
		component3.linkedStopGroupSelectionType = aGroup.linkedStopGroupSelectionType;
		component3.soundPlayedEventActive = aGroup.soundPlayedEventActive;
		component3.soundPlayedCustomEvent = aGroup.soundPlayedCustomEvent;
		component3.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
		component3.despawnFadeTime = aGroup.despawnFadeTime;
		component3.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
		component3.logSound = aGroup.logSound;
		component3.alwaysHighestPriority = aGroup.alwaysHighestPriority;
		component3.spatialBlendType = aGroup.spatialBlendType;
		component3.spatialBlend = aGroup.spatialBlend;
		List<AudioInfo> list = new List<AudioInfo>();
		List<int> list2 = new List<int>();
		for (int k = 0; k < gameObject.transform.childCount; k++)
		{
			list2.Add(k);
			Transform child = gameObject.transform.GetChild(k);
			AudioSource component4 = child.GetComponent<AudioSource>();
			SoundGroupVariation component = child.GetComponent<SoundGroupVariation>();
			list.Add(new AudioInfo(component, component4, component4.volume));
			component.DisableUpdater();
		}
		AddRuntimeGroupInfo(text, new AudioGroupInfo(list, component3));
		if (component3.curVariationSequence == MasterAudioGroup.VariationSequence.Randomized)
		{
			ArrayListUtil.SortIntArray(ref list2);
		}
		Instance._randomizer.Add(text, list2);
		List<int> list3 = new List<int>(list2.Count);
		list3.AddRange(list2);
		Instance._randomizerOrigin.Add(text, list3);
		Instance._randomizerLeftovers.Add(text, new List<int>(list2.Count));
		Instance._randomizerLeftovers[text].AddRange(list2);
		Instance._clipsPlayedBySoundTypeOldestFirst.Add(text, new List<int>(list2.Count));
		RescanGroupsNow();
		if (string.IsNullOrEmpty(aGroup.busName))
		{
			return transform;
		}
		component3.busIndex = GetBusIndex(aGroup.busName, alertMissing: true);
		if (component3.BusForGroup != null && component3.BusForGroup.isMuted)
		{
			MuteGroup(component3.name, shouldCheckMuteStatus: false);
		}
		else if (Instance.mixerMuted)
		{
			MuteGroup(component3.name, shouldCheckMuteStatus: false);
		}
		return transform;
	}

	public static float GetGroupVolume(string sType)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (masterAudioGroup == null)
		{
			return 0f;
		}
		return masterAudioGroup.groupMasterVolume;
	}

	public static void SetGroupVolume(string sType, float volumeLevel)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType, Application.isPlaying);
		if (masterAudioGroup == null || AppIsShuttingDown)
		{
			return;
		}
		masterAudioGroup.groupMasterVolume = volumeLevel;
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		float busVolume = GetBusVolume(masterAudioGroup);
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			AudioInfo audioInfo = audioGroupInfo.Sources[i];
			AudioSource source = audioInfo.Source;
			if (!(source == null))
			{
				float volume = ((audioInfo.Variation.randomVolumeMode != 0) ? (audioInfo.OriginalVolume * audioInfo.LastPercentageVolume * masterAudioGroup.groupMasterVolume * busVolume * Instance._masterAudioVolume) : (audioInfo.OriginalVolume * audioInfo.LastPercentageVolume * masterAudioGroup.groupMasterVolume * busVolume * Instance._masterAudioVolume + audioInfo.LastRandomVolume));
				source.volume = volume;
			}
		}
	}

	public static void MuteGroup(string sType, bool shouldCheckMuteStatus = true)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (!(masterAudioGroup == null))
		{
			Instance.SoloedGroups.Remove(masterAudioGroup);
			masterAudioGroup.isSoloed = false;
			SetGroupMuteStatus(masterAudioGroup, sType, isMute: true);
			if (shouldCheckMuteStatus)
			{
				SilenceOrUnsilenceGroupsFromSoloChange();
			}
		}
	}

	public static void UnmuteGroup(string sType, bool shouldCheckMuteStatus = true)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (!(masterAudioGroup == null))
		{
			SetGroupMuteStatus(masterAudioGroup, sType, isMute: false);
			if (shouldCheckMuteStatus)
			{
				SilenceOrUnsilenceGroupsFromSoloChange();
			}
		}
	}

	private static void AddRuntimeGroupInfo(string groupName, AudioGroupInfo groupInfo)
	{
		Instance.AudioSourcesBySoundType.Add(groupName, groupInfo);
		List<AudioSource> list = new List<AudioSource>(groupInfo.Sources.Count);
		for (int i = 0; i < groupInfo.Sources.Count; i++)
		{
			list.Add(groupInfo.Sources[i].Source);
		}
		TrackRuntimeAudioSources(list);
	}

	private static void FireAudioSourcesNumberChangedEvent()
	{
		if (NumberOfAudioSourcesChanged != null)
		{
			NumberOfAudioSourcesChanged();
		}
	}

	public static void TrackRuntimeAudioSources(List<AudioSource> sources)
	{
		bool flag = false;
		for (int i = 0; i < sources.Count; i++)
		{
			AudioSource item = sources[i];
			if (!Instance.AllAudioSources.Contains(item))
			{
				Instance.AllAudioSources.Add(item);
				flag = true;
			}
		}
		if (flag)
		{
			FireAudioSourcesNumberChangedEvent();
		}
	}

	public static void StopTrackingRuntimeAudioSources(List<AudioSource> sources)
	{
		if (AppIsShuttingDown || SafeInstance == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < sources.Count; i++)
		{
			AudioSource item = sources[i];
			if (Instance.AllAudioSources.Contains(item))
			{
				Instance.AllAudioSources.Remove(item);
				flag = true;
			}
		}
		if (flag)
		{
			FireAudioSourcesNumberChangedEvent();
		}
	}

	private static void RemoveRuntimeGroupInfo(string groupName)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(groupName);
		if (masterAudioGroup != null)
		{
			List<AudioSource> list = new List<AudioSource>(masterAudioGroup.groupVariations.Count);
			for (int i = 0; i < masterAudioGroup.groupVariations.Count; i++)
			{
				list.Add(masterAudioGroup.groupVariations[i].VarAudio);
			}
			StopTrackingRuntimeAudioSources(list);
		}
		Instance.AudioSourcesBySoundType.Remove(groupName);
	}

	private static void RescanChildren(MasterAudioGroup group)
	{
		List<SoundGroupVariation> list = new List<SoundGroupVariation>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < group.transform.childCount; i++)
		{
			Transform child = group.transform.GetChild(i);
			if (!list2.Contains(child.name))
			{
				list2.Add(child.name);
				SoundGroupVariation component = child.GetComponent<SoundGroupVariation>();
				list.Add(component);
			}
		}
		group.groupVariations = list;
	}

	private static void SetGroupMuteStatus(MasterAudioGroup aGroup, string sType, bool isMute)
	{
		aGroup.isMuted = isMute;
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
		{
			audioGroupInfo.Sources[i].Source.mute = isMute;
		}
	}

	public static void SoloGroup(string sType, bool shouldCheckMuteStatus = true)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (!(masterAudioGroup == null))
		{
			masterAudioGroup.isMuted = false;
			masterAudioGroup.isSoloed = true;
			Instance.SoloedGroups.Add(masterAudioGroup);
			SetGroupMuteStatus(masterAudioGroup, sType, isMute: false);
			if (shouldCheckMuteStatus)
			{
				SilenceOrUnsilenceGroupsFromSoloChange();
			}
		}
	}

	public static void SilenceOrUnsilenceGroupsFromSoloChange()
	{
		if (Instance.SoloedGroups.Count > 0)
		{
			SilenceNonSoloedGroups();
		}
		else
		{
			UnsilenceNonSoloedGroups();
		}
	}

	private static void UnsilenceNonSoloedGroups()
	{
		foreach (AudioGroupInfo value in Instance.AudioSourcesBySoundType.Values)
		{
			if (!value.Group.isMuted)
			{
				UnsilenceGroup(value.Group.GameObjectName);
			}
		}
	}

	private static void UnsilenceGroup(string sType)
	{
		if (Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
			for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
			{
				audioGroupInfo.Sources[i].Source.mute = false;
			}
		}
	}

	private static void SilenceNonSoloedGroups()
	{
		foreach (AudioGroupInfo value in Instance.AudioSourcesBySoundType.Values)
		{
			if (!value.Group.isSoloed && !value.Group.isMuted)
			{
				SilenceGroup(value.Group.GameObjectName);
			}
		}
	}

	private static void SilenceGroup(string sType)
	{
		if (Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
			for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
			{
				audioGroupInfo.Sources[i].Source.mute = true;
			}
		}
	}

	public static void UnsoloGroup(string sType, bool shouldCheckMuteStatus = true)
	{
		MasterAudioGroup masterAudioGroup = GrabGroup(sType);
		if (!(masterAudioGroup == null))
		{
			masterAudioGroup.isSoloed = false;
			Instance.SoloedGroups.Remove(masterAudioGroup);
			if (shouldCheckMuteStatus)
			{
				SilenceOrUnsilenceGroupsFromSoloChange();
			}
		}
	}

	public static MasterAudioGroup GrabGroup(string sType, bool logIfMissing = true)
	{
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			if (logIfMissing)
			{
				Debug.LogError("Could not grab Sound Group '" + sType + "' because it does not exist in this scene.");
			}
			return null;
		}
		AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[sType];
		if (audioGroupInfo.Group == null)
		{
			Transform childTransform = Instance.Trans.GetChildTransform(sType);
			if (!(childTransform != null))
			{
				return null;
			}
			MasterAudioGroup component = childTransform.GetComponent<MasterAudioGroup>();
			audioGroupInfo.Group = component;
		}
		MasterAudioGroup group = audioGroupInfo.Group;
		if (group.groupVariations.Count == 0)
		{
			RescanChildren(group);
		}
		return group;
	}

	public static int VoicesForGroup(string sType)
	{
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			return -1;
		}
		return Instance.AudioSourcesBySoundType[sType].Sources.Count;
	}

	public static Transform FindGroupTransform(string sType)
	{
		if (SafeInstance != null)
		{
			Transform childTransform = Instance.Trans.GetChildTransform(sType);
			if (childTransform != null)
			{
				return childTransform;
			}
		}
		DynamicSoundGroupCreator[] array = UnityEngine.Object.FindObjectsOfType<DynamicSoundGroupCreator>();
		for (int i = 0; i < array.Count(); i++)
		{
			Transform childTransform = array[i].transform.GetChildTransform(sType);
			if (childTransform != null)
			{
				return childTransform;
			}
		}
		return null;
	}

	public static List<AudioInfo> GetAllVariationsOfGroup(string sType, bool logIfMissing = true)
	{
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			if (logIfMissing)
			{
				Debug.LogError("Could not grab Sound Group '" + sType + "' because it does not exist in this scene.");
			}
			return null;
		}
		return Instance.AudioSourcesBySoundType[sType].Sources;
	}

	public static AudioGroupInfo GetGroupInfo(string sType)
	{
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			return null;
		}
		return Instance.AudioSourcesBySoundType[sType];
	}

	public static void SubscribeToLastVariationPlayed(string sType, Action finishedCallback)
	{
		if (!Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Debug.LogError("Could not grab Sound Group '" + sType + "' because it does not exist in this scene.");
		}
		else
		{
			Instance.AudioSourcesBySoundType[sType].Group.SubscribeToLastVariationFinishedPlay(finishedCallback);
		}
	}

	public static void UnsubscribeFromLastVariationPlayed(string sType)
	{
		if (Instance.AudioSourcesBySoundType.ContainsKey(sType))
		{
			Instance.AudioSourcesBySoundType[sType].Group.UnsubscribeFromLastVariationFinishedPlay();
		}
	}

	public void SetSpatialBlendForMixer()
	{
		foreach (string key in AudioSourcesBySoundType.Keys)
		{
			SetGroupSpatialBlend(key);
		}
	}

	public static void PauseMixer()
	{
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			PauseSoundGroup(Instance.AudioSourcesBySoundType[key].Group.GameObjectName);
		}
	}

	public static void UnpauseMixer()
	{
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			UnpauseSoundGroup(Instance.AudioSourcesBySoundType[key].Group.GameObjectName);
		}
	}

	public static void StopMixer()
	{
		Instance.VariationsStartedDuringMultiStop.Clear();
		Instance._isStoppingMultiple = true;
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			StopAllOfSound(Instance.AudioSourcesBySoundType[key].Group.GameObjectName);
		}
		Instance._isStoppingMultiple = false;
	}

	public static void UnsubscribeFromAllVariations()
	{
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			List<AudioInfo> sources = Instance.AudioSourcesBySoundType[key].Sources;
			for (int i = 0; i < sources.Count; i++)
			{
				sources[i].Variation.ClearSubscribers();
			}
		}
	}

	public static void StopEverything()
	{
		StopMixer();
		StopAllPlaylists();
	}

	public static void PauseEverything()
	{
		PauseMixer();
		PauseAllPlaylists();
	}

	public static void UnpauseEverything()
	{
		UnpauseMixer();
		UnpauseAllPlaylists();
	}

	public static void MuteEverything()
	{
		MixerMuted = true;
		MuteAllPlaylists();
	}

	public static void UnmuteEverything()
	{
		MixerMuted = false;
		UnmuteAllPlaylists();
	}

	public static List<string> ListOfAudioClipsInGroupsEditTime()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < Instance.transform.childCount; i++)
		{
			MasterAudioGroup component = Instance.transform.GetChild(i).GetComponent<MasterAudioGroup>();
			for (int j = 0; j < component.transform.childCount; j++)
			{
				SoundGroupVariation component2 = component.transform.GetChild(j).GetComponent<SoundGroupVariation>();
				string text = string.Empty;
				switch (component2.audLocation)
				{
				case AudioLocation.Clip:
				{
					AudioClip clip = component2.VarAudio.clip;
					if (clip != null)
					{
						text = clip.name;
					}
					break;
				}
				case AudioLocation.ResourceFile:
					text = component2.resourceFileName;
					break;
				case AudioLocation.FileOnInternet:
					text = component2.internetFileUrl;
					break;
				}
				if (!string.IsNullOrEmpty(text) && !list.Contains(text))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	private static int GetBusIndex(string busName, bool alertMissing)
	{
		if (!SceneHasMasterAudio)
		{
			return -1;
		}
		for (int i = 0; i < GroupBuses.Count; i++)
		{
			if (GroupBuses[i].busName == busName)
			{
				return i + 2;
			}
		}
		if (alertMissing)
		{
			LogWarning("Could not find bus '" + busName + "'.");
		}
		return -1;
	}

	private static GroupBus GetBusByIndex(int busIndex)
	{
		if (busIndex < 2)
		{
			return null;
		}
		return GroupBuses[busIndex - 2];
	}

	public static void ChangeBusPitch(string busName, float pitch)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				ChangeVariationPitch(group.GameObjectName, changeAllVariations: true, string.Empty, pitch);
			}
		}
	}

	public static void MuteBus(string busName)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		GroupBus groupBus = GrabBusByName(busName);
		groupBus.isMuted = true;
		if (groupBus.isSoloed)
		{
			UnsoloBus(busName);
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				MuteGroup(group.GameObjectName, shouldCheckMuteStatus: false);
			}
		}
		if (Application.isPlaying)
		{
			SilenceOrUnsilenceGroupsFromSoloChange();
		}
	}

	public static void UnmuteBus(string busName, bool shouldCheckMuteStatus = true)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		GrabBusByName(busName).isMuted = false;
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				UnmuteGroup(group.GameObjectName, shouldCheckMuteStatus: false);
			}
		}
		if (shouldCheckMuteStatus)
		{
			SilenceOrUnsilenceGroupsFromSoloChange();
		}
	}

	public static void ToggleMuteBus(string busName)
	{
		if (GetBusIndex(busName, alertMissing: true) >= 0)
		{
			if (GrabBusByName(busName).isMuted)
			{
				UnmuteBus(busName);
			}
			else
			{
				MuteBus(busName);
			}
		}
	}

	public static void PauseBus(string busName)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				PauseSoundGroup(group.GameObjectName);
			}
		}
	}

	public static void SoloBus(string busName)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		GroupBus groupBus = GrabBusByName(busName);
		groupBus.isSoloed = true;
		if (groupBus.isMuted)
		{
			UnmuteBus(busName);
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				SoloGroup(group.GameObjectName, shouldCheckMuteStatus: false);
			}
		}
		if (Application.isPlaying)
		{
			SilenceOrUnsilenceGroupsFromSoloChange();
		}
	}

	public static void UnsoloBus(string busName, bool shouldCheckMuteStatus = true)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		GrabBusByName(busName).isSoloed = false;
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				UnsoloGroup(group.GameObjectName, shouldCheckMuteStatus: false);
			}
		}
		if (shouldCheckMuteStatus)
		{
			SilenceOrUnsilenceGroupsFromSoloChange();
		}
	}

	public static void RouteBusToUnityMixerGroup(string busName, AudioMixerGroup mixerGroup)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				RouteGroupToUnityMixerGroup(group.name, mixerGroup);
			}
		}
	}

	private static void StopOldestSoundOnBus(GroupBus bus)
	{
		int busIndex = GetBusIndex(bus.busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		SoundGroupVariation soundGroupVariation = null;
		float num = -1f;
		while (enumerator.MoveNext())
		{
			AudioGroupInfo value = enumerator.Current.Value;
			MasterAudioGroup group = value.Group;
			if (group.busIndex != busIndex || group.ActiveVoices == 0)
			{
				continue;
			}
			for (int i = 0; i < value.Sources.Count; i++)
			{
				SoundGroupVariation variation = value.Sources[i].Variation;
				if (variation.PlaySoundParm.IsPlaying)
				{
					if (variation.curFadeMode == SoundGroupVariation.FadeMode.FadeOutEarly)
					{
						variation.Stop();
					}
					else if (soundGroupVariation == null)
					{
						soundGroupVariation = variation;
						num = variation.LastTimePlayed;
					}
					else if (variation.LastTimePlayed < num)
					{
						soundGroupVariation = variation;
						num = variation.LastTimePlayed;
					}
				}
			}
		}
		if (soundGroupVariation != null)
		{
			soundGroupVariation.FadeOutNow(Instance.stopOldestBusFadeTime);
		}
	}

	public static void StopBus(string busName)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Instance.VariationsStartedDuringMultiStop.Clear();
		Instance._isStoppingMultiple = true;
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				StopAllOfSound(group.GameObjectName);
			}
		}
		Instance._isStoppingMultiple = false;
	}

	public static void UnpauseBus(string busName)
	{
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				UnpauseSoundGroup(group.GameObjectName);
			}
		}
	}

	public static bool CreateBus(string busName, bool errorOnExisting = true, bool isTemporary = false)
	{
		if (GroupBuses.FindAll((GroupBus obj) => obj.busName == busName).Count > 0)
		{
			if (errorOnExisting)
			{
				LogError("You already have a bus named '" + busName + "'. Not creating a second one.");
			}
			return false;
		}
		GroupBus item = new GroupBus
		{
			busName = busName,
			isTemporary = isTemporary
		};
		float? busVolume = PersistentAudioSettings.GetBusVolume(busName);
		GroupBuses.Add(item);
		if (busVolume.HasValue)
		{
			SetBusVolumeByName(busName, busVolume.Value);
		}
		return true;
	}

	public static void DeleteBusByName(string busName)
	{
		int busIndex = GetBusIndex(busName, alertMissing: false);
		if (busIndex > 0)
		{
			DeleteBusByIndex(busIndex);
		}
	}

	public static void DeleteBusByIndex(int busIndex)
	{
		int index = busIndex - 2;
		if (Application.isPlaying)
		{
			GroupBus groupBus = GroupBuses[index];
			if (groupBus.isSoloed)
			{
				UnsoloBus(groupBus.busName, shouldCheckMuteStatus: false);
			}
			else if (groupBus.isMuted)
			{
				UnmuteBus(groupBus.busName, shouldCheckMuteStatus: false);
			}
		}
		GroupBuses.RemoveAt(index);
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AudioGroupInfo value = enumerator.Current.Value;
			MasterAudioGroup group = value.Group;
			if (group.busIndex == -1)
			{
				continue;
			}
			if (group.busIndex == busIndex)
			{
				group.busIndex = -1;
				RouteGroupToUnityMixerGroup(group.name, null);
				for (int i = 0; i < value.Sources.Count; i++)
				{
					value.Sources[i].Variation.SetSpatialBlend();
				}
				RecalculateGroupVolumes(value, null);
			}
			else if (group.busIndex > busIndex)
			{
				group.busIndex--;
			}
		}
	}

	public static float GetBusVolume(MasterAudioGroup maGroup)
	{
		float result = 1f;
		if (maGroup.busIndex >= 2)
		{
			result = GroupBuses[maGroup.busIndex - 2].volume;
		}
		return result;
	}

	public static void FadeBusToVolume(string busName, float newVolume, float fadeTime, Action completionCallback = null, bool willStopAfterFade = false, bool willResetVolumeAfterFade = false)
	{
		if (newVolume < 0f || newVolume > 1f)
		{
			Debug.LogError("Illegal volume passed to FadeBusToVolume: '" + newVolume + "'. Legal volumes are between 0 and 1");
			return;
		}
		if (fadeTime <= 0.1f)
		{
			SetBusVolumeByName(busName, newVolume);
			completionCallback?.Invoke();
			if (willStopAfterFade)
			{
				StopBus(busName);
			}
			return;
		}
		GroupBus groupBus = GrabBusByName(busName);
		if (groupBus == null)
		{
			Debug.Log("Could not find bus '" + busName + "' to fade it.");
			return;
		}
		BusFadeInfo busFadeInfo = Instance.BusFades.Find((BusFadeInfo obj) => obj.NameOfBus == busName);
		if (busFadeInfo != null)
		{
			busFadeInfo.IsActive = false;
		}
		BusFadeInfo busFadeInfo2 = new BusFadeInfo
		{
			NameOfBus = busName,
			ActingBus = groupBus,
			StartVolume = groupBus.volume,
			TargetVolume = newVolume,
			StartTime = AudioUtil.Time,
			CompletionTime = AudioUtil.Time + fadeTime,
			WillStopGroupAfterFade = willStopAfterFade,
			WillResetVolumeAfterFade = willResetVolumeAfterFade
		};
		if (completionCallback != null)
		{
			busFadeInfo2.completionAction = completionCallback;
		}
		Instance.BusFades.Add(busFadeInfo2);
	}

	public static void GlideBusByPitch(string busName, float pitchAddition, float glideTime, Action completionCallback = null)
	{
		if (pitchAddition < -3f || pitchAddition > 3f)
		{
			Debug.LogError("Illegal pitch passed to GlideBusByPitch: '" + pitchAddition + "'. Legal pitches are between -3 and 3");
			return;
		}
		if (pitchAddition == 0f)
		{
			completionCallback?.Invoke();
			return;
		}
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		if (glideTime <= 0.1f)
		{
			while (enumerator.MoveNext())
			{
				AudioGroupInfo value = enumerator.Current.Value;
				AudioGroupInfo audioGroupInfo = Instance.AudioSourcesBySoundType[value.Group.name];
				if (audioGroupInfo.Group.busIndex != busIndex)
				{
					continue;
				}
				for (int i = 0; i < audioGroupInfo.Sources.Count; i++)
				{
					SoundGroupVariation variation = audioGroupInfo.Sources[i].Variation;
					if (variation.IsPlaying)
					{
						if (variation.curPitchMode == SoundGroupVariation.PitchMode.Gliding)
						{
							variation.VariationUpdater.StopPitchGliding();
						}
						variation.GlideByPitch(pitchAddition, 0f);
					}
				}
			}
			completionCallback?.Invoke();
			return;
		}
		BusPitchGlideInfo busPitchGlideInfo = Instance.BusPitchGlides.Find((BusPitchGlideInfo obj) => obj.NameOfBus == busName);
		if (busPitchGlideInfo != null)
		{
			busPitchGlideInfo.IsActive = false;
			if (busPitchGlideInfo.completionAction != null)
			{
				busPitchGlideInfo.completionAction();
				busPitchGlideInfo.completionAction = null;
			}
		}
		List<SoundGroupVariation> list = new List<SoundGroupVariation>(16);
		while (enumerator.MoveNext())
		{
			AudioGroupInfo value = enumerator.Current.Value;
			AudioGroupInfo audioGroupInfo2 = Instance.AudioSourcesBySoundType[value.Group.name];
			if (audioGroupInfo2.Group.busIndex != busIndex)
			{
				continue;
			}
			for (int j = 0; j < audioGroupInfo2.Sources.Count; j++)
			{
				SoundGroupVariation variation2 = audioGroupInfo2.Sources[j].Variation;
				if (variation2.IsPlaying)
				{
					if (variation2.curPitchMode == SoundGroupVariation.PitchMode.Gliding)
					{
						variation2.VariationUpdater.StopPitchGliding();
					}
					variation2.GlideByPitch(pitchAddition, glideTime);
					list.Add(variation2);
				}
			}
		}
		if (list.Count == 0)
		{
			completionCallback?.Invoke();
			return;
		}
		BusPitchGlideInfo busPitchGlideInfo2 = new BusPitchGlideInfo
		{
			NameOfBus = busName,
			CompletionTime = AudioUtil.Time + glideTime,
			GlidingVariations = list
		};
		if (completionCallback != null)
		{
			busPitchGlideInfo2.completionAction = completionCallback;
		}
		Instance.BusPitchGlides.Add(busPitchGlideInfo2);
	}

	public static void SetBusVolumeByName(string busName, float newVolume)
	{
		GroupBus groupBus = GrabBusByName(busName);
		if (groupBus == null)
		{
			Debug.LogError("bus '" + busName + "' not found!");
		}
		else
		{
			SetBusVolume(groupBus, newVolume);
		}
	}

	private static void RecalculateGroupVolumes(AudioGroupInfo aGroup, GroupBus bus)
	{
		GroupBus busByIndex = GetBusByIndex(aGroup.Group.busIndex);
		bool num = busByIndex != null && bus != null && busByIndex.busName == bus.busName;
		float num2 = 1f;
		if (num)
		{
			num2 = bus.volume;
		}
		else if (busByIndex != null)
		{
			num2 = busByIndex.volume;
		}
		for (int i = 0; i < aGroup.Sources.Count; i++)
		{
			AudioInfo audioInfo = aGroup.Sources[i];
			AudioSource source = audioInfo.Source;
			if (audioInfo.Variation.IsPlaying)
			{
				float num3 = aGroup.Group.groupMasterVolume * num2 * Instance._masterAudioVolume;
				float volume = audioInfo.OriginalVolume * audioInfo.LastPercentageVolume * num3 + audioInfo.LastRandomVolume;
				source.volume = volume;
				source.GetComponent<SoundGroupVariation>().SetGroupVolume = num3;
			}
		}
	}

	private static void SetBusVolume(GroupBus bus, float newVolume)
	{
		if (bus != null)
		{
			bus.volume = newVolume;
		}
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			RecalculateGroupVolumes(Instance.AudioSourcesBySoundType[key], bus);
		}
	}

	public static GroupBus GrabBusByName(string busName)
	{
		for (int i = 0; i < GroupBuses.Count; i++)
		{
			GroupBus groupBus = GroupBuses[i];
			if (groupBus.busName == busName)
			{
				return groupBus;
			}
		}
		return null;
	}

	public static void PauseBusOfTransform(Transform sourceTrans, string busName)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				PauseSoundGroupOfTransform(sourceTrans, group.GameObjectName);
			}
		}
	}

	public static void UnpauseBusOfTransform(Transform sourceTrans, string busName)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				UnpauseSoundGroupOfTransform(sourceTrans, group.GameObjectName);
			}
		}
	}

	public static void StopBusOfTransform(Transform sourceTrans, string busName)
	{
		if (!SceneHasMasterAudio)
		{
			return;
		}
		int busIndex = GetBusIndex(busName, alertMissing: true);
		if (busIndex < 0)
		{
			return;
		}
		Dictionary<string, AudioGroupInfo>.Enumerator enumerator = Instance.AudioSourcesBySoundType.GetEnumerator();
		Instance.VariationsStartedDuringMultiStop.Clear();
		Instance._isStoppingMultiple = true;
		while (enumerator.MoveNext())
		{
			MasterAudioGroup group = enumerator.Current.Value.Group;
			if (group.busIndex == busIndex)
			{
				StopSoundGroupOfTransform(sourceTrans, group.GameObjectName);
			}
		}
		Instance._isStoppingMultiple = false;
	}

	public static void AddSoundGroupToDuckList(string sType, float riseVolumeStart, float duckedVolCut, float unduckTime, bool isTemporary = false)
	{
		MasterAudio instance = Instance;
		if (!instance.duckingBySoundType.ContainsKey(sType))
		{
			DuckGroupInfo duckGroupInfo = new DuckGroupInfo
			{
				soundType = sType,
				riseVolStart = riseVolumeStart,
				duckedVolumeCut = duckedVolCut,
				unduckTime = unduckTime,
				isTemporary = isTemporary
			};
			instance.duckingBySoundType.Add(sType, duckGroupInfo);
			instance.musicDuckingSounds.Add(duckGroupInfo);
		}
	}

	public static void RemoveSoundGroupFromDuckList(string sType)
	{
		MasterAudio instance = Instance;
		if (instance.duckingBySoundType.ContainsKey(sType))
		{
			DuckGroupInfo item = instance.duckingBySoundType[sType];
			instance.musicDuckingSounds.Remove(item);
			instance.duckingBySoundType.Remove(sType);
		}
	}

	public static Playlist GrabPlaylist(string playlistName, bool logErrorIfNotFound = true)
	{
		if (playlistName == "[None]")
		{
			return null;
		}
		for (int i = 0; i < MusicPlaylists.Count; i++)
		{
			Playlist playlist = MusicPlaylists[i];
			if (playlist.playlistName == playlistName)
			{
				return playlist;
			}
		}
		if (logErrorIfNotFound)
		{
			Debug.LogError("Could not find Playlist '" + playlistName + "'.");
		}
		return null;
	}

	public static void ChangePlaylistPitch(string playlistName, float pitch, string songName = null)
	{
		Playlist playlist = GrabPlaylist(playlistName);
		if (playlist == null)
		{
			return;
		}
		for (int i = 0; i < playlist.MusicSettings.Count; i++)
		{
			MusicSetting musicSetting = playlist.MusicSettings[i];
			if (string.IsNullOrEmpty(songName) || !(musicSetting.alias != songName) || !(musicSetting.songName != songName))
			{
				musicSetting.pitch = pitch;
			}
		}
	}

	public static void MutePlaylist()
	{
		MutePlaylist("~only~");
	}

	public static void MutePlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "PausePlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		MutePlaylists(list);
	}

	public static void MuteAllPlaylists()
	{
		MutePlaylists(PlaylistController.Instances);
	}

	private static void MutePlaylists(List<PlaylistController> playlists)
	{
		if (playlists.Count == PlaylistController.Instances.Count)
		{
			PlaylistsMuted = true;
		}
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].MutePlaylist();
		}
	}

	public static void UnmutePlaylist()
	{
		UnmutePlaylist("~only~");
	}

	public static void UnmutePlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "PausePlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		UnmutePlaylists(list);
	}

	public static void UnmuteAllPlaylists()
	{
		UnmutePlaylists(PlaylistController.Instances);
	}

	private static void UnmutePlaylists(List<PlaylistController> playlists)
	{
		if (playlists.Count == PlaylistController.Instances.Count)
		{
			PlaylistsMuted = false;
		}
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].UnmutePlaylist();
		}
	}

	public static void ToggleMutePlaylist()
	{
		ToggleMutePlaylist("~only~");
	}

	public static void ToggleMutePlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "PausePlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		ToggleMutePlaylists(list);
	}

	public static void ToggleMuteAllPlaylists()
	{
		ToggleMutePlaylists(PlaylistController.Instances);
	}

	private static void ToggleMutePlaylists(List<PlaylistController> playlists)
	{
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].ToggleMutePlaylist();
		}
	}

	public static void PausePlaylist()
	{
		PausePlaylist("~only~");
	}

	public static void PausePlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "PausePlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		PausePlaylists(list);
	}

	public static void PauseAllPlaylists()
	{
		PausePlaylists(PlaylistController.Instances);
	}

	private static void PausePlaylists(List<PlaylistController> playlists)
	{
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].PausePlaylist();
		}
	}

	public static void UnpausePlaylist()
	{
		UnpausePlaylist("~only~");
	}

	public static void UnpausePlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "UnpausePlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		UnpausePlaylists(list);
	}

	public static void UnpauseAllPlaylists()
	{
		UnpausePlaylists(PlaylistController.Instances);
	}

	private static void UnpausePlaylists(List<PlaylistController> controllers)
	{
		for (int i = 0; i < controllers.Count; i++)
		{
			controllers[i].UnpausePlaylist();
		}
	}

	public static void StopPlaylist()
	{
		StopPlaylist("~only~");
	}

	public static void StopPlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "StopPlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		StopPlaylists(list);
	}

	public static void StopAllPlaylists()
	{
		StopPlaylists(PlaylistController.Instances);
	}

	private static void StopPlaylists(List<PlaylistController> playlists)
	{
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].StopPlaylist();
		}
	}

	public static void TriggerNextPlaylistClip()
	{
		TriggerNextPlaylistClip("~only~");
	}

	public static void TriggerNextPlaylistClip(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "TriggerNextPlaylistClip"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		NextPlaylistClips(list);
	}

	public static void TriggerNextClipAllPlaylists()
	{
		NextPlaylistClips(PlaylistController.Instances);
	}

	private static void NextPlaylistClips(List<PlaylistController> playlists)
	{
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].PlayNextSong();
		}
	}

	public static void TriggerRandomPlaylistClip()
	{
		TriggerRandomPlaylistClip("~only~");
	}

	public static void TriggerRandomPlaylistClip(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "TriggerRandomPlaylistClip"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		RandomPlaylistClips(list);
	}

	public static void TriggerRandomClipAllPlaylists()
	{
		RandomPlaylistClips(PlaylistController.Instances);
	}

	private static void RandomPlaylistClips(List<PlaylistController> playlists)
	{
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].PlayRandomSong();
		}
	}

	public static void RestartPlaylist()
	{
		RestartPlaylist("~only~");
	}

	public static void RestartPlaylist(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		PlaylistController playlistController;
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "RestartPlaylist"))
			{
				return;
			}
			playlistController = instances[0];
		}
		else
		{
			PlaylistController playlistController2 = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController2 == null)
			{
				return;
			}
			playlistController = playlistController2;
		}
		if (playlistController != null)
		{
			RestartPlaylists(new List<PlaylistController> { playlistController });
		}
	}

	public static void RestartAllPlaylists()
	{
		RestartPlaylists(PlaylistController.Instances);
	}

	private static void RestartPlaylists(List<PlaylistController> playlists)
	{
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].RestartPlaylist();
		}
	}

	public static void StartPlaylist(string playlistName)
	{
		StartPlaylist("~only~", playlistName);
	}

	public static void StartPlaylist(string playlistControllerName, string playlistName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "PausePlaylist"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].StartPlaylist(playlistName);
		}
	}

	public static void StopLoopingAllCurrentSongs()
	{
		StopLoopingCurrentSongs(PlaylistController.Instances);
	}

	public static void StopLoopingCurrentSong()
	{
		StopLoopingCurrentSong("~only~");
	}

	public static void StopLoopingCurrentSong(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		PlaylistController playlistController;
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "StopLoopingCurrentSong"))
			{
				return;
			}
			playlistController = instances[0];
		}
		else
		{
			PlaylistController playlistController2 = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController2 == null)
			{
				return;
			}
			playlistController = playlistController2;
		}
		if (playlistController != null)
		{
			StopLoopingCurrentSongs(new List<PlaylistController> { playlistController });
		}
	}

	private static void StopLoopingCurrentSongs(List<PlaylistController> playlistControllers)
	{
		for (int i = 0; i < playlistControllers.Count; i++)
		{
			playlistControllers[i].StopLoopingCurrentSong();
		}
	}

	public static void StopAllPlaylistsAfterCurrentSongs()
	{
		StopPlaylistAfterCurrentSongs(PlaylistController.Instances);
	}

	public static void StopPlaylistAfterCurrentSong()
	{
		StopPlaylistAfterCurrentSong("~only~");
	}

	public static void StopPlaylistAfterCurrentSong(string playlistControllerName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		PlaylistController playlistController;
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "StopPlaylistAfterCurrentSong"))
			{
				return;
			}
			playlistController = instances[0];
		}
		else
		{
			PlaylistController playlistController2 = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController2 == null)
			{
				return;
			}
			playlistController = playlistController2;
		}
		if (playlistController != null)
		{
			StopPlaylistAfterCurrentSongs(new List<PlaylistController> { playlistController });
		}
	}

	private static void StopPlaylistAfterCurrentSongs(List<PlaylistController> playlistControllers)
	{
		for (int i = 0; i < playlistControllers.Count; i++)
		{
			playlistControllers[i].StopPlaylistAfterCurrentSong();
		}
	}

	public static void QueuePlaylistClip(string clipName)
	{
		QueuePlaylistClip("~only~", clipName);
	}

	public static void QueuePlaylistClip(string playlistControllerName, string clipName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		PlaylistController playlistController;
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "QueuePlaylistClip"))
			{
				return;
			}
			playlistController = instances[0];
		}
		else
		{
			PlaylistController playlistController2 = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController2 == null)
			{
				return;
			}
			playlistController = playlistController2;
		}
		if (playlistController != null)
		{
			playlistController.QueuePlaylistClip(clipName);
		}
	}

	public static bool TriggerPlaylistClip(string clipName)
	{
		return TriggerPlaylistClip("~only~", clipName);
	}

	public static bool TriggerPlaylistClip(string playlistControllerName, string clipName)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		PlaylistController playlistController;
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "TriggerPlaylistClip"))
			{
				return false;
			}
			playlistController = instances[0];
		}
		else
		{
			PlaylistController playlistController2 = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController2 == null)
			{
				return false;
			}
			playlistController = playlistController2;
		}
		if (playlistController == null)
		{
			return false;
		}
		return playlistController.TriggerPlaylistClip(clipName);
	}

	public static void ChangePlaylistByName(string playlistName, bool playFirstClip = true)
	{
		ChangePlaylistByName("~only~", playlistName, playFirstClip);
	}

	public static void ChangePlaylistByName(string playlistControllerName, string playlistName, bool playFirstClip = true)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		PlaylistController playlistController;
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "ChangePlaylistByName"))
			{
				return;
			}
			playlistController = instances[0];
		}
		else
		{
			PlaylistController playlistController2 = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController2 == null)
			{
				return;
			}
			playlistController = playlistController2;
		}
		if (playlistController != null)
		{
			playlistController.ChangePlaylist(playlistName, playFirstClip);
		}
	}

	public static void FadePlaylistToVolume(float targetVolume, float fadeTime)
	{
		FadePlaylistToVolume("~only~", targetVolume, fadeTime);
	}

	public static void FadePlaylistToVolume(string playlistControllerName, float targetVolume, float fadeTime)
	{
		List<PlaylistController> instances = PlaylistController.Instances;
		List<PlaylistController> list = new List<PlaylistController>();
		if (playlistControllerName == "~only~")
		{
			if (!IsOkToCallOnlyPlaylistMethod(instances, "FadePlaylistToVolume"))
			{
				return;
			}
			list.Add(instances[0]);
		}
		else
		{
			PlaylistController playlistController = PlaylistController.InstanceByName(playlistControllerName);
			if (playlistController != null)
			{
				list.Add(playlistController);
			}
		}
		FadePlaylists(list, targetVolume, fadeTime);
	}

	public static void FadeAllPlaylistsToVolume(float targetVolume, float fadeTime)
	{
		FadePlaylists(PlaylistController.Instances, targetVolume, fadeTime);
	}

	private static void FadePlaylists(List<PlaylistController> playlists, float targetVolume, float fadeTime)
	{
		if (targetVolume < 0f || targetVolume > 1f)
		{
			Debug.LogError("Illegal volume passed to FadePlaylistToVolume: '" + targetVolume + "'. Legal volumes are between 0 and 1");
			return;
		}
		for (int i = 0; i < playlists.Count; i++)
		{
			playlists[i].FadeToVolume(targetVolume, fadeTime);
		}
	}

	public static void CreatePlaylist(Playlist playlist, bool errorOnDuplicate)
	{
		Playlist playlist2 = GrabPlaylist(playlist.playlistName, logErrorIfNotFound: false);
		if (playlist2 != null)
		{
			if (errorOnDuplicate)
			{
				Debug.LogError("You already have a Playlist Controller with the name '" + playlist2.playlistName + "'. You must name them all uniquely. Not adding duplicate named Playlist.");
			}
		}
		else
		{
			MusicPlaylists.Add(playlist);
		}
	}

	public static void DeletePlaylist(string playlistName)
	{
		if (SafeInstance == null)
		{
			return;
		}
		Playlist playlist = GrabPlaylist(playlistName);
		if (playlist == null)
		{
			return;
		}
		for (int i = 0; i < PlaylistController.Instances.Count; i++)
		{
			PlaylistController playlistController = PlaylistController.Instances[i];
			if (!(playlistController.PlaylistName != playlistName))
			{
				playlistController.StopPlaylist();
				break;
			}
		}
		MusicPlaylists.Remove(playlist);
	}

	public static void AddSongToPlaylist(string playlistName, AudioClip song, bool loopSong = false, float songPitch = 1f, float songVolume = 1f)
	{
		Playlist playlist = GrabPlaylist(playlistName);
		if (playlist != null)
		{
			MusicSetting item = new MusicSetting
			{
				clip = song,
				isExpanded = true,
				isLoop = loopSong,
				pitch = songPitch,
				volume = songVolume
			};
			playlist.MusicSettings.Add(item);
		}
	}

	public static void ReDownloadAllInternetFiles()
	{
		List<SoundGroupVariation> list = new List<SoundGroupVariation>();
		foreach (string key in Instance.AudioSourcesBySoundType.Keys)
		{
			for (int i = 0; i < Instance.AudioSourcesBySoundType[key].Sources.Count; i++)
			{
				SoundGroupVariation component = Instance.AudioSourcesBySoundType[key].Sources[i].Source.GetComponent<SoundGroupVariation>();
				if (!(component == null) && component.audLocation == AudioLocation.FileOnInternet)
				{
					AudioResourceOptimizer.RemoveLoadedInternetClip(component.internetFileUrl);
					component.internetFileLoadStatus = InternetFileLoadStatus.Loading;
					list.Add(component);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			SoundGroupVariation soundGroupVariation = list[j];
			soundGroupVariation.Stop();
			AudioResourceOptimizer.AddTargetForClip(soundGroupVariation.internetFileUrl, soundGroupVariation.VarAudio);
			soundGroupVariation.LoadInternetFile();
		}
	}

	public static void FireCustomEventNextFrame(string customEventName, Transform eventOrigin)
	{
		if (!AppIsShuttingDown && !("[None]" == customEventName) && !string.IsNullOrEmpty(customEventName))
		{
			if (!CustomEventExists(customEventName) && !IsWarming)
			{
				Debug.LogError("Custom Event '" + customEventName + "' was not found in Master Audio.");
				return;
			}
			Instance.CustomEventsToFire.Enqueue(new CustomEventToFireInfo
			{
				eventName = customEventName,
				eventOrigin = eventOrigin
			});
		}
	}

	public static void AddCustomEventReceiver(ICustomEventReceiver receiver, Transform receiverTrans)
	{
		if (AppIsShuttingDown)
		{
			return;
		}
		IList<AudioEventGroup> allEvents = receiver.GetAllEvents();
		for (int i = 0; i < allEvents.Count; i++)
		{
			AudioEventGroup audioEventGroup = allEvents[i];
			if (!receiver.SubscribesToEvent(audioEventGroup.customEventName))
			{
				continue;
			}
			if (!Instance.ReceiversByEventName.ContainsKey(audioEventGroup.customEventName))
			{
				Instance.ReceiversByEventName.Add(audioEventGroup.customEventName, new Dictionary<ICustomEventReceiver, Transform> { { receiver, receiverTrans } });
				continue;
			}
			Dictionary<ICustomEventReceiver, Transform> dictionary = Instance.ReceiversByEventName[audioEventGroup.customEventName];
			if (!dictionary.ContainsKey(receiver))
			{
				dictionary.Add(receiver, receiverTrans);
			}
		}
	}

	public static void RemoveCustomEventReceiver(ICustomEventReceiver receiver)
	{
		if (AppIsShuttingDown || SafeInstance == null)
		{
			if (!(SafeInstance != null))
			{
				return;
			}
			{
				foreach (string key in Instance.ReceiversByEventName.Keys)
				{
					Instance.ReceiversByEventName[key].Remove(receiver);
				}
				return;
			}
		}
		for (int i = 0; i < Instance.customEvents.Count; i++)
		{
			CustomEvent customEvent = Instance.customEvents[i];
			if (receiver.SubscribesToEvent(customEvent.EventName))
			{
				Instance.ReceiversByEventName[customEvent.EventName].Remove(receiver);
			}
		}
	}

	public static List<Transform> ReceiversForEvent(string customEventName)
	{
		List<Transform> list = new List<Transform>();
		if (!Instance.ReceiversByEventName.ContainsKey(customEventName))
		{
			return list;
		}
		Dictionary<ICustomEventReceiver, Transform> dictionary = Instance.ReceiversByEventName[customEventName];
		foreach (ICustomEventReceiver key in dictionary.Keys)
		{
			if (key.SubscribesToEvent(customEventName))
			{
				list.Add(dictionary[key]);
			}
		}
		return list;
	}

	public static CustomEventCategory CreateCustomEventCategoryIfNotThere(string categoryName, bool isTemporary)
	{
		if (AppIsShuttingDown)
		{
			return null;
		}
		if (Instance.customEventCategories.FindAll((CustomEventCategory cat) => cat.CatName == categoryName).Count > 0)
		{
			return null;
		}
		CustomEventCategory customEventCategory = new CustomEventCategory
		{
			CatName = categoryName,
			ProspectiveName = categoryName,
			IsTemporary = isTemporary
		};
		Instance.customEventCategories.Add(customEventCategory);
		return customEventCategory;
	}

	public static void CreateCustomEvent(string customEventName, CustomEventReceiveMode eventReceiveMode, float distanceThreshold, EventReceiveFilter receiveFilter, int filterModeQty, string categoryName = "", bool isTemporary = false, bool errorOnDuplicate = true)
	{
		if (AppIsShuttingDown)
		{
			return;
		}
		if (Instance.customEvents.FindAll((CustomEvent obj) => obj.EventName == customEventName).Count > 0)
		{
			if (errorOnDuplicate)
			{
				Debug.LogError("You already have a Custom Event named '" + customEventName + "'. No need to add it again.");
			}
			return;
		}
		if (string.IsNullOrEmpty(categoryName))
		{
			categoryName = Instance.customEventCategories[0].CatName;
		}
		CustomEvent item = new CustomEvent(customEventName)
		{
			eventReceiveMode = eventReceiveMode,
			distanceThreshold = distanceThreshold,
			eventRcvFilterMode = receiveFilter,
			filterModeQty = filterModeQty,
			categoryName = categoryName,
			isTemporary = isTemporary
		};
		Instance.customEvents.Add(item);
	}

	public static void DeleteCustomEvent(string customEventName)
	{
		if (!AppIsShuttingDown && !(SafeInstance == null))
		{
			Instance.customEvents.RemoveAll((CustomEvent obj) => obj.EventName == customEventName);
		}
	}

	public static CustomEvent GetCustomEventByName(string customEventName)
	{
		List<CustomEvent> list = Instance.customEvents.FindAll((CustomEvent obj) => obj.EventName == customEventName);
		if (list.Count <= 0)
		{
			return null;
		}
		return list[0];
	}

	public static void FireCustomEvent(string customEventName, Transform originObject, bool logDupe = true)
	{
		if (AppIsShuttingDown || "[None]" == customEventName || string.IsNullOrEmpty(customEventName))
		{
			return;
		}
		if (originObject == null)
		{
			Debug.LogError("Custom Event '" + customEventName + "' cannot be fired without an originObject passed in.");
			return;
		}
		if (!CustomEventExists(customEventName) && !IsWarming)
		{
			Debug.LogError("Custom Event '" + customEventName + "' was not found in Master Audio.");
			return;
		}
		CustomEvent customEventByName = GetCustomEventByName(customEventName);
		if (customEventByName == null)
		{
			return;
		}
		if (customEventByName.frameLastFired >= AudioUtil.FrameCount)
		{
			if (logDupe)
			{
				Debug.LogWarning("Already fired Custom Event '" + customEventName + "' this frame or later. Cannot be fired twice in the same frame.");
			}
			return;
		}
		customEventByName.frameLastFired = AudioUtil.FrameCount;
		if (!Instance.disableLogging && Instance.logCustomEvents)
		{
			Debug.Log("Firing Custom Event: " + customEventName);
		}
		if (!Instance.ReceiversByEventName.ContainsKey(customEventName))
		{
			return;
		}
		Vector3 position = originObject.position;
		float? num = null;
		Dictionary<ICustomEventReceiver, Transform> dictionary = Instance.ReceiversByEventName[customEventName];
		List<ICustomEventReceiver> list = null;
		switch (customEventByName.eventReceiveMode)
		{
		case CustomEventReceiveMode.Never:
			if (Instance.LogSounds)
			{
				Debug.LogWarning("Custom Event '" + customEventName + "' not being transmitted because it is set to 'Never transmit'.");
			}
			return;
		case CustomEventReceiveMode.OnChildGameObject:
			list = GetChildReceivers(originObject, customEventName, includeSelf: false);
			break;
		case CustomEventReceiveMode.OnParentGameObject:
			list = GetParentReceivers(originObject, customEventName, includeSelf: false);
			break;
		case CustomEventReceiveMode.OnSameOrChildGameObject:
			list = GetChildReceivers(originObject, customEventName, includeSelf: true);
			break;
		case CustomEventReceiveMode.OnSameOrParentGameObject:
			list = GetParentReceivers(originObject, customEventName, includeSelf: true);
			break;
		case CustomEventReceiveMode.WhenDistanceLessThan:
		case CustomEventReceiveMode.WhenDistanceMoreThan:
			num = customEventByName.distanceThreshold * customEventByName.distanceThreshold;
			break;
		}
		if (list == null)
		{
			list = new List<ICustomEventReceiver>();
			foreach (ICustomEventReceiver key in dictionary.Keys)
			{
				switch (customEventByName.eventReceiveMode)
				{
				case CustomEventReceiveMode.WhenDistanceLessThan:
					if ((dictionary[key].position - position).sqrMagnitude > num)
					{
						continue;
					}
					break;
				case CustomEventReceiveMode.WhenDistanceMoreThan:
					if ((dictionary[key].position - position).sqrMagnitude < num)
					{
						continue;
					}
					break;
				case CustomEventReceiveMode.OnSameGameObject:
					if (originObject != dictionary[key])
					{
						continue;
					}
					break;
				}
				list.Add(key);
			}
		}
		if (customEventByName.eventRcvFilterMode == EventReceiveFilter.All || customEventByName.filterModeQty >= list.Count || list.Count <= 1)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ReceiveEvent(customEventName, position);
			}
			return;
		}
		Instance.ValidReceivers.Clear();
		for (int j = 0; j < list.Count; j++)
		{
			ICustomEventReceiver customEventReceiver = list[j];
			Transform transform = dictionary[customEventReceiver];
			float distance = 0f;
			int randomId = 0;
			switch (customEventByName.eventRcvFilterMode)
			{
			case EventReceiveFilter.Random:
				randomId = UnityEngine.Random.Range(0, 1000);
				break;
			case EventReceiveFilter.Closest:
				distance = (transform.position - position).sqrMagnitude;
				break;
			}
			Instance.ValidReceivers.Add(new CustomEventCandidate(distance, customEventReceiver, transform, randomId));
		}
		switch (customEventByName.eventRcvFilterMode)
		{
		case EventReceiveFilter.Closest:
		{
			Instance.ValidReceivers.Sort((CustomEventCandidate x, CustomEventCandidate y) => x.DistanceAway.CompareTo(y.DistanceAway));
			int filterModeQty = customEventByName.filterModeQty;
			int count = Instance.ValidReceivers.Count - filterModeQty;
			Instance.ValidReceivers.RemoveRange(filterModeQty, count);
			break;
		}
		case EventReceiveFilter.Random:
		{
			Instance.ValidReceivers.Sort((CustomEventCandidate x, CustomEventCandidate y) => x.RandomId.CompareTo(y.RandomId));
			int filterModeQty = customEventByName.filterModeQty;
			int count = Instance.ValidReceivers.Count - filterModeQty;
			Instance.ValidReceivers.RemoveRange(filterModeQty, count);
			break;
		}
		}
		for (int k = 0; k < Instance.ValidReceivers.Count; k++)
		{
			Instance.ValidReceivers[k].Receiver.ReceiveEvent(customEventName, position);
		}
	}

	public static bool CustomEventExists(string customEventName)
	{
		if (AppIsShuttingDown)
		{
			return true;
		}
		return Instance.customEvents.FindAll((CustomEvent obj) => obj.EventName == customEventName).Count > 0;
	}

	private static List<ICustomEventReceiver> GetChildReceivers(Transform origin, string eventName, bool includeSelf)
	{
		List<ICustomEventReceiver> list = origin.GetComponentsInChildren<ICustomEventReceiver>().ToList();
		list.RemoveAll((ICustomEventReceiver rec) => !rec.SubscribesToEvent(eventName));
		if (includeSelf)
		{
			return list;
		}
		return FilterOutSelf(list, origin);
	}

	private static List<ICustomEventReceiver> GetParentReceivers(Transform origin, string eventName, bool includeSelf)
	{
		List<ICustomEventReceiver> list = origin.GetComponentsInParent<ICustomEventReceiver>().ToList();
		list.RemoveAll((ICustomEventReceiver rec) => !rec.SubscribesToEvent(eventName));
		if (includeSelf)
		{
			return list;
		}
		return FilterOutSelf(list, origin);
	}

	private static List<ICustomEventReceiver> FilterOutSelf(List<ICustomEventReceiver> sourceList, Transform origin)
	{
		List<ICustomEventReceiver> list = new List<ICustomEventReceiver>();
		foreach (ICustomEventReceiver source in sourceList)
		{
			MonoBehaviour monoBehaviour = source as MonoBehaviour;
			if (!(monoBehaviour == null) && !(monoBehaviour.transform != origin))
			{
				list.Add(source);
			}
		}
		int num = 0;
		while (list.Count > 0 && num < 20)
		{
			sourceList.Remove(list[0]);
			list.RemoveAt(0);
			num++;
		}
		return sourceList;
	}

	private static bool LoggingEnabledForGroup(MasterAudioGroup grp)
	{
		if (IsWarming)
		{
			return false;
		}
		if (Instance.disableLogging)
		{
			return false;
		}
		if (grp != null && grp.logSound)
		{
			return true;
		}
		return Instance.LogSounds;
	}

	private static void LogMessage(string message)
	{
		if (!Instance.disableLogging)
		{
			Debug.Log("T: " + Time.time + " - MasterAudio " + message);
		}
	}

	public static void LogWarning(string msg)
	{
		if (!Instance.disableLogging)
		{
			Debug.LogWarning(msg);
		}
	}

	public static void LogError(string msg)
	{
		if (!Instance.disableLogging)
		{
			Debug.LogError(msg);
		}
	}

	public static void LogNoPlaylist(string playlistControllerName, string methodName)
	{
		LogWarning("There is currently no Playlist assigned to Playlist Controller '" + playlistControllerName + "'. Cannot call '" + methodName + "' method.");
	}

	private static bool IsOkToCallOnlyPlaylistMethod(List<PlaylistController> pcs, string methodName)
	{
		if (pcs.Count == 0)
		{
			LogError($"You have no Playlist Controllers in the Scene. You cannot '{methodName}'.");
			return false;
		}
		if (pcs.Count > 1)
		{
			LogError($"You cannot call '{methodName}' without specifying a Playlist Controller name when you have more than one Playlist Controller.");
			return false;
		}
		return true;
	}

	public static void QueueTransformFollowerForColliderPositionRecalc(TransformFollower follower)
	{
		if (SafeInstance == null)
		{
			return;
		}
		foreach (TransformFollower transFollowerColliderPositionRecalc in Instance.TransFollowerColliderPositionRecalcs)
		{
			if (transFollowerColliderPositionRecalc == follower)
			{
				return;
			}
		}
		Instance.TransFollowerColliderPositionRecalcs.Enqueue(follower);
	}

	public static void AddToQueuedOcclusionRays(SoundGroupVariationUpdater updater)
	{
		if (SafeInstance == null)
		{
			return;
		}
		foreach (SoundGroupVariationUpdater queuedOcclusionRay in Instance.QueuedOcclusionRays)
		{
			if (queuedOcclusionRay == updater)
			{
				return;
			}
		}
		Instance.QueuedOcclusionRays.Enqueue(updater);
	}

	public static void AddToOcclusionInRangeSources(GameObject src)
	{
		if (Application.isEditor && !(SafeInstance == null) && Instance.occlusionShowCategories)
		{
			if (!Instance.OcclusionSourcesInRange.Contains(src))
			{
				Instance.OcclusionSourcesInRange.Add(src);
			}
			if (Instance.OcclusionSourcesOutOfRange.Contains(src))
			{
				Instance.OcclusionSourcesOutOfRange.Remove(src);
			}
		}
	}

	public static void AddToOcclusionOutOfRangeSources(GameObject src)
	{
		if (Application.isEditor && !(SafeInstance == null) && Instance.occlusionShowCategories)
		{
			if (!Instance.OcclusionSourcesOutOfRange.Contains(src))
			{
				Instance.OcclusionSourcesOutOfRange.Add(src);
			}
			if (Instance.OcclusionSourcesInRange.Contains(src))
			{
				Instance.OcclusionSourcesInRange.Remove(src);
			}
			RemoveFromBlockedOcclusionSources(src);
		}
	}

	public static void AddToBlockedOcclusionSources(GameObject src)
	{
		if (Application.isEditor && !(SafeInstance == null) && Instance.occlusionShowCategories && !Instance.OcclusionSourcesBlocked.Contains(src))
		{
			Instance.OcclusionSourcesBlocked.Add(src);
		}
	}

	public static bool HasQueuedOcclusionRays()
	{
		return Instance.QueuedOcclusionRays.Count > 0;
	}

	public static SoundGroupVariationUpdater OldestQueuedOcclusionRay()
	{
		if (SafeInstance == null)
		{
			return null;
		}
		return Instance.QueuedOcclusionRays.Dequeue();
	}

	public static bool IsOcclusionFreqencyTransitioning(SoundGroupVariation variation)
	{
		for (int i = 0; i < Instance.VariationOcclusionFreqChanges.Count; i++)
		{
			if (Instance.VariationOcclusionFreqChanges[i].ActingVariation == variation)
			{
				return true;
			}
		}
		return false;
	}

	public static void RemoveFromOcclusionFrequencyTransitioning(SoundGroupVariation variation)
	{
		for (int i = 0; i < Instance.VariationOcclusionFreqChanges.Count; i++)
		{
			if (!(Instance.VariationOcclusionFreqChanges[i].ActingVariation != variation))
			{
				Instance.VariationOcclusionFreqChanges.RemoveAt(i);
				break;
			}
		}
	}

	public static void RemoveFromBlockedOcclusionSources(GameObject src)
	{
		if (Application.isEditor && !(SafeInstance == null) && Instance.occlusionShowCategories && Instance.OcclusionSourcesBlocked.Contains(src))
		{
			Instance.OcclusionSourcesBlocked.Remove(src);
		}
	}

	public static void StopTrackingOcclusionForSource(GameObject src)
	{
		if (Application.isEditor && !(SafeInstance == null) && Instance.occlusionShowCategories)
		{
			if (Instance.OcclusionSourcesOutOfRange.Contains(src))
			{
				Instance.OcclusionSourcesOutOfRange.Remove(src);
			}
			if (Instance.OcclusionSourcesInRange.Contains(src))
			{
				Instance.OcclusionSourcesInRange.Remove(src);
			}
			if (Instance.OcclusionSourcesBlocked.Contains(src))
			{
				Instance.OcclusionSourcesBlocked.Remove(src);
			}
		}
	}

	private static bool IsLinkedGroupPlay(SoundGroupVariation variation)
	{
		if (!Instance._isStoppingMultiple)
		{
			return false;
		}
		return Instance.VariationsStartedDuringMultiStop.Contains(variation);
	}

	public static int RemainingClipsInGroup(string sType)
	{
		if (!Instance._randomizer.ContainsKey(sType))
		{
			return 0;
		}
		return Instance._randomizer[sType].Count;
	}

	public static bool HasAsyncResourceLoaderFeature()
	{
		return true;
	}

	public static void RescanGroupsNow()
	{
		Instance._mustRescanGroups = true;
	}

	public static void DoneRescanningGroups()
	{
		Instance._mustRescanGroups = false;
	}

	public static GameObject CreateMasterAudio()
	{
		UnityEngine.Object @object = Resources.Load("Assets/Plugins/DarkTonic/MasterAudio/Prefabs/MasterAudio.prefab", typeof(GameObject));
		if (@object == null)
		{
			Debug.LogError("Could not find MasterAudio prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(@object) as GameObject;
		obj.name = "MasterAudio";
		return obj;
	}

	public static GameObject CreatePlaylistController()
	{
		UnityEngine.Object @object = Resources.Load("Assets/Plugins/DarkTonic/MasterAudio/Prefabs/PlaylistController.prefab", typeof(GameObject));
		if (@object == null)
		{
			Debug.LogError("Could not find PlaylistController prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(@object) as GameObject;
		obj.name = "PlaylistController";
		return obj;
	}

	public static GameObject CreateDynamicSoundGroupCreator()
	{
		UnityEngine.Object @object = Resources.Load("Assets/Plugins/DarkTonic/MasterAudio/Prefabs/DynamicSoundGroupCreator.prefab", typeof(GameObject));
		if (@object == null)
		{
			Debug.LogError("Could not find DynamicSoundGroupCreator prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(@object) as GameObject;
		obj.name = "DynamicSoundGroupCreator";
		return obj;
	}

	public static GameObject CreateSoundGroupOrganizer()
	{
		UnityEngine.Object @object = Resources.Load("Assets/Plugins/DarkTonic/MasterAudio/Prefabs/SoundGroupOrganizer.prefab", typeof(GameObject));
		if (@object == null)
		{
			Debug.LogError("Could not find SoundGroupOrganizer prefab. Please update the Installation Path in the Master Audio Manager window if you have moved the folder from its default location, then try again.");
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(@object) as GameObject;
		obj.name = "SoundGroupOrganizer";
		return obj;
	}
}
