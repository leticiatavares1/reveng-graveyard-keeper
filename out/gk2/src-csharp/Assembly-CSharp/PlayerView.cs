using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;

public class PlayerView : MonoBehaviour, IBubbleDrawable
{
	[SerializeField]
	private Transform bubblePoint;

	[SerializeField]
	private Transform larryInPocketBubblePoint;

	[SerializeField]
	private Transform interactionItemPoint;

	[Space]
	[Header("Banner")]
	[SerializeField]
	private BannerView banner;

	[Space]
	[Header("Wisp")]
	[SerializeField]
	private WispController playerWisp;

	[SerializeField]
	private Transform wispTargetRightDirection;

	[SerializeField]
	private Transform wispTargetLeftDirection;

	[SerializeField]
	private Transform wispTargetUpDirection;

	[SerializeField]
	private Transform wispTargetDownDirection;

	[Space]
	[Header("Fishing line")]
	[SerializeField]
	private FishingContainer fishingContainer;

	[Space]
	[Header("Sermon")]
	[SerializeField]
	private Transform sermonContainer;

	[Space]
	[SerializeField]
	public Transform cylinderShadowcaster;

	[SerializeField]
	private PlayerAnimation customizationCharacter;

	[NonSerialized]
	public Vector3 cylinderShadowCasterScale;

	[SerializeField]
	private PlayerMovementAdjustComponent movementAdjustComponent;

	private int pixelSizeForPosRounding = 1;

	private UIInteractingItem interactingItem;

	private PlayerAnimation playerAnimation;

	private float cachedDirectionAngle;

	[SerializeField]
	private bool controllingWispView = true;

	private bool isInitialized;

	public bool ControllingWispView
	{
		get
		{
			return controllingWispView;
		}
		set
		{
			controllingWispView = value;
		}
	}

	public WispController WispController => playerWisp;

	public PlayerAnimation PlayerAnimation => playerAnimation;

	public PlayerAnimation CustomizationCharacter => customizationCharacter;

	public Transform BubblePoint => bubblePoint;

	public BannerView Banner => banner;

	public FishingContainer FishingContainer => fishingContainer;

	public Transform LarryInPocketBubblePoint => larryInPocketBubblePoint;

	private PlayerController PlayerController => MainGame.PlayerController;

	public Vector3 RoundedPosition => VisualConsts.GetRoundedPosXYZ(base.transform.parent.position, pixelSizeForPosRounding);

	private Object3DTransparencyOccluder transparencyOccluder => Object3DTransparencyOccluder.Shared;

	public SGuid BubbleDrawableUniqueId => MainGame.PlayerController.PlayerData.Guid;

