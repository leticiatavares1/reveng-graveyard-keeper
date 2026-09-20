using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkTonic.MasterAudio;

[AddComponentMenu("Dark Tonic/Master Audio/Event Sounds")]
[AudioScriptOrder(-30)]
public class EventSounds : MonoBehaviour, ICustomEventReceiver
{
	public enum UnityUIVersion
	{
		Legacy,
		uGUI
	}

	public enum EventType
	{
		OnStart,
		OnVisible,
		OnInvisible,
		OnCollision,
		OnTriggerEnter,
		OnTriggerExit,
		OnMouseEnter,
		OnMouseClick,
		OnSpawned,
		OnDespawned,
		OnEnable,
		OnDisable,
		OnCollision2D,
		OnTriggerEnter2D,
		OnTriggerExit2D,
		OnParticleCollision,
		UserDefinedEvent,
		OnCollisionExit,
		OnCollisionExit2D,
		OnMouseUp,
		OnMouseExit,
		OnMouseDrag,
		NGUIOnClick,
		NGUIMouseDown,
		NGUIMouseUp,
		NGUIMouseEnter,
		NGUIMouseExit,
		MechanimStateChanged,
		UnitySliderChanged,
		UnityButtonClicked,
		UnityPointerDown,
		UnityPointerUp,
		UnityPointerEnter,
		UnityPointerExit,
		UnityDrag,
		UnityDrop,
		UnityScroll,
		UnityUpdateSelected,
		UnitySelect,
		UnityDeselect,
		UnityMove,
		UnityInitializePotentialDrag,
		UnityBeginDrag,
		UnityEndDrag,
		UnitySubmit,
		UnityCancel,
		UnityToggle,
		OnTriggerStay,
		OnTriggerStay2D
	}

	public enum GlidePitchType
	{
		None,
		RaisePitch,
		LowerPitch
	}

	public enum VariationType
	{
		PlaySpecific,
		PlayRandom
	}

	public enum PreviousSoundStopMode
	{
		None,
		Stop,
		FadeOut
	}

	public enum RetriggerLimMode
	{
		None,
		FrameBased,
		TimeBased
	}

	public bool showGizmo;

	public MasterAudio.SoundSpawnLocationMode soundSpawnMode = MasterAudio.SoundSpawnLocationMode.AttachToCaller;

	public bool disableSounds;

	public bool showPoolManager;

	public bool showNGUI;

	public UnityUIVersion unityUIMode = UnityUIVersion.uGUI;

	public bool logMissingEvents = true;

	public static List<string> LayerTagFilterEvents = new List<string>
	{
		EventType.OnCollision.ToString(),
		EventType.OnTriggerEnter.ToString(),
		EventType.OnTriggerExit.ToString(),
		EventType.OnCollision2D.ToString(),
		EventType.OnTriggerEnter2D.ToString(),
		EventType.OnTriggerExit2D.ToString(),
		EventType.OnParticleCollision.ToString(),
		EventType.OnCollisionExit.ToString(),
		EventType.OnCollisionExit2D.ToString()
	};

	public static List<MasterAudio.PlaylistCommand> PlaylistCommandsWithAll = new List<MasterAudio.PlaylistCommand>
	{
		MasterAudio.PlaylistCommand.FadeToVolume,
		MasterAudio.PlaylistCommand.Pause,
		MasterAudio.PlaylistCommand.PlayNextSong,
		MasterAudio.PlaylistCommand.PlayRandomSong,
		MasterAudio.PlaylistCommand.Resume,
		MasterAudio.PlaylistCommand.Stop,
		MasterAudio.PlaylistCommand.Mute,
		MasterAudio.PlaylistCommand.Unmute,
		MasterAudio.PlaylistCommand.ToggleMute,
		MasterAudio.PlaylistCommand.Restart,
		MasterAudio.PlaylistCommand.StopLoopingCurrentSong,
		MasterAudio.PlaylistCommand.StopPlaylistAfterCurrentSong
	};

	public AudioEventGroup startSound;

	public AudioEventGroup visibleSound;

	public AudioEventGroup invisibleSound;

	public AudioEventGroup collisionSound;

	public AudioEventGroup collisionExitSound;

	public AudioEventGroup triggerSound;

	public AudioEventGroup triggerExitSound;

	public AudioEventGroup triggerStaySound;

	public AudioEventGroup mouseEnterSound;

	public AudioEventGroup mouseExitSound;

	public AudioEventGroup mouseClickSound;

	public AudioEventGroup mouseUpSound;

	public AudioEventGroup mouseDragSound;

	public AudioEventGroup spawnedSound;

	public AudioEventGroup despawnedSound;

	public AudioEventGroup enableSound;

	public AudioEventGroup disableSound;

	public AudioEventGroup collision2dSound;

	public AudioEventGroup collisionExit2dSound;

	public AudioEventGroup triggerEnter2dSound;

	public AudioEventGroup triggerStay2dSound;

	public AudioEventGroup triggerExit2dSound;

	public AudioEventGroup particleCollisionSound;

	public AudioEventGroup nguiOnClickSound;

	public AudioEventGroup nguiMouseDownSound;

	public AudioEventGroup nguiMouseUpSound;

	public AudioEventGroup nguiMouseEnterSound;

	public AudioEventGroup nguiMouseExitSound;

	public AudioEventGroup unitySliderChangedSound;

	public AudioEventGroup unityButtonClickedSound;

	public AudioEventGroup unityPointerDownSound;

	public AudioEventGroup unityDragSound;

	public AudioEventGroup unityPointerUpSound;

	public AudioEventGroup unityPointerEnterSound;

	public AudioEventGroup unityPointerExitSound;

	public AudioEventGroup unityDropSound;

	public AudioEventGroup unityScrollSound;

	public AudioEventGroup unityUpdateSelectedSound;

	public AudioEventGroup unitySelectSound;

	public AudioEventGroup unityDeselectSound;

	public AudioEventGroup unityMoveSound;

	public AudioEventGroup unityInitializePotentialDragSound;

	public AudioEventGroup unityBeginDragSound;

	public AudioEventGroup unityEndDragSound;

	public AudioEventGroup unitySubmitSound;

	public AudioEventGroup unityCancelSound;

	public AudioEventGroup unityToggleSound;

	public List<AudioEventGroup> userDefinedSounds = new List<AudioEventGroup>();

	public List<AudioEventGroup> mechanimStateChangedSounds = new List<AudioEventGroup>();

	public bool useStartSound;

	public bool useVisibleSound;

	public bool useInvisibleSound;

	public bool useCollisionSound;

	public bool useCollisionExitSound;

	public bool useTriggerEnterSound;

	public bool useTriggerExitSound;

	public bool useTriggerStaySound;

	public bool useMouseEnterSound;

	public bool useMouseExitSound;

	public bool useMouseClickSound;

	public bool useMouseUpSound;

	public bool useMouseDragSound;

	public bool useSpawnedSound;

	public bool useDespawnedSound;

	public bool useEnableSound;

	public bool useDisableSound;

	public bool useCollision2dSound;

	public bool useCollisionExit2dSound;

	public bool useTriggerEnter2dSound;

	public bool useTriggerStay2dSound;

	public bool useTriggerExit2dSound;

	public bool useParticleCollisionSound;

	public bool useNguiOnClickSound;

	public bool useNguiMouseDownSound;

	public bool useNguiMouseUpSound;

	public bool useNguiMouseEnterSound;

	public bool useNguiMouseExitSound;

