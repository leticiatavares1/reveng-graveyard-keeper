using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AnimationComponentBase : MonoBehaviour
{
	public enum SteppedRotationPresetType
	{
		BasicNpc,
		Donkey
	}

	private const float RAYCAST_DISTANCE = 1f;

	protected const string ANIM_STATE_ID = "State";

	protected const string MOVEMENT_ANIM_DIRECTION_ID = "Direction";

	protected const string WALK_SPEED = "walk_speed";

	public static readonly string ATTACK_BLOCK_TRIGGER = "doBlock";

	public static readonly string RESET_TO_IDLE_TRIGGER = "force_static";

	public static readonly int idStateAnimator = Animator.StringToHash("State");

	public static readonly int idDirectionAnimator = Animator.StringToHash("Direction");

	private static readonly int walkSpeedMultiplier = Animator.StringToHash("walk_speed");

	[SerializeField]
	private SteppedRotationPresetType steppedRotationPresetType;

	[SerializeField]
	protected Animator animator;

	[FormerlySerializedAs("playerDropView")]
	[SerializeField]
	[Space]
	protected DropViewAtomMesh dropView;

	[FormerlySerializedAs("playerDropViewParent")]
	[SerializeField]
	protected GameObject dropViewParent;

	[SerializeField]
	protected bool hasBlockAnimation;

	[SerializeField]
	[Space]
	[Tooltip("Primary reset mechanism. On disable, this clip is sampled at t=0 via AnimationClip.SampleAnimation, applying its curves to the hierarchy and reverting any animation-driven state (IsActive, transforms, material props, etc.) to the values captured when the 'Generate Reset Clip' button was pressed. If the animator uses an AnimatorOverrideController that overrides this clip, the overridden clip is sampled at runtime. When set, supersedes Reset Animator On Disable.")]
	protected AnimationClip animatorResetClip;

	[SerializeField]
	[Tooltip("Fallback: when no Animator Reset Clip is assigned and this is enabled, animator.WriteDefaultValues() is called on disable to revert animated properties to their bind-time values. Note: only useful when the prefab-authored state of the animated properties is the desired resting state. Ignored when Animator Reset Clip is assigned.")]
	protected bool resetAnimatorOnDisable;

	private AnimationClip cachedResolvedResetClip;

	private RuntimeAnimatorController cachedResolvedAgainstController;

	private AnimationClip cachedResolvedAgainstBaseClip;

	private SteppedRotationPreset steppedRotationPreset;

	protected AnimationState animationState;

	protected SkinPresetGK2 skinPreset;

	protected AnimationEventReceiver animationEventReceiver;

	private SurfaceType currentSurfaceType;

	private string stepSoundId = string.Empty;

	private int soundTimerId;

	private static RaycastHit[] raycastHitsBuffer = new RaycastHit[8];

	private float computedAngle;

	private SoundHandler bowSoundHandler;

	private SoundHandler idleSoundHandler;

	protected bool isInitialized;

	[NonSerialized]
	private readonly List<DropViewAtomMesh> extraOverheadDropViews = new List<DropViewAtomMesh>();

	public Animator Animator => animator;

	public Vector3 OverheadItemWorldPosition
	{
		get
		{
			if (!(dropView != null))
			{
				return base.transform.position;
			}
			return dropView.transform.position;
		}
	}

	public AnimationState AnimationState => animationState;

	public SkinPresetGK2 SkinPreset => skinPreset;

	public bool HasBlockAnimation => hasBlockAnimation;

	public AnimationEventReceiver AnimationEventReceiver => animationEventReceiver;

	private SteppedRotationPreset SteppedRotationPreset
	{
		get
		{
			if (steppedRotationPreset == null)
			{
				return BasicNpcSteppedRotationPreset.Instance;
			}
			return steppedRotationPreset;
		}
	}

	public event Action<ItemType> OnToolLoopStarted;

	public event Action<ItemType> OnToolLoopFinished;

	public virtual void Init(string visualId)
	{
		if (skinPreset == null)
		{
			skinPreset = SkinPresetGK2.LoadAsset(visualId);
		}
		if (skinPreset == null)
		{
			Debug.LogError("Failed to load skin preset: " + visualId);
		}
		else
		{
			InitInternal(skinPreset);
		}
	}

	public virtual void Init(SkinPresetGK2 skinPreset)
	{
		InitInternal(skinPreset);
	}

	public void SetSkinPreset(SkinPresetGK2 skinPreset)
	{
		this.skinPreset = skinPreset;
	}

	public virtual void InitWithSkinOrApplyCurrentSkin(string skinPreset)
	{
		Init(skinPreset);
	}

	public virtual void InitWithSkinOrApplySkin(string skinPreset)
	{
		Init(skinPreset);
	}

	public void SetState(AnimationState animationState)
	{
		this.animationState = animationState;
		animator.SetInteger(idStateAnimator, (int)animationState);
	}

	public AnimationState GetState()
	{
		return (AnimationState)animator.GetInteger(idStateAnimator);
	}

	public void SetTrigger(string trigger)
	{
		animator.SetTrigger(trigger);
	}

	public void SetTrigger(int trigger)
	{
		animator.SetTrigger(trigger);
	}

	public float SetDirection(Vector2 direction)
	{
		float num = SteppedRotationPreset.ComputeAngle(direction);
		if (!animator.isActiveAndEnabled)
		{
			return 0f;
		}
		animator.SetFloat(idDirectionAnimator, num);
		return num;
	}

	public void SetDirection(Direction direction)
	{
		if (animator.isActiveAndEnabled)
		{
			animator.SetFloat(idDirectionAnimator, SteppedRotationPreset.GetSteppedDirection(direction));
		}
	}

	public Vector2 GetDirection()
	{
		float @float = animator.GetFloat(idDirectionAnimator);
		return new Vector2(Mathf.Cos(@float * (MathF.PI / 180f)), Mathf.Sin(@float * (MathF.PI / 180f)));
	}

	public void SetWalkAnimationSpeedMultiplier(float speed)
	{
		animator.SetFloat(walkSpeedMultiplier, speed / 1.5f);
	}

	public void SetOverheadItem(Item item)
	{
		ApplyOverheadItemToView(dropView, item);
	}

	public void SetOverheadItems(IReadOnlyList<Item> items)
	{
		if (items == null || items.Count == 0)
		{
			RemoveOverheadItem();
			return;
		}
		ApplyOverheadItemToView(dropView, items[0]);
		EnsureExtraOverheadViews(items.Count - 1);
		ApplyOverheadStackSorting(dropView, 0);
		for (int i = 1; i < items.Count; i++)
		{
			DropViewAtomMesh dropViewAtomMesh = extraOverheadDropViews[i - 1];
			dropViewAtomMesh.transform.localPosition = LazyConsts.OVERHEAD_STACK_OFFSET * i;
			dropViewAtomMesh.transform.localRotation = Quaternion.identity;
			dropViewAtomMesh.transform.localScale = Vector3.one;
			ApplyOverheadItemToView(dropViewAtomMesh, items[i]);
			ApplyOverheadStackSorting(dropViewAtomMesh, i);
		}
		for (int j = items.Count - 1; j < extraOverheadDropViews.Count; j++)
		{
			if (!(extraOverheadDropViews[j] == null))
			{
				extraOverheadDropViews[j].Deactivate();
				extraOverheadDropViews[j].gameObject.SetActive(value: false);
			}
		}
	}

	public void RemoveOverheadItem()
	{
		if (dropView != null)
		{
			dropView.Deactivate();
		}
		for (int i = 0; i < extraOverheadDropViews.Count; i++)
		{
			if (!(extraOverheadDropViews[i] == null))
			{
				extraOverheadDropViews[i].Deactivate();
				extraOverheadDropViews[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void ApplyOverheadItemToView(DropViewAtomMesh view, Item item)
	{
		if (view == null || item == null || item.IsEmpty)
		{
			return;
		}
		view.isOverhead = true;
		if (item.Definition.isLinkedToWgo)
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(item.UniqueId);
			if (zombie != null)
			{
				SkinPresetGK2 presetForWgoData = ZombieSkinHelper.GetPresetForWgoData(zombie, "zombie_worker");
				if (presetForWgoData != null)
				{
					view.ActivateZombie(presetForWgoData.head.id.ToString("D4"), presetForWgoData.head.palette);
					return;
				}
			}
		}
		view.Activate(item.Definition.iconId);
		view.SetInteractionState(isUnderInteraction: false);
	}

	private static void ApplyOverheadStackSorting(DropViewAtomMesh view, int stackIndex)
	{
		if (!(view == null))
		{
			Renderer[] componentsInChildren = view.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].sortingOrder = stackIndex;
			}
		}
	}

	private void EnsureExtraOverheadViews(int count)
	{
		if (!(dropView == null))
		{
			while (extraOverheadDropViews.Count < count)
			{
				DropViewAtomMesh dropViewAtomMesh = UnityEngine.Object.Instantiate((extraOverheadDropViews.Count > 0) ? extraOverheadDropViews[0] : dropView, dropView.transform.parent);
				dropViewAtomMesh.name = $"{dropView.name}_stack_{extraOverheadDropViews.Count}";
				dropViewAtomMesh.transform.SetParent(dropView.transform, worldPositionStays: false);
				dropViewAtomMesh.transform.localRotation = Quaternion.identity;
				dropViewAtomMesh.transform.localScale = Vector3.one;
				dropViewAtomMesh.gameObject.SetActive(value: false);
				extraOverheadDropViews.Add(dropViewAtomMesh);
			}
		}
	}

	public void DisableDropView()
	{
		if (dropViewParent != null)
		{
			dropViewParent.gameObject.SetActive(value: false);
		}
	}

	public void EnableDropView()
	{
		if (dropViewParent != null)
		{
			dropViewParent.gameObject.SetActive(value: true);
		}
	}

	public void SetLayerWeight(int layerIndex, float weight)
	{
		animator.SetLayerWeight(layerIndex, weight);
	}

	public float GetLayerWeight(int layerIndex)
	{
		return animator.GetLayerWeight(layerIndex);
	}

	public void PlaySound(string sound)
	{
		LazyAudio.PlayAtGameObject(sound, base.transform, SpatialType.sound3D);
	}

	public void PlaySoundIfVoiceOverNotEnabled(string sound)
	{
		if (!VoiceOverSettings.IsEnabled)
		{
			PlaySound(sound);
		}
	}

	public void OnUseToolAnimation(ToolComponent toolComponent)
	{
		if (toolComponent.IsActionActive)
		{
			PlayToolUseSound(toolComponent.ToolInUse.Definition.type);
		}
	}

	public void PlayToolUseSound(ItemType toolType)
	{
		string text = string.Empty;
		switch (toolType)
		{
		case ItemType.Axe:
			text = "tool_axe";
			break;
		case ItemType.Pickaxe:
			text = "tool_pickaxe";
			break;
		case ItemType.Shovel:
			text = "tool_shovel";
			break;
		case ItemType.Hammer:
			text = "tool_hammer";
			break;
		}
		if (text != string.Empty)
		{
			LazyAudio.PlayAtGameObject(text, base.transform, SpatialType.sound3D);
		}
	}

	public void OnPlantingAnimation()
	{
	}

	public void ShowFishingRope()
	{
		NpcFishingContainer componentInChildren = GetComponentInChildren<NpcFishingContainer>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: true);
		}
	}

	public void HideFishingRope()
	{
		NpcFishingContainer componentInChildren = GetComponentInChildren<NpcFishingContainer>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
	}

	public void OnFishingStart()
	{
		LazyAudio.PlayAtGameObject("fishing_start", base.transform, SpatialType.sound3D);
	}

	public void OnFishingCast()
	{
		LazyAudio.PlayAtGameObject("fishing_cast_swoosh", base.transform, SpatialType.sound3D);
	}

	public virtual void OnSpearAttack()
	{
		LazyAudio.PlayAtGameObject("spear_attack", base.transform, SpatialType.sound3D);
	}

	public virtual void OnBowAimStart()
	{
		CancelBowAimLoop();
		bowSoundHandler = LazyAudio.PlayAtGameObject("bow_aim_start", base.transform, SpatialType.sound3D);
		if (bowSoundHandler != null)
		{
			SoundHandler soundHandler = bowSoundHandler;
			soundHandler.OnSoundPlayed = (Action)Delegate.Combine(soundHandler.OnSoundPlayed, new Action(OnBowAimLoopStart));
		}
	}

	private void OnBowAimLoopStart()
	{
		CancelBowAimLoop();
		if (!(this == null))
		{
			bowSoundHandler = LazyAudio.PlayAtGameObject("bow_aim_loop", base.transform, SpatialType.sound3D);
		}
	}

	public void OnBowAimShot()
	{
		CancelBowAimLoop();
		bowSoundHandler = LazyAudio.PlayAtGameObject("bow_aim_shot", base.transform, SpatialType.sound3D);
	}

	public void OnAllyDeathAnimationStart()
	{
		FightingAgent componentInParent = GetComponentInParent<FightingAgent>();
		if (componentInParent != null)
		{
			if (componentInParent.FighterDef.id.StartsWith("npc_town_barracks_mercenary"))
			{
				LazyAudio.PlayAndForget("ally_death_human");
			}
			else if (componentInParent.FighterDef.id == "zmb_wild_mob_allie")
			{
				LazyAudio.PlayAndForget("ally_death_zombie");
			}
		}
	}

	public void CancelBowAimLoop()
	{
		if (bowSoundHandler != null)
		{
			bowSoundHandler.OnSoundPlayed = null;
			bowSoundHandler.Stop();
			bowSoundHandler = null;
		}
	}

	public void OnZombieAttack()
	{
		LazyAudio.PlayAtGameObject("zombie_attack", base.transform, SpatialType.sound3D);
	}

	public void StartZombieIdleSound()
	{
		if (idleSoundHandler != null && idleSoundHandler.IsActive)
		{
			return;
		}
		StopZombieIdleSound();
		idleSoundHandler = LazyAudio.PlayAtGameObject("zombie_idle", base.transform, SpatialType.sound3D, checkDelay: false);
		if (idleSoundHandler == null)
		{
			return;
		}
		SoundHandler soundHandler = idleSoundHandler;
		soundHandler.OnSoundPlayed = (Action)Delegate.Combine(soundHandler.OnSoundPlayed, (Action)delegate
		{
			if (this == null)
			{
				StopZombieIdleSound();
			}
			else if (animationState != 0 && animationState != AnimationState.Walk)
			{
				StopZombieIdleSound();
			}
			else
			{
				StartZombieIdleSound();
			}
		});
	}

	public void StopZombieIdleSound()
	{
		if (idleSoundHandler != null)
		{
			idleSoundHandler.OnSoundPlayed = null;
			idleSoundHandler.Stop();
			idleSoundHandler = null;
		}
	}

	public virtual void HandleLoopStarted(ToolAnimationSMB machineBehaviour)
	{
		this.OnToolLoopStarted?.Invoke(machineBehaviour.itemType);
	}

	public virtual void HandleLoopFinished(ToolAnimationSMB machineBehaviour)
	{
		this.OnToolLoopFinished?.Invoke(machineBehaviour.itemType);
	}

	public virtual void OnDeathAnimationFinished()
	{
	}

	public virtual void ResetForPool()
	{
		this.OnToolLoopStarted = null;
		this.OnToolLoopFinished = null;
		SetState(AnimationState.Idle);
	}

	protected virtual void OnDisable()
	{
		CancelBowAimLoop();
		StopZombieIdleSound();
		LazyTimer.Stop(soundTimerId);
		if (resetAnimatorOnDisable)
		{
			TryPlayResetClip();
		}
	}

	private AnimationClip ResolveResetClip()
	{
		if (animatorResetClip == null)
		{
			return null;
		}
		RuntimeAnimatorController runtimeAnimatorController = ((animator != null) ? animator.runtimeAnimatorController : null);
		if (cachedResolvedResetClip != null && cachedResolvedAgainstController == runtimeAnimatorController && cachedResolvedAgainstBaseClip == animatorResetClip)
		{
			return cachedResolvedResetClip;
		}
		cachedResolvedResetClip = ResolveOverrideOrBase(runtimeAnimatorController);
		cachedResolvedAgainstController = runtimeAnimatorController;
		cachedResolvedAgainstBaseClip = animatorResetClip;
		return cachedResolvedResetClip;
	}

	private AnimationClip ResolveOverrideOrBase(RuntimeAnimatorController controller)
	{
		if (controller is AnimatorOverrideController animatorOverrideController)
		{
			List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>(animatorOverrideController.overridesCount);
			animatorOverrideController.GetOverrides(list);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Key == animatorResetClip && list[i].Value != null)
				{
					return list[i].Value;
				}
			}
		}
		return animatorResetClip;
	}

	public virtual void OnAnimationStepCompleted()
	{
		SurfaceType surfaceType = GetSurfaceType();
		if (surfaceType == SurfaceType.None)
		{
			return;
		}
		if (surfaceType != currentSurfaceType)
		{
			currentSurfaceType = surfaceType;
			if (currentSurfaceType != 0)
			{
				stepSoundId = LazySingletonSO<SurfaceStepSoundSettings>.Instance.GetStepSoundIdForSurfaceType(currentSurfaceType);
			}
		}
		if (string.IsNullOrEmpty(stepSoundId))
		{
			return;
		}
		if (UnityEngine.Random.Range(0f, 100f) < (float)LazySingletonSO<SurfaceStepSoundSettings>.Instance.delayChance)
		{
			soundTimerId = LazyTimer.AddTimer(UnityEngine.Random.Range(LazySingletonSO<SurfaceStepSoundSettings>.Instance.minDelayTime, LazySingletonSO<SurfaceStepSoundSettings>.Instance.maxDelayTime), delegate
			{
				try
				{
					LazyAudio.PlayAtGameObject(stepSoundId, base.transform, SpatialType.sound3D);
				}
				catch (Exception)
				{
				}
			});
		}
		else
		{
			LazyAudio.PlayAtGameObject(stepSoundId, base.transform, SpatialType.sound3D);
		}
	}

	private SurfaceType GetSurfaceType()
	{
		float num = 0.01666667f;
		Vector3 origin = base.transform.position + Vector3.up * num;
		Vector3 normalized = Physics.gravity.normalized;
		int num2 = Physics.RaycastNonAlloc(new Ray(origin, normalized), raycastHitsBuffer, 1f, 2060);
		if (num2 > 0)
		{
			int num3 = int.MaxValue;
			IOrderedSurface orderedSurface = null;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit raycastHit = raycastHitsBuffer[i];
				IOrderedSurface component = raycastHit.collider.GetComponent<IOrderedSurface>();
				if (component != null && component.Depth < num3)
				{
					num3 = component.Depth;
					orderedSurface = component;
				}
			}
			if (orderedSurface != null)
			{
				return orderedSurface.SurfaceType;
			}
		}
		return SurfaceType.None;
	}

	protected virtual void InitInternal(SkinPresetGK2 skinPreset)
	{
		if (dropView != null)
		{
			dropView.gameObject.SetActive(value: false);
		}
		animationEventReceiver = GetComponent<AnimationEventReceiver>();
		switch (steppedRotationPresetType)
		{
		case SteppedRotationPresetType.BasicNpc:
			steppedRotationPreset = BasicNpcSteppedRotationPreset.Instance;
			break;
		case SteppedRotationPresetType.Donkey:
			steppedRotationPreset = DonkeySteppedRotationPreset.Instance;
			break;
		}
	}

	public void TryPlayResetClip()
	{
		AnimationClip animationClip = ResolveResetClip();
		if (animationClip != null)
		{
			animationClip.SampleAnimation(base.gameObject, 0f);
		}
		else if (animator != null)
		{
			animator.WriteDefaultValues();
		}
	}
}
