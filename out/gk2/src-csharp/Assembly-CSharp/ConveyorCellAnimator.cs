using LazyBearTechnology;
using UnityEngine;

public class ConveyorCellAnimator : ConveyorAnimator
{
	private static readonly int directionIn = Animator.StringToHash("directionIn");

	private static readonly int directionOut = Animator.StringToHash("directionOut");

	private static readonly int idleStateHash = Animator.StringToHash("Idle");

	private static readonly int inStateHash = Animator.StringToHash("In");

	private static readonly int outStateHash = Animator.StringToHash("Out");

	private static int itemScaleInLayerIndex = 2;

	private static int itemScaleOutLayerIndex = 3;

	[SerializeField]
	private bool isUndergroundCell;

	private Wgo parent;

	private bool shouldEnableScaleIn;

	private bool shouldEnableScaleOut;

	private bool playbackDirty = true;

	private float lastPlayedPhase = -1f;

	private int lastLayer0StateHash;

	private int lastLayer1StateHash;

	public override ConveyorSystemAnimatorType Type => ConveyorSystemAnimatorType.Cell;

	public Wgo Parent => parent ?? (parent = GetComponentInParent<Wgo>(includeInactive: true));

	public override void OnOrchestratorRegistered(string globalState, float phase)
	{
		SetupAnimatorForOrchestrator();
		MarkPlaybackDirty();
	}

	public override void MarkPlaybackDirty()
	{
		playbackDirty = true;
	}

	public override void TryRegister()
	{
		if (!(animator == null) && LazySingleton<ConveyorSystemAnimationOrchestrator>.Instance.TryAddAnimator(this))
		{
			MainGame.Instance.conveyorSystem.OnUpdated += UpdateAnimatorValues;
		}
	}

	public override void Unregister()
	{
		if (!(animator == null))
		{
			parent = null;
			LazySingleton<ConveyorSystemAnimationOrchestrator>.Instance.RemoveAnimator(this);
			MainGame.Instance.conveyorSystem.OnUpdated -= UpdateAnimatorValues;
		}
	}

	public override void CustomUpdate()
	{
		bool num = shouldEnableScaleIn && base.CurrentState == "In";
		bool flag = shouldEnableScaleOut && base.CurrentState == "Out";
		if (!num && !flag)
		{
			itemScale = Vector3.one;
		}
		if (isItemIdleLocked || base.CurrentState == "Idle")
		{
			itemRotation = Vector3.zero;
		}
		base.CustomUpdate();
	}

	public void UpdateAnimatorValues()
	{
		UpdateAnimatorValuesCommonCell();
	}

	private void UpdateScaleLayers(ConveyorWgoData current)
	{
		UpdateScaleLayersCommonCell(current);
	}

	private void SetLayerWeight(int layerIndex, bool enabled)
	{
		if (layerIndex >= 0 && layerIndex < animator.layerCount)
		{
			animator.SetLayerWeight(layerIndex, enabled ? 1f : 0f);
		}
	}

	public override void Play(float phase)
	{
		if (!(animator == null))
		{
			int stateHash = GetStateHash(base.CurrentState);
			int stateHash2 = GetStateHash(isItemIdleLocked ? "Idle" : base.CurrentState);
			float num = ((base.CurrentState == "Idle") ? 0f : phase);
			bool flag = stateHash == idleStateHash;
			bool num2 = playbackDirty || stateHash != lastLayer0StateHash || (!flag && !Mathf.Approximately(num, lastPlayedPhase));
			bool flag2 = playbackDirty || stateHash2 != lastLayer1StateHash || !Mathf.Approximately(phase, lastPlayedPhase);
			if (num2)
			{
				animator.Play(stateHash, 0, num);
			}
			if (flag2)
			{
				animator.Play(stateHash2, 1, phase);
			}
			if (num2 || flag2)
			{
				lastPlayedPhase = phase;
				lastLayer0StateHash = stateHash;
				lastLayer1StateHash = stateHash2;
				playbackDirty = false;
			}
		}
	}

	public override void UpdateView()
	{
		UpdateViewCommonCell();
	}

	private void UpdateAnimatorValuesCommonCell()
	{
		if (Parent.Data is ConveyorWgoData conveyorWgoData && !(animator == null))
		{
			ConveyorMovableItemData inItem = conveyorWgoData.ConveyorComponent.InItem;
			ConveyorMovableItemData outItem = conveyorWgoData.ConveyorComponent.OutItem;
			Direction direction = Direction.Left;
			switch (conveyorWgoData.MainWgoPartData.rotationIndex)
			{
			case 0:
				direction = Direction.Down;
				break;
			case 1:
				direction = Direction.Left;
				break;
			case 2:
				direction = Direction.Up;
				break;
			case 3:
				direction = Direction.Right;
				break;
			}
			Direction direction2 = inItem?.direction ?? direction;
			Direction direction3 = outItem?.direction ?? direction;
			animator.SetFloat(directionIn, (float)direction2);
			animator.SetFloat(directionOut, (float)direction3);
			UpdateScaleLayers(conveyorWgoData);
			bool flag = inItem == null && outItem == null && conveyorWgoData.Inventory.Data.Inventory.Count > 0;
			if (isItemIdleLocked != flag)
			{
				isItemIdleLocked = flag;
				MarkPlaybackDirty();
			}
			base.CurrentState = "Out";
		}
	}

	public void UpdateDropViewIn()
	{
		if (Parent.Data is ConveyorWgoData conveyorWgoData)
		{
			Parent.MainWgoPart.UpdateDropViewFromItemData(conveyorWgoData.ConveyorComponent.InItem);
		}
	}

	public void UpdateDropViewOut()
	{
		if (Parent.Data is ConveyorWgoData conveyorWgoData)
		{
			Parent.MainWgoPart.UpdateDropViewFromItemData(conveyorWgoData.ConveyorComponent.OutItem);
		}
	}

	public void UpdateDropView()
	{
		if (Parent.Data is ConveyorWgoData && !(Parent.MainWgoPart == null))
		{
			Parent.MainWgoPart.UpdateDropViewFromInventory();
		}
	}

	private void UpdateScaleLayersCommonCell(ConveyorWgoData current)
	{
		shouldEnableScaleIn = current.ConveyorComponent.InItem != null && !current.ConveyorComponent.InItem.isCommon;
		shouldEnableScaleOut = current.ConveyorComponent.OutItem != null && !current.ConveyorComponent.OutItem.isCommon;
		SetLayerWeight(itemScaleInLayerIndex, shouldEnableScaleIn);
		SetLayerWeight(itemScaleOutLayerIndex, shouldEnableScaleOut);
	}

	private void UpdateViewCommonCell()
	{
		if (base.CurrentState == "Idle" || isItemIdleLocked)
		{
			UpdateDropView();
		}
		else if (base.CurrentState == "In")
		{
			UpdateDropViewIn();
		}
		else if (base.CurrentState == "Out")
		{
			UpdateDropViewOut();
		}
	}

	private void SetupAnimatorForOrchestrator()
	{
		if (!(animator == null))
		{
			animator.updateMode = AnimatorUpdateMode.Normal;
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			animator.applyRootMotion = false;
			animator.speed = 0f;
		}
	}

	private static int GetStateHash(string state)
	{
		if (state == "In")
		{
			return inStateHash;
		}
		if (state == "Out")
		{
			return outStateHash;
		}
		return idleStateHash;
	}
}