	public bool useUnitySliderChangedSound;

	public bool useUnityButtonClickedSound;

	public bool useUnityPointerDownSound;

	public bool useUnityDragSound;

	public bool useUnityPointerUpSound;

	public bool useUnityPointerEnterSound;

	public bool useUnityPointerExitSound;

	public bool useUnityDropSound;

	public bool useUnityScrollSound;

	public bool useUnityUpdateSelectedSound;

	public bool useUnitySelectSound;

	public bool useUnityDeselectSound;

	public bool useUnityMoveSound;

	public bool useUnityInitializePotentialDragSound;

	public bool useUnityBeginDragSound;

	public bool useUnityEndDragSound;

	public bool useUnitySubmitSound;

	public bool useUnityCancelSound;

	public bool useUnityToggleSound;

	private Slider _slider;

	private Toggle _toggle;

	private Button _button;

	private bool _isVisible;

	private bool _needsCoroutine;

	private float? _triggerEnterTime;

	private float? _triggerEnter2dTime;

	private bool _mouseDragSoundPlayed;

	private PlaySoundResult _mouseDragResult;

	private Transform _trans;

	private readonly List<AudioEventGroup> _validMechanimStateChangedSounds = new List<AudioEventGroup>();

	private Animator _anim;

	private bool IsSetToUGUI => unityUIMode != UnityUIVersion.Legacy;

	private bool IsSetToLegacyUI => unityUIMode == UnityUIVersion.Legacy;

	private void Awake()
	{
		_trans = base.transform;
		_anim = GetComponent<Animator>();
		_slider = GetComponent<Slider>();
		_button = GetComponent<Button>();
		_toggle = GetComponent<Toggle>();
		if (IsSetToUGUI)
		{
			AddUGUIComponents();
		}
		SpawnedOrAwake();
	}

	protected virtual void SpawnedOrAwake()
	{
		_isVisible = false;
		_validMechanimStateChangedSounds.Clear();
		_needsCoroutine = false;
		if (disableSounds || _anim == null)
		{
			return;
		}
		for (int i = 0; i < mechanimStateChangedSounds.Count; i++)
		{
			AudioEventGroup audioEventGroup = mechanimStateChangedSounds[i];
			if (audioEventGroup.mechanimEventActive && !string.IsNullOrEmpty(audioEventGroup.mechanimStateName))
			{
				_needsCoroutine = true;
				_validMechanimStateChangedSounds.Add(audioEventGroup);
			}
		}
	}

	private IEnumerator CoUpdate()
	{
		while (true)
		{
			yield return MasterAudio.EndOfFrameDelay;
			for (int i = 0; i < _validMechanimStateChangedSounds.Count; i++)
			{
				AudioEventGroup audioEventGroup = _validMechanimStateChangedSounds[i];
				if (!_anim.GetCurrentAnimatorStateInfo(0).IsName(audioEventGroup.mechanimStateName))
				{
					audioEventGroup.mechEventPlayedForState = false;
				}
				else if (!audioEventGroup.mechEventPlayedForState)
				{
					audioEventGroup.mechEventPlayedForState = true;
					PlaySounds(audioEventGroup, EventType.MechanimStateChanged);
				}
			}
		}
	}

	private void Start()
	{
		CheckForIllegalCustomEvents();
		if (useStartSound)
		{
			PlaySounds(startSound, EventType.OnStart);
		}
	}

	private void OnBecameVisible()
	{
		if (useVisibleSound && !_isVisible)
		{
			_isVisible = true;
			PlaySounds(visibleSound, EventType.OnVisible);
		}
	}

	private void OnBecameInvisible()
	{
		if (useInvisibleSound)
		{
			_isVisible = false;
			PlaySounds(invisibleSound, EventType.OnInvisible);
		}
	}

	private void OnEnable()
	{
		if (_slider != null)
		{
			_slider.onValueChanged.AddListener(SliderChanged);
			RestorePersistentSliders();
		}
		if (_button != null)
		{
			_button.onClick.AddListener(ButtonClicked);
		}
		if (_toggle != null)
		{
			_toggle.onValueChanged.AddListener(ToggleChanged);
		}
		_mouseDragResult = null;
		RegisterReceiver();
		if (_needsCoroutine)
		{
			StopAllCoroutines();
			StartCoroutine(CoUpdate());
		}
		if (useEnableSound)
		{
			PlaySounds(enableSound, EventType.OnEnable);
		}
	}

	private void RestorePersistentSliders()
	{
		if (!useUnitySliderChangedSound)
		{
			return;
		}
		foreach (AudioEvent soundEvent in unitySliderChangedSound.SoundEvents)
		{
			if (soundEvent.currentSoundFunctionType != MasterAudio.EventSoundFunctionType.PersistentSettingsControl || soundEvent.targetVolMode != 0)
			{
				continue;
			}
			switch (soundEvent.currentPersistentSettingsCommand)
			{
			case MasterAudio.PersistentSettingsCommand.SetMusicVolume:
			{
				float? musicVolume = PersistentAudioSettings.MusicVolume;
				if (musicVolume.HasValue)
				{
					_slider.value = musicVolume.Value;
				}
				break;
			}
			case MasterAudio.PersistentSettingsCommand.SetBusVolume:
				if (!soundEvent.allSoundTypesForBusCmd)
				{
					float? busVolume = PersistentAudioSettings.GetBusVolume(soundEvent.busName);
					if (busVolume.HasValue)
					{
						_slider.value = busVolume.Value;
					}
				}
				break;
			case MasterAudio.PersistentSettingsCommand.SetMixerVolume:
			{
				float? mixerVolume = PersistentAudioSettings.MixerVolume;
				if (mixerVolume.HasValue)
				{
					_slider.value = mixerVolume.Value;
				}
				break;
			}
			case MasterAudio.PersistentSettingsCommand.SetGroupVolume:
				if (!soundEvent.allSoundTypesForGroupCmd)
				{
					float? groupVolume = PersistentAudioSettings.GetGroupVolume(soundEvent.soundType);
					if (groupVolume.HasValue)
					{
						_slider.value = groupVolume.Value;
					}
				}
				break;
			}
		}
	}