	public List<LazyWidgetDataBase> BubbleDrawableWidgets
	{
		get
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			bool flag = LazySingleton<FightingGameController>.Instance.CurrentFightState != FightState.Disabled;
			if (!PlayerController.PlayerData.hpComponent.HasFullHp || flag)
			{
				list.Add(new HpBarPlayerWidgetData(MainGame.PlayerController.PlayerData.hpComponent));
			}
			if (flag)
			{
				list.Add(new StaminaBarPlayerWidgetData());
			}
			return list;
		}
	}

	public Vector3 BubbleDrawablePosition => bubblePoint.position;

	public event Action OnPlantingAnimationEvent;

	public event Action<ToolComponent> OnWorkActionAnimationEvent;

	public void Init()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			if (!TryGetComponent<PlayerAnimation>(out playerAnimation))
			{
				Debug.LogError("Player Animation Component doesn't found");
			}
			InitPlayerAnimation();
			OnWorkActionAnimationEvent += PlayerAnimation.OnUseToolAnimation;
			OnPlantingAnimationEvent += PlayerAnimation.OnPlantingAnimation;
			if ((bool)cylinderShadowcaster)
			{
				cylinderShadowCasterScale = cylinderShadowcaster.localScale;
			}
			HandleMainCameraRenderTypeChanged(CameraSystem.Instance.MainCamera.GetRenderType());
			MainCamera.OnRenderModeChanged += HandleMainCameraRenderTypeChanged;
			InitPlatformDependentElements();
			LazySingleton<Microphone>.Instance.SetTarget(base.transform);
		}
	}

	public void PrepareForGame(PlayerData playerData)
	{
		movementAdjustComponent.Init(PlayerController);
		LazySingleton<FightingGameController>.Instance.OnFightStateChanged += HandleFightStateChanged;
		RefreshFightBubbleWidgets();
	}

	public void UnPrepareFromGame()
	{
		movementAdjustComponent.DeInit();
		LazySingleton<FightingGameController>.Instance.OnFightStateChanged -= HandleFightStateChanged;
		PlayerAnimation.TryPlayResetClip();
	}

	public void UpdatePosition(Vector3 position)
	{
		base.transform.position = position;
	}

	public void UpdateAnimationDirection(Vector2 direction)
	{
		cachedDirectionAngle = PlayerAnimation.SetDirection(direction);
		UpdateWispDirection(cachedDirectionAngle);
	}

	public void UpdateWispDirection()
	{
		UpdateWispDirection(cachedDirectionAngle);
	}

	private void UpdateWispDirection(float angle)
	{
		if (controllingWispView)
		{
			switch (angle.ConvertFromSignedAngle())
			{
			case Direction.None:
				playerWisp.SetTargetTransform(wispTargetDownDirection);
				break;
			case Direction.Right:
				playerWisp.SetTargetTransform(wispTargetRightDirection);
				break;
			case Direction.Up:
				playerWisp.SetTargetTransform(wispTargetUpDirection);
				break;
			case Direction.Left:
				playerWisp.SetTargetTransform(wispTargetLeftDirection);
				break;
			case Direction.Down:
				playerWisp.SetTargetTransform(wispTargetDownDirection);
				break;
			}
		}
	}

	public Vector2 GetAnimationDirection()
	{
		return PlayerAnimation.GetDirection();
	}

	public void UpdateHpBarState(HPComponent hpComponent)
	{
		RefreshFightBubbleWidgets();
	}

	public void UpdateStaminaBarState()
	{
		RefreshFightBubbleWidgets();
	}

	public void Update()
	{
		if (!(BuildController.Instance != null) || !BuildController.Instance.IsBuildModeActive)
		{
			transparencyOccluder.BeginFrame();
			transparencyOccluder.AddOccludersFromCapsule(base.transform.position, base.transform.position + Vector3.up * 1.4f, 0.24f, 10f);
			transparencyOccluder.EndFrame();
		}
	}

	private void LateUpdate()
	{
		Vector3 roundedPosition = RoundedPosition;
		base.transform.position = roundedPosition;
	}

	private void OnDestroy()
	{
		OnWorkActionAnimationEvent -= PlayerAnimation.OnUseToolAnimation;
		OnPlantingAnimationEvent -= PlayerAnimation.OnPlantingAnimation;
		MainCamera.OnRenderModeChanged -= HandleMainCameraRenderTypeChanged;
		transparencyOccluder.Clear();
	}

	public void SendWorkActionAnimationEvent()
	{
		this.OnWorkActionAnimationEvent?.Invoke(PlayerController.PlayerWorkComponent.ToolComponent);
		PlayerController.PlayerWorkComponent.ToolComponent.OnUseToolActionAnimationEvent();
	}

	public void SendPlantingActionAnimationEvent()
	{
		this.OnPlantingAnimationEvent?.Invoke();
	}

	public void SetPlayerPreset(SkinPresetGK2 skinPreset, bool onlyForCustomizationCharacter)
	{
		if (!onlyForCustomizationCharacter)
		{
			playerAnimation.ChangeSkinPreset(skinPreset);
		}
		customizationCharacter.ChangeSkinPreset(skinPreset);
	}

	public void ApplyPlayerColors(Texture2D palette, List<CustomizablePartType> affectedPartTypes, bool onlyForCustomizationCharacter)
	{
		if (!onlyForCustomizationCharacter)
		{
			playerAnimation.ApplyPlayerColors(palette, affectedPartTypes);
		}
		customizationCharacter.ApplyPlayerColors(palette, affectedPartTypes);
	}

	private void InitPlayerAnimation()
	{
		playerAnimation.Init(PlayerSkinHelper.CurrentPreset);
	}

	public void SetInteractingItem(Item item, int totalCount)
	{
		if (interactingItem != null)
		{
			interactingItem.DisableBubble();
		}
		if (item != null)
		{
			interactingItem = UIInteractingItem.ShowInteractingItem(new Item(item.id, totalCount), interactionItemPoint);
			playerAnimation.DisableDropView();
		}
	}

	public void RemoveInteractingItem()
	{
		playerAnimation.EnableDropView();
		if (interactingItem != null)
		{
			interactingItem.DisableBubble();
		}
	}

	private void HandleMainCameraRenderTypeChanged(MainCamera.RenderMode renderType)
	{
		pixelSizeForPosRounding = ((renderType == MainCamera.RenderMode.Native) ? 1 : 2);
	}

	public void DisplayFishingRope()
	{
		fishingContainer.ResetColor();
		fishingContainer.gameObject.SetActive(value: true);
	}

	public void SetSermonContainerActive(bool isActive)
	{
		sermonContainer.gameObject.SetActive(isActive);
		LazyAudio.PlayAndForget(MainGame.PlayerData.currentSermon.success ? "sermon_success" : "sermon_fail");
	}

	public void SetSermonIcon(Sprite sprite)
	{
		sermonContainer.GetComponentsInChildren<SpriteRenderer>(includeInactive: true).ToList().ForEach(delegate(SpriteRenderer s)
		{
			s.sprite = sprite;
		});
	}

	public void FinishSermon(bool isSuccess, Action onAnimationFinished = null)
	{
		Animator component = sermonContainer.GetComponent<Animator>();
		if (component == null)
		{
			Debug.LogWarning("[PlayerView.FinishSermon]: sermonAnimator is null");
			return;
		}
		component.SetTrigger(isSuccess ? "success" : "fail");
		StartCoroutine(FinishSermonEnumerator(component, onAnimationFinished));
	}

	private IEnumerator FinishSermonEnumerator(Animator animator, Action onAnimationFinished = null)
	{
		yield return null;
		yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
		sermonContainer.gameObject.SetActive(value: false);
		onAnimationFinished?.Invoke();
	}

	private void HandleFightStateChanged(FightState fightState)
	{
		RefreshFightBubbleWidgets();
	}

	private void RefreshFightBubbleWidgets()
	{
		UIObjectBubbleManager.Instance.Display(this);
	}

	private void InitPlatformDependentElements()
	{
		LazyPlatformDependentElement[] componentsInChildren = GetComponentsInChildren<LazyPlatformDependentElement>(includeInactive: true);
		foreach (LazyPlatformDependentElement lazyPlatformDependentElement in componentsInChildren)
		{
			if (!lazyPlatformDependentElement.UseAwakeForInit)
			{
				lazyPlatformDependentElement.Init();
			}
		}
	}
}