	private void OnDisable()
	{
		if (!(MasterAudio.SafeInstance == null) && !MasterAudio.AppIsShuttingDown)
		{
			if (_slider != null)
			{
				_slider.onValueChanged.RemoveListener(SliderChanged);
			}
			if (_button != null)
			{
				_button.onClick.RemoveListener(ButtonClicked);
			}
			if (_toggle != null)
			{
				_toggle.onValueChanged.RemoveListener(ToggleChanged);
			}
			UnregisterReceiver();
			if (useDisableSound && !MasterAudio.AppIsShuttingDown)
			{
				PlaySounds(disableSound, EventType.OnDisable);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		_triggerEnter2dTime = Time.realtimeSinceStartup;
		if (useTriggerEnter2dSound && (!triggerEnter2dSound.useLayerFilter || triggerEnter2dSound.matchingLayers.Contains(other.gameObject.layer)) && (!triggerEnter2dSound.useTagFilter || triggerEnter2dSound.matchingTags.Contains(other.gameObject.tag)))
		{
			PlaySounds(triggerEnter2dSound, EventType.OnTriggerEnter2D);
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (useTriggerStay2dSound)
		{
			float num = 0f;
			num = (_triggerEnter2dTime.HasValue ? (Time.realtimeSinceStartup - _triggerEnter2dTime.Value) : 0f);
			if (!(triggerStay2dSound.triggerStayForTime > num) && PlaySounds(triggerStay2dSound, EventType.OnTriggerStay2D) && triggerStay2dSound.doesTriggerStayRepeat)
			{
				_triggerEnter2dTime = Time.realtimeSinceStartup;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		_triggerEnter2dTime = null;
		if (useTriggerExit2dSound && (!triggerExit2dSound.useLayerFilter || triggerExit2dSound.matchingLayers.Contains(other.gameObject.layer)) && (!triggerExit2dSound.useTagFilter || triggerExit2dSound.matchingTags.Contains(other.gameObject.tag)))
		{
			PlaySounds(triggerExit2dSound, EventType.OnTriggerExit2D);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (useCollision2dSound && (!collision2dSound.useLayerFilter || collision2dSound.matchingLayers.Contains(collision.gameObject.layer)) && (!collision2dSound.useTagFilter || collision2dSound.matchingTags.Contains(collision.gameObject.tag)))
		{
			PlaySounds(collision2dSound, EventType.OnCollision2D);
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (useCollisionExit2dSound && (!collisionExit2dSound.useLayerFilter || collisionExit2dSound.matchingLayers.Contains(collision.gameObject.layer)) && (!collisionExit2dSound.useTagFilter || collisionExit2dSound.matchingTags.Contains(collision.gameObject.tag)))
		{
			PlaySounds(collisionExit2dSound, EventType.OnCollisionExit2D);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (useCollisionSound && (!collisionSound.useLayerFilter || collisionSound.matchingLayers.Contains(collision.gameObject.layer)) && (!collisionSound.useTagFilter || collisionSound.matchingTags.Contains(collision.gameObject.tag)))
		{
			PlaySounds(collisionSound, EventType.OnCollision);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (useCollisionExitSound && (!collisionExitSound.useLayerFilter || collisionExitSound.matchingLayers.Contains(collision.gameObject.layer)) && (!collisionExitSound.useTagFilter || collisionExitSound.matchingTags.Contains(collision.gameObject.tag)))
		{
			PlaySounds(collisionExitSound, EventType.OnCollisionExit);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		_triggerEnterTime = Time.realtimeSinceStartup;
		if (useTriggerEnterSound && (!triggerSound.useLayerFilter || triggerSound.matchingLayers.Contains(other.gameObject.layer)) && (!triggerSound.useTagFilter || triggerSound.matchingTags.Contains(other.gameObject.tag)))
		{
			PlaySounds(triggerSound, EventType.OnTriggerEnter);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (useTriggerStaySound)
		{
			float num = 0f;
			num = (_triggerEnterTime.HasValue ? (Time.realtimeSinceStartup - _triggerEnterTime.Value) : 0f);
			if (!(triggerStaySound.triggerStayForTime > num) && PlaySounds(triggerStaySound, EventType.OnTriggerStay) && triggerStaySound.doesTriggerStayRepeat)
			{
				_triggerEnterTime = Time.realtimeSinceStartup;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		_triggerEnterTime = null;
		if (useTriggerExitSound && (!triggerExitSound.useLayerFilter || triggerExitSound.matchingLayers.Contains(other.gameObject.layer)) && (!triggerExitSound.useTagFilter || triggerExitSound.matchingTags.Contains(other.gameObject.tag)))
		{
			PlaySounds(triggerExitSound, EventType.OnTriggerExit);
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		if (useParticleCollisionSound && (!particleCollisionSound.useLayerFilter || particleCollisionSound.matchingLayers.Contains(other.gameObject.layer)) && (!particleCollisionSound.useTagFilter || particleCollisionSound.matchingTags.Contains(other.gameObject.tag)))
		{
			PlaySounds(particleCollisionSound, EventType.OnParticleCollision);
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityPointerEnterSound)
		{
			PlaySounds(unityPointerEnterSound, EventType.UnityPointerEnter);
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityPointerExitSound)
		{
			PlaySounds(unityPointerExitSound, EventType.UnityPointerExit);
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityPointerDownSound)
		{
			PlaySounds(unityPointerDownSound, EventType.UnityPointerDown);
		}
	}

	public void OnPointerUp(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityPointerUpSound)
		{
			PlaySounds(unityPointerUpSound, EventType.UnityPointerUp);
		}
	}

	private void OnDrag(Vector2 delta)
	{
	}

	public void OnDrag(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityDragSound)
		{
			PlaySounds(unityDragSound, EventType.UnityDrag);
		}
	}

	private void OnDrop(GameObject go)
	{
	}

	public void OnDrop(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityDropSound)
		{
			PlaySounds(unityDropSound, EventType.UnityDrop);
		}
	}

	public void OnScroll(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityScrollSound)
		{
			PlaySounds(unityScrollSound, EventType.UnityScroll);
		}
	}

	public void OnUpdateSelected(BaseEventData data)
	{
		if (IsSetToUGUI && useUnityUpdateSelectedSound)
		{
			PlaySounds(unityUpdateSelectedSound, EventType.UnityUpdateSelected);
		}
	}

	private void OnSelect(bool isSelected)
	{
	}

	public void OnSelect(BaseEventData data)
	{
		if (IsSetToUGUI && useUnitySelectSound)
		{
			PlaySounds(unitySelectSound, EventType.UnitySelect);
		}
	}

	public void OnDeselect(BaseEventData data)
	{
		if (IsSetToUGUI && useUnityDeselectSound)
		{
			PlaySounds(unityDeselectSound, EventType.UnityDeselect);
		}
	}

	public void OnMove(AxisEventData data)
	{
		if (IsSetToUGUI && useUnityMoveSound)
		{
			PlaySounds(unityMoveSound, EventType.UnityMove);
		}
	}

	public void OnInitializePotentialDrag(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityInitializePotentialDragSound)
		{
			PlaySounds(unityInitializePotentialDragSound, EventType.UnityInitializePotentialDrag);
		}
	}

	public void OnBeginDrag(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityBeginDragSound)
		{
			PlaySounds(unityBeginDragSound, EventType.UnityBeginDrag);
		}
	}

	public void OnEndDrag(PointerEventData data)
	{
		if (IsSetToUGUI && useUnityEndDragSound)
		{
			PlaySounds(unityEndDragSound, EventType.UnityEndDrag);
		}
	}

	public void OnSubmit(BaseEventData data)
	{
		if (IsSetToUGUI && useUnitySubmitSound)
		{
			PlaySounds(unitySubmitSound, EventType.UnitySubmit);
		}
	}

	public void OnCancel(BaseEventData data)
	{
		if (IsSetToUGUI && useUnityCancelSound)
		{
			PlaySounds(unityCancelSound, EventType.UnityCancel);
		}
	}

	private void SliderChanged(float newValue)
	{
		if (useUnitySliderChangedSound)
		{
			unitySliderChangedSound.sliderValue = newValue;
			PlaySounds(unitySliderChangedSound, EventType.UnitySliderChanged);
		}
	}

	private void ToggleChanged(bool newValue)
	{
		if (useUnityToggleSound)
		{
			PlaySounds(unityToggleSound, EventType.UnityToggle);
		}
	}

	private void ButtonClicked()
	{
		if (useUnityButtonClickedSound)
		{
			PlaySounds(unityButtonClickedSound, EventType.UnityButtonClicked);
		}
	}

	private void OnMouseEnter()
	{
		if (IsSetToLegacyUI && useMouseEnterSound)
		{
			PlaySounds(mouseEnterSound, EventType.OnMouseEnter);
		}
	}

	private void OnMouseExit()
	{
		if (IsSetToLegacyUI && useMouseExitSound)
		{
			PlaySounds(mouseExitSound, EventType.OnMouseExit);
		}
	}

	private void OnMouseDown()
	{
		if (IsSetToLegacyUI && useMouseClickSound)
		{
			PlaySounds(mouseClickSound, EventType.OnMouseClick);
		}
	}

	private void OnMouseUp()
	{
		if (IsSetToLegacyUI && useMouseUpSound)
		{
			PlaySounds(mouseUpSound, EventType.OnMouseUp);
		}
		if (useMouseDragSound)
		{
			switch (mouseUpSound.mouseDragStopMode)
			{
			case PreviousSoundStopMode.Stop:
				if (_mouseDragResult != null && (_mouseDragResult.SoundPlayed || _mouseDragResult.SoundScheduled))
				{
					_mouseDragResult.ActingVariation.Stop(stopEndDetection: true);
				}
				break;
			case PreviousSoundStopMode.FadeOut:
				if (_mouseDragResult != null && (_mouseDragResult.SoundPlayed || _mouseDragResult.SoundScheduled))
				{
					_mouseDragResult.ActingVariation.FadeToVolume(0f, mouseUpSound.mouseDragFadeOutTime);
				}
				break;
			}
			_mouseDragResult = null;
		}
		_mouseDragSoundPlayed = false;
	}

	private void OnMouseDrag()
	{
		if (IsSetToLegacyUI && useMouseDragSound && !_mouseDragSoundPlayed)
		{
			PlaySounds(mouseDragSound, EventType.OnMouseDrag);
			_mouseDragSoundPlayed = true;
		}
	}

	private void OnPress(bool isDown)
	{
		if (!showNGUI)
		{
			return;
		}
		if (isDown)
		{
			if (useNguiMouseDownSound)
			{
				PlaySounds(nguiMouseDownSound, EventType.NGUIMouseDown);
			}
		}
		else if (useNguiMouseUpSound)
		{
			PlaySounds(nguiMouseUpSound, EventType.NGUIMouseUp);
		}
	}

	private void OnClick()
	{
		if (showNGUI && useNguiOnClickSound)
		{
			PlaySounds(nguiOnClickSound, EventType.NGUIOnClick);
		}
	}

	private void OnHover(bool isOver)
	{
		if (!showNGUI)
		{
			return;
		}
		if (isOver)
		{
			if (useNguiMouseEnterSound)
			{
				PlaySounds(nguiMouseEnterSound, EventType.NGUIMouseEnter);
			}
		}
		else if (useNguiMouseExitSound)
		{
			PlaySounds(nguiMouseExitSound, EventType.NGUIMouseExit);
		}
	}

	private void OnSpawned()
	{
		SpawnedOrAwake();
		if (showPoolManager && useSpawnedSound)
		{
			PlaySounds(spawnedSound, EventType.OnSpawned);
		}
	}

	private void OnDespawned()
	{
		if (showPoolManager && useDespawnedSound)
		{
			PlaySounds(despawnedSound, EventType.OnDespawned);
		}
	}

	public AudioEventGroup GetMechanimAudioEventGroup(string stateName)
	{
		return _validMechanimStateChangedSounds.Find((AudioEventGroup grp) => grp.mechanimStateName == stateName);
	}

	public bool PlaySounds(AudioEventGroup eventGrp, EventType eType)
	{
		if (!CheckForRetriggerLimit(eventGrp))
		{
			return false;
		}
		if (MasterAudio.SafeInstance == null)
		{
			return false;
		}
		switch (eventGrp.retriggerLimitMode)
		{
		case RetriggerLimMode.FrameBased:
			eventGrp.triggeredLastFrame = AudioUtil.FrameCount;
			break;
		case RetriggerLimMode.TimeBased:
			eventGrp.triggeredLastTime = AudioUtil.Time;
			break;
		}
		if (!MasterAudio.AppIsShuttingDown && MasterAudio.IsWarming)
		{
			AudioEvent aEvent = new AudioEvent();
			PerformSingleAction(eventGrp, aEvent, eType);
			return true;
		}
		for (int i = 0; i < eventGrp.SoundEvents.Count; i++)
		{
			PerformSingleAction(eventGrp, eventGrp.SoundEvents[i], eType);
		}
		return true;
	}

	private void OnDrawGizmos()
	{
		if (showGizmo)
		{
			Gizmos.DrawIcon(base.transform.position, "MasterAudio/MasterAudio Icon.png", allowScaling: true);
		}
	}

	private static bool CheckForRetriggerLimit(AudioEventGroup grp)
	{
		switch (grp.retriggerLimitMode)
		{
		case RetriggerLimMode.FrameBased:
			if (grp.triggeredLastFrame > 0 && AudioUtil.FrameCount - grp.triggeredLastFrame < grp.limitPerXFrm)
			{
				return false;
			}
			break;
		case RetriggerLimMode.TimeBased:
			if (grp.triggeredLastTime > 0f && AudioUtil.Time - grp.triggeredLastTime < grp.limitPerXSec)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private void PerformSingleAction(AudioEventGroup grp, AudioEvent aEvent, EventType eType)
	{
		if (disableSounds || MasterAudio.AppIsShuttingDown || MasterAudio.SafeInstance == null)
		{
			return;
		}
		bool flag = eType == EventType.UnitySliderChanged && aEvent.targetVolMode == AudioEvent.TargetVolumeMode.UseSliderValue;
		float volumePercentage = aEvent.volume;
		string soundType = aEvent.soundType;
		float? pitch = aEvent.pitch;
		if (!aEvent.useFixedPitch)
		{
			pitch = null;
		}
		PlaySoundResult playSoundResult = null;
		MasterAudio.SoundSpawnLocationMode soundSpawnLocationMode = soundSpawnMode;
		if (eType == EventType.OnDisable || eType == EventType.OnDespawned)
		{
			soundSpawnLocationMode = MasterAudio.SoundSpawnLocationMode.CallerLocation;
		}
		bool flag2 = eType == EventType.OnMouseDrag || aEvent.glidePitchType != GlidePitchType.None;
		switch (aEvent.currentSoundFunctionType)
		{
		case MasterAudio.EventSoundFunctionType.PlaySound:
		{
			string variationName = null;
			if (aEvent.variationType == VariationType.PlaySpecific)
			{
				variationName = aEvent.variationName;
			}
			if (flag)
			{
				volumePercentage = grp.sliderValue;
			}
			switch (soundSpawnLocationMode)
			{
			case MasterAudio.SoundSpawnLocationMode.CallerLocation:
				if (flag2)
				{
					playSoundResult = MasterAudio.PlaySound3DAtTransform(soundType, _trans, volumePercentage, pitch, aEvent.delaySound, variationName);
				}
				else
				{
					MasterAudio.PlaySound3DAtTransformAndForget(soundType, _trans, volumePercentage, pitch, aEvent.delaySound, variationName);
				}
				break;
			case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
				if (flag2)
				{
					playSoundResult = MasterAudio.PlaySound3DFollowTransform(soundType, _trans, volumePercentage, pitch, aEvent.delaySound, variationName);
				}
				else
				{
					MasterAudio.PlaySound3DFollowTransformAndForget(soundType, _trans, volumePercentage, pitch, aEvent.delaySound, variationName);
				}
				break;
			case MasterAudio.SoundSpawnLocationMode.MasterAudioLocation:
				if (flag2)
				{
					playSoundResult = MasterAudio.PlaySound(soundType, volumePercentage, pitch, aEvent.delaySound, variationName);
				}
				else
				{
					MasterAudio.PlaySoundAndForget(soundType, volumePercentage, pitch, aEvent.delaySound, variationName);
				}
				break;
			}
			if (playSoundResult != null && playSoundResult.ActingVariation != null && aEvent.glidePitchType != 0)
			{
				switch (aEvent.glidePitchType)
				{
				case GlidePitchType.RaisePitch:
					playSoundResult.ActingVariation.GlideByPitch(aEvent.targetGlidePitch, aEvent.pitchGlideTime);
					break;
				case GlidePitchType.LowerPitch:
					playSoundResult.ActingVariation.GlideByPitch(0f - aEvent.targetGlidePitch, aEvent.pitchGlideTime);
					break;
				}
			}
			if (eType == EventType.OnMouseDrag)
			{
				_mouseDragResult = playSoundResult;
			}
			break;
		}
		case MasterAudio.EventSoundFunctionType.PlaylistControl:
			playSoundResult = new PlaySoundResult
			{
				ActingVariation = null,
				SoundPlayed = true,
				SoundScheduled = false
			};
			if (string.IsNullOrEmpty(aEvent.playlistControllerName))
			{
				aEvent.playlistControllerName = "~only~";
			}
			switch (aEvent.currentPlaylistCommand)
			{
			case MasterAudio.PlaylistCommand.None:
				playSoundResult.SoundPlayed = false;
				break;
			case MasterAudio.PlaylistCommand.Restart:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.RestartAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.RestartPlaylist(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.Start:
				if (!(aEvent.playlistControllerName == "[None]") && !(aEvent.playlistName == "[None]"))
				{
					MasterAudio.StartPlaylist(aEvent.playlistControllerName, aEvent.playlistName);
				}
				break;
			case MasterAudio.PlaylistCommand.ChangePlaylist:
				if (string.IsNullOrEmpty(aEvent.playlistName))
				{
					Debug.Log("You have not specified a Playlist name for Event Sounds on '" + _trans.name + "'.");
					playSoundResult.SoundPlayed = false;
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.ChangePlaylistByName(aEvent.playlistControllerName, aEvent.playlistName, aEvent.startPlaylist);
				}
				break;
			case MasterAudio.PlaylistCommand.StopLoopingCurrentSong:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.StopLoopingAllCurrentSongs();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.StopLoopingCurrentSong(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.StopPlaylistAfterCurrentSong:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.StopAllPlaylistsAfterCurrentSongs();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.StopPlaylistAfterCurrentSong(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.FadeToVolume:
			{
				float targetVolume = (flag ? grp.sliderValue : aEvent.fadeVolume);
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.FadeAllPlaylistsToVolume(targetVolume, aEvent.fadeTime);
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.FadePlaylistToVolume(aEvent.playlistControllerName, targetVolume, aEvent.fadeTime);
				}
				break;
			}
			case MasterAudio.PlaylistCommand.Mute:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.MuteAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.MutePlaylist(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.Unmute:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.UnmuteAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.UnmutePlaylist(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.ToggleMute:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.ToggleMuteAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.ToggleMutePlaylist(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.PlaySong:
				if (string.IsNullOrEmpty(aEvent.clipName))
				{
					Debug.Log("You have not specified a song name for Event Sounds on '" + _trans.name + "'.");
					playSoundResult.SoundPlayed = false;
				}
				else if (!(aEvent.playlistControllerName == "[None]") && !MasterAudio.TriggerPlaylistClip(aEvent.playlistControllerName, aEvent.clipName))
				{
					playSoundResult.SoundPlayed = false;
				}
				break;
			case MasterAudio.PlaylistCommand.AddSongToQueue:
				playSoundResult.SoundPlayed = false;
				if (string.IsNullOrEmpty(aEvent.clipName))
				{
					Debug.Log("You have not specified a song name for Event Sounds on '" + _trans.name + "'.");
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.QueuePlaylistClip(aEvent.playlistControllerName, aEvent.clipName);
					playSoundResult.SoundPlayed = true;
				}
				break;
			case MasterAudio.PlaylistCommand.PlayRandomSong:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.TriggerRandomClipAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.TriggerRandomPlaylistClip(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.PlayNextSong:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.TriggerNextClipAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.TriggerNextPlaylistClip(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.Pause:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.PauseAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.PausePlaylist(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.Stop:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.StopAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.StopPlaylist(aEvent.playlistControllerName);
				}
				break;
			case MasterAudio.PlaylistCommand.Resume:
				if (aEvent.allPlaylistControllersForGroupCmd)
				{
					MasterAudio.UnpauseAllPlaylists();
				}
				else if (!(aEvent.playlistControllerName == "[None]"))
				{
					MasterAudio.UnpausePlaylist(aEvent.playlistControllerName);
				}
				break;
			}
			break;
		case MasterAudio.EventSoundFunctionType.GroupControl:
		{
			playSoundResult = new PlaySoundResult
			{
				ActingVariation = null,
				SoundPlayed = true,
				SoundScheduled = false
			};
			List<string> list5 = new List<string>();
			if (!aEvent.allSoundTypesForGroupCmd || MasterAudio.GroupCommandsWithNoAllGroupSelector.Contains(aEvent.currentSoundGroupCommand))
			{
				list5.Add(aEvent.soundType);
			}
			else
			{
				list5.AddRange(MasterAudio.RuntimeSoundGroupNames);
			}
			for (int l = 0; l < list5.Count; l++)
			{
				string sType = list5[l];
				switch (aEvent.currentSoundGroupCommand)
				{
				case MasterAudio.SoundGroupCommand.None:
					playSoundResult.SoundPlayed = false;
					break;
				case MasterAudio.SoundGroupCommand.ToggleSoundGroup:
					if (MasterAudio.IsSoundGroupPlaying(sType))
					{
						MasterAudio.FadeOutAllOfSound(sType, aEvent.fadeTime);
						break;
					}
					switch (soundSpawnLocationMode)
					{
					case MasterAudio.SoundSpawnLocationMode.CallerLocation:
						MasterAudio.PlaySound3DAtTransformAndForget(soundType, _trans, volumePercentage, pitch, aEvent.delaySound);
						break;
					case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
						MasterAudio.PlaySound3DFollowTransformAndForget(soundType, _trans, volumePercentage, pitch, aEvent.delaySound);
						break;
					case MasterAudio.SoundSpawnLocationMode.MasterAudioLocation:
						MasterAudio.PlaySoundAndForget(soundType, volumePercentage, pitch, aEvent.delaySound);
						break;
					}
					break;
				case MasterAudio.SoundGroupCommand.ToggleSoundGroupOfTransform:
					if (MasterAudio.IsTransformPlayingSoundGroup(sType, _trans))
					{
						MasterAudio.FadeOutSoundGroupOfTransform(_trans, sType, aEvent.fadeTime);
						break;
					}
					switch (soundSpawnLocationMode)
					{
					case MasterAudio.SoundSpawnLocationMode.CallerLocation:
						MasterAudio.PlaySound3DAtTransformAndForget(soundType, _trans, volumePercentage, pitch, aEvent.delaySound);
						break;
					case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
						MasterAudio.PlaySound3DFollowTransformAndForget(soundType, _trans, volumePercentage, pitch, aEvent.delaySound);
						break;
					case MasterAudio.SoundSpawnLocationMode.MasterAudioLocation:
						MasterAudio.PlaySoundAndForget(soundType, volumePercentage, pitch, aEvent.delaySound);
						break;
					}
					break;
				case MasterAudio.SoundGroupCommand.RefillSoundGroupPool:
					MasterAudio.RefillSoundGroupPool(sType);
					break;
				case MasterAudio.SoundGroupCommand.FadeToVolume:
				{
					float newVolume = (flag ? grp.sliderValue : aEvent.fadeVolume);
					MasterAudio.FadeSoundGroupToVolume(sType, newVolume, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
					break;
				}
				case MasterAudio.SoundGroupCommand.FadeOutAllOfSound:
					MasterAudio.FadeOutAllOfSound(sType, aEvent.fadeTime);
					break;
				case MasterAudio.SoundGroupCommand.Mute:
					MasterAudio.MuteGroup(sType);
					break;
				case MasterAudio.SoundGroupCommand.Pause:
					MasterAudio.PauseSoundGroup(sType);
					break;
				case MasterAudio.SoundGroupCommand.Solo:
					MasterAudio.SoloGroup(sType);
					break;
				case MasterAudio.SoundGroupCommand.StopAllOfSound:
					MasterAudio.StopAllOfSound(sType);
					break;
				case MasterAudio.SoundGroupCommand.Unmute:
					MasterAudio.UnmuteGroup(sType);
					break;
				case MasterAudio.SoundGroupCommand.Unpause:
					MasterAudio.UnpauseSoundGroup(sType);
					break;
				case MasterAudio.SoundGroupCommand.Unsolo:
					MasterAudio.UnsoloGroup(sType);
					break;
				case MasterAudio.SoundGroupCommand.StopAllSoundsOfTransform:
					MasterAudio.StopAllSoundsOfTransform(_trans);
					break;
				case MasterAudio.SoundGroupCommand.StopSoundGroupOfTransform:
					MasterAudio.StopSoundGroupOfTransform(_trans, sType);
					break;
				case MasterAudio.SoundGroupCommand.PauseAllSoundsOfTransform:
					MasterAudio.PauseAllSoundsOfTransform(_trans);
					break;
				case MasterAudio.SoundGroupCommand.PauseSoundGroupOfTransform:
					MasterAudio.PauseSoundGroupOfTransform(_trans, sType);
					break;
				case MasterAudio.SoundGroupCommand.UnpauseAllSoundsOfTransform:
					MasterAudio.UnpauseAllSoundsOfTransform(_trans);
					break;
				case MasterAudio.SoundGroupCommand.UnpauseSoundGroupOfTransform:
					MasterAudio.UnpauseSoundGroupOfTransform(_trans, sType);
					break;
				case MasterAudio.SoundGroupCommand.FadeOutSoundGroupOfTransform:
					MasterAudio.FadeOutSoundGroupOfTransform(_trans, sType, aEvent.fadeTime);
					break;
				case MasterAudio.SoundGroupCommand.FadeOutAllSoundsOfTransform:
					MasterAudio.FadeOutAllSoundsOfTransform(_trans, aEvent.fadeTime);
					break;
				case MasterAudio.SoundGroupCommand.RouteToBus:
				{
					string text = aEvent.busName;
					if (text == "[None]")
					{
						text = null;
					}
					MasterAudio.RouteGroupToBus(sType, text);
					break;
				}
				case MasterAudio.SoundGroupCommand.GlideByPitch:
					switch (aEvent.glidePitchType)
					{
					case GlidePitchType.RaisePitch:
						MasterAudio.GlideSoundGroupByPitch(sType, aEvent.targetGlidePitch, aEvent.pitchGlideTime);
						break;
					case GlidePitchType.LowerPitch:
						MasterAudio.GlideSoundGroupByPitch(sType, 0f - aEvent.targetGlidePitch, aEvent.pitchGlideTime);
						break;
					}
					break;
				}
			}
			break;
		}
		case MasterAudio.EventSoundFunctionType.BusControl:
		{
			playSoundResult = new PlaySoundResult
			{
				ActingVariation = null,
				SoundPlayed = true,
				SoundScheduled = false
			};
			List<string> list6 = new List<string>();
			if (!aEvent.allSoundTypesForBusCmd)
			{
				list6.Add(aEvent.busName);
			}
			else
			{
				list6.AddRange(MasterAudio.RuntimeBusNames);
			}
			for (int m = 0; m < list6.Count; m++)
			{
				string busName2 = list6[m];
				switch (aEvent.currentBusCommand)
				{
				case MasterAudio.BusCommand.None:
					playSoundResult.SoundPlayed = false;
					break;
				case MasterAudio.BusCommand.FadeToVolume:
				{
					float newVolume2 = (flag ? grp.sliderValue : aEvent.fadeVolume);
					MasterAudio.FadeBusToVolume(busName2, newVolume2, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
					break;
				}
				case MasterAudio.BusCommand.GlideByPitch:
					switch (aEvent.glidePitchType)
					{
					case GlidePitchType.RaisePitch:
						MasterAudio.GlideBusByPitch(busName2, aEvent.targetGlidePitch, aEvent.pitchGlideTime);
						break;
					case GlidePitchType.LowerPitch:
						MasterAudio.GlideBusByPitch(busName2, 0f - aEvent.targetGlidePitch, aEvent.pitchGlideTime);
						break;
					}
					break;
				case MasterAudio.BusCommand.Pause:
					MasterAudio.PauseBus(busName2);
					break;
				case MasterAudio.BusCommand.Stop:
					MasterAudio.StopBus(busName2);
					break;
				case MasterAudio.BusCommand.Unpause:
					MasterAudio.UnpauseBus(busName2);
					break;
				case MasterAudio.BusCommand.Mute:
					MasterAudio.MuteBus(busName2);
					break;
				case MasterAudio.BusCommand.Unmute:
					MasterAudio.UnmuteBus(busName2);
					break;
				case MasterAudio.BusCommand.ToggleMute:
					MasterAudio.ToggleMuteBus(busName2);
					break;
				case MasterAudio.BusCommand.Solo:
					MasterAudio.SoloBus(busName2);
					break;
				case MasterAudio.BusCommand.Unsolo:
					MasterAudio.UnsoloBus(busName2);
					break;
				case MasterAudio.BusCommand.ChangePitch:
					MasterAudio.ChangeBusPitch(busName2, aEvent.pitch);
					break;
				case MasterAudio.BusCommand.PauseBusOfTransform:
					MasterAudio.PauseBusOfTransform(_trans, aEvent.busName);
					break;
				case MasterAudio.BusCommand.UnpauseBusOfTransform:
					MasterAudio.UnpauseBusOfTransform(_trans, aEvent.busName);
					break;
				case MasterAudio.BusCommand.StopBusOfTransform:
					MasterAudio.StopBusOfTransform(_trans, aEvent.busName);
					break;
				}
			}
			break;
		}
		case MasterAudio.EventSoundFunctionType.CustomEventControl:
			if (eType == EventType.UserDefinedEvent)
			{
				Debug.LogError("Custom Event Receivers cannot fire events. Occured in Transform '" + base.name + "'.");
			}
			else if (aEvent.currentCustomEventCommand == MasterAudio.CustomEventCommand.FireEvent)
			{
				MasterAudio.FireCustomEvent(aEvent.theCustomEventName, _trans);
			}
			break;
		case MasterAudio.EventSoundFunctionType.GlobalControl:
			switch (aEvent.currentGlobalCommand)
			{
			case MasterAudio.GlobalCommand.SetMasterMixerVolume:
				MasterAudio.MasterVolumeLevel = (flag ? grp.sliderValue : aEvent.volume);
				break;
			case MasterAudio.GlobalCommand.SetMasterPlaylistVolume:
				MasterAudio.PlaylistMasterVolume = (flag ? grp.sliderValue : aEvent.volume);
				break;
			case MasterAudio.GlobalCommand.PauseMixer:
				MasterAudio.PauseMixer();
				break;
			case MasterAudio.GlobalCommand.UnpauseMixer:
				MasterAudio.UnpauseMixer();
				break;
			case MasterAudio.GlobalCommand.StopMixer:
				MasterAudio.StopMixer();
				break;
			case MasterAudio.GlobalCommand.MuteEverything:
				MasterAudio.MuteEverything();
				break;
			case MasterAudio.GlobalCommand.UnmuteEverything:
				MasterAudio.UnmuteEverything();
				break;
			case MasterAudio.GlobalCommand.PauseEverything:
				MasterAudio.PauseEverything();
				break;
			case MasterAudio.GlobalCommand.UnpauseEverything:
				MasterAudio.UnpauseEverything();
				break;
			case MasterAudio.GlobalCommand.StopEverything:
				MasterAudio.StopEverything();
				break;
			}
			break;
		case MasterAudio.EventSoundFunctionType.UnityMixerControl:
			switch (aEvent.currentMixerCommand)
			{
			case MasterAudio.UnityMixerCommand.TransitionToSnapshot:
			{
				AudioMixerSnapshot snapshotToTransitionTo = aEvent.snapshotToTransitionTo;
				if (snapshotToTransitionTo != null)
				{
					snapshotToTransitionTo.audioMixer.TransitionToSnapshots(new AudioMixerSnapshot[1] { snapshotToTransitionTo }, new float[1] { 1f }, aEvent.snapshotTransitionTime);
				}
				break;
			}
			case MasterAudio.UnityMixerCommand.TransitionToSnapshotBlend:
			{
				List<AudioMixerSnapshot> list3 = new List<AudioMixerSnapshot>();
				List<float> list4 = new List<float>();
				AudioMixer audioMixer = null;
				for (int k = 0; k < aEvent.snapshotsToBlend.Count; k++)
				{
					AudioEvent.MA_SnapshotInfo mA_SnapshotInfo = aEvent.snapshotsToBlend[k];
					if (!(mA_SnapshotInfo.snapshot == null))
					{
						if (audioMixer == null)
						{
							audioMixer = mA_SnapshotInfo.snapshot.audioMixer;
						}
						else if (audioMixer != mA_SnapshotInfo.snapshot.audioMixer)
						{
							Debug.LogError("Snapshot '" + mA_SnapshotInfo.snapshot.name + "' isn't in the same Audio Mixer as the previous snapshot in EventSounds on GameObject '" + base.name + "'. Please make sure all the Snapshots to blend are on the same mixer.");
							break;
						}
						list3.Add(mA_SnapshotInfo.snapshot);
						list4.Add(mA_SnapshotInfo.weight);
					}
				}
				if (list3.Count > 0)
				{
					Debug.Log("trans");
					audioMixer.TransitionToSnapshots(list3.ToArray(), list4.ToArray(), aEvent.snapshotTransitionTime);
				}
				break;
			}
			}
			break;
		case MasterAudio.EventSoundFunctionType.PersistentSettingsControl:
			switch (aEvent.currentPersistentSettingsCommand)
			{
			case MasterAudio.PersistentSettingsCommand.SetBusVolume:
			{
				List<string> list2 = new List<string>();
				if (!aEvent.allSoundTypesForBusCmd)
				{
					list2.Add(aEvent.busName);
				}
				else
				{
					list2.AddRange(MasterAudio.RuntimeBusNames);
				}
				for (int j = 0; j < list2.Count; j++)
				{
					string busName = list2[j];
					float vol2 = (flag ? grp.sliderValue : aEvent.volume);
					PersistentAudioSettings.SetBusVolume(busName, vol2);
				}
				break;
			}
			case MasterAudio.PersistentSettingsCommand.SetGroupVolume:
			{
				List<string> list = new List<string>();
				if (!aEvent.allSoundTypesForGroupCmd)
				{
					list.Add(aEvent.soundType);
				}
				else
				{
					list.AddRange(MasterAudio.RuntimeSoundGroupNames);
				}
				for (int i = 0; i < list.Count; i++)
				{
					string grpName = list[i];
					float vol = (flag ? grp.sliderValue : aEvent.volume);
					PersistentAudioSettings.SetGroupVolume(grpName, vol);
				}
				break;
			}
			case MasterAudio.PersistentSettingsCommand.SetMixerVolume:
				PersistentAudioSettings.MixerVolume = (flag ? grp.sliderValue : aEvent.volume);
				break;
			case MasterAudio.PersistentSettingsCommand.SetMusicVolume:
				PersistentAudioSettings.MusicVolume = (flag ? grp.sliderValue : aEvent.volume);
				break;
			case MasterAudio.PersistentSettingsCommand.MixerMuteToggle:
				if (PersistentAudioSettings.MixerMuted.HasValue)
				{
					PersistentAudioSettings.MixerMuted = !PersistentAudioSettings.MixerMuted.Value;
				}
				else
				{
					PersistentAudioSettings.MixerMuted = true;
				}
				break;
			case MasterAudio.PersistentSettingsCommand.MusicMuteToggle:
				if (PersistentAudioSettings.MusicMuted.HasValue)
				{
					PersistentAudioSettings.MusicMuted = !PersistentAudioSettings.MusicMuted.Value;
				}
				else
				{
					PersistentAudioSettings.MusicMuted = true;
				}
				break;
			}
			break;
		}
	}

	private void LogIfCustomEventMissing(AudioEventGroup eventGroup)
	{
		if (!logMissingEvents || (eventGroup.isCustomEvent && (!eventGroup.customSoundActive || string.IsNullOrEmpty(eventGroup.customEventName))))
		{
			return;
		}
		for (int i = 0; i < eventGroup.SoundEvents.Count; i++)
		{
			AudioEvent audioEvent = eventGroup.SoundEvents[i];
			if (audioEvent.currentSoundFunctionType == MasterAudio.EventSoundFunctionType.CustomEventControl)
			{
				string theCustomEventName = audioEvent.theCustomEventName;
				if (!MasterAudio.CustomEventExists(theCustomEventName))
				{
					MasterAudio.LogWarning("Transform '" + base.name + "' is set up to receive or fire Custom Event '" + theCustomEventName + "', which does not exist in Master Audio.");
				}
			}
		}
	}

	public void CheckForIllegalCustomEvents()
	{
		if (useStartSound)
		{
			LogIfCustomEventMissing(startSound);
		}
		if (useVisibleSound)
		{
			LogIfCustomEventMissing(visibleSound);
		}
		if (useInvisibleSound)
		{
			LogIfCustomEventMissing(invisibleSound);
		}
		if (useCollisionSound)
		{
			LogIfCustomEventMissing(collisionSound);
		}
		if (useCollisionExitSound)
		{
			LogIfCustomEventMissing(collisionExitSound);
		}
		if (useTriggerEnterSound)
		{
			LogIfCustomEventMissing(triggerSound);
		}
		if (useTriggerExitSound)
		{
			LogIfCustomEventMissing(triggerExitSound);
		}
		if (useMouseEnterSound)
		{
			LogIfCustomEventMissing(mouseEnterSound);
		}
		if (useMouseExitSound)
		{
			LogIfCustomEventMissing(mouseExitSound);
		}
		if (useMouseClickSound)
		{
			LogIfCustomEventMissing(mouseClickSound);
		}
		if (useMouseDragSound)
		{
			LogIfCustomEventMissing(mouseDragSound);
		}
		if (useMouseUpSound)
		{
			LogIfCustomEventMissing(mouseUpSound);
		}
		if (useNguiMouseDownSound)
		{
			LogIfCustomEventMissing(nguiMouseDownSound);
		}
		if (useNguiMouseUpSound)
		{
			LogIfCustomEventMissing(nguiMouseUpSound);
		}
		if (useNguiOnClickSound)
		{
			LogIfCustomEventMissing(nguiOnClickSound);
		}
		if (useNguiMouseEnterSound)
		{
			LogIfCustomEventMissing(nguiMouseEnterSound);
		}
		if (useNguiMouseExitSound)
		{
			LogIfCustomEventMissing(nguiMouseExitSound);
		}
		if (useSpawnedSound)
		{
			LogIfCustomEventMissing(spawnedSound);
		}
		if (useDespawnedSound)
		{
			LogIfCustomEventMissing(despawnedSound);
		}
		if (useEnableSound)
		{
			LogIfCustomEventMissing(enableSound);
		}
		if (useDisableSound)
		{
			LogIfCustomEventMissing(disableSound);
		}
		if (useCollision2dSound)
		{
			LogIfCustomEventMissing(collision2dSound);
		}
		if (useCollisionExit2dSound)
		{
			LogIfCustomEventMissing(collisionExit2dSound);
		}
		if (useTriggerEnter2dSound)
		{
			LogIfCustomEventMissing(triggerEnter2dSound);
		}
		if (useTriggerExit2dSound)
		{
			LogIfCustomEventMissing(triggerExit2dSound);
		}
		if (useParticleCollisionSound)
		{
			LogIfCustomEventMissing(particleCollisionSound);
		}
		if (useUnitySliderChangedSound)
		{
			LogIfCustomEventMissing(unitySliderChangedSound);
		}
		if (useUnityButtonClickedSound)
		{
			LogIfCustomEventMissing(unityButtonClickedSound);
		}
		if (useUnityPointerDownSound)
		{
			LogIfCustomEventMissing(unityPointerDownSound);
		}
		if (useUnityDragSound)
		{
			LogIfCustomEventMissing(unityDragSound);
		}
		if (useUnityDropSound)
		{
			LogIfCustomEventMissing(unityDropSound);
		}
		if (useUnityPointerUpSound)
		{
			LogIfCustomEventMissing(unityPointerUpSound);
		}
		if (useUnityPointerEnterSound)
		{
			LogIfCustomEventMissing(unityPointerEnterSound);
		}
		if (useUnityPointerExitSound)
		{
			LogIfCustomEventMissing(unityPointerExitSound);
		}
		if (useUnityScrollSound)
		{
			LogIfCustomEventMissing(unityScrollSound);
		}
		if (useUnityUpdateSelectedSound)
		{
			LogIfCustomEventMissing(unityUpdateSelectedSound);
		}
		if (useUnitySelectSound)
		{
			LogIfCustomEventMissing(unitySelectSound);
		}
		if (useUnityDeselectSound)
		{
			LogIfCustomEventMissing(unityDeselectSound);
		}
		if (useUnityMoveSound)
		{
			LogIfCustomEventMissing(unityMoveSound);
		}
		if (useUnityInitializePotentialDragSound)
		{
			LogIfCustomEventMissing(unityInitializePotentialDragSound);
		}
		if (useUnityBeginDragSound)
		{
			LogIfCustomEventMissing(unityBeginDragSound);
		}
		if (useUnityEndDragSound)
		{
			LogIfCustomEventMissing(unityEndDragSound);
		}
		if (useUnitySubmitSound)
		{
			LogIfCustomEventMissing(unitySubmitSound);
		}
		if (useUnityCancelSound)
		{
			LogIfCustomEventMissing(unityCancelSound);
		}
		for (int i = 0; i < userDefinedSounds.Count; i++)
		{
			AudioEventGroup eventGroup = userDefinedSounds[i];
			LogIfCustomEventMissing(eventGroup);
		}
	}

	public void ReceiveEvent(string customEventName, Vector3 originPoint)
	{
		for (int i = 0; i < userDefinedSounds.Count; i++)
		{
			AudioEventGroup audioEventGroup = userDefinedSounds[i];
			if (audioEventGroup.customSoundActive && !string.IsNullOrEmpty(audioEventGroup.customEventName) && audioEventGroup.customEventName.Equals(customEventName))
			{
				PlaySounds(audioEventGroup, EventType.UserDefinedEvent);
			}
		}
	}

	public bool SubscribesToEvent(string customEventName)
	{
		for (int i = 0; i < userDefinedSounds.Count; i++)
		{
			AudioEventGroup audioEventGroup = userDefinedSounds[i];
			if (audioEventGroup.customSoundActive && !string.IsNullOrEmpty(audioEventGroup.customEventName) && audioEventGroup.customEventName.Equals(customEventName))
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterReceiver()
	{
		if (userDefinedSounds.Count > 0)
		{
			MasterAudio.AddCustomEventReceiver(this, _trans);
		}
	}

	public void UnregisterReceiver()
	{
		if (userDefinedSounds.Count > 0)
		{
			MasterAudio.RemoveCustomEventReceiver(this);
		}
	}

	public IList<AudioEventGroup> GetAllEvents()
	{
		return userDefinedSounds.AsReadOnly();
	}

	private void AddUGUIComponents()
	{
		AddUGUIHandler<EventSoundsPointerEnterHandler>(useUnityPointerEnterSound);
		AddUGUIHandler<EventSoundsPointerExitHandler>(useUnityPointerExitSound);
		AddUGUIHandler<EventSoundsPointerDownHandler>(useUnityPointerDownSound);
		AddUGUIHandler<EventSoundsPointerUpHandler>(useUnityPointerUpSound);
		AddUGUIHandler<EventSoundsDragHandler>(useUnityDragSound);
		AddUGUIHandler<EventSoundsDropHandler>(useUnityDropSound);
		AddUGUIHandler<EventSoundsScrollHandler>(useUnityScrollSound);
		AddUGUIHandler<EventSoundsUpdateSelectedHandler>(useUnityUpdateSelectedSound);
		AddUGUIHandler<EventSoundsSelectHandler>(useUnitySelectSound);
		AddUGUIHandler<EventSoundsDeselectHandler>(useUnityDeselectSound);
		AddUGUIHandler<EventSoundsMoveHandler>(useUnityMoveSound);
		AddUGUIHandler<EventSoundsInitializePotentialDragHandler>(useUnityInitializePotentialDragSound);
		AddUGUIHandler<EventSoundsBeginDragHandler>(useUnityBeginDragSound);
		AddUGUIHandler<EventSoundsEndDragHandler>(useUnityEndDragSound);
		AddUGUIHandler<EventSoundsSubmitHandler>(useUnitySubmitSound);
		AddUGUIHandler<EventSoundsCancelHandler>(useUnityCancelSound);
	}

	private void AddUGUIHandler<T>(bool useSound) where T : EventSoundsUGUIHandler
	{
		if (useSound)
		{
			base.gameObject.AddComponent<T>().eventSounds = this;
		}
	}
}
