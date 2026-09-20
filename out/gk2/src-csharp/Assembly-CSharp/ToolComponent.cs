using System;

public class ToolComponent
{
	public enum ToolUseStatus
	{
		None = 0,
		OK = 1,
		NotEnoughEnergy = 1,
		NotEnoughInsanity = 2,
		NotEnoughDurability = 3,
		NeedAppropriateToolType = 4,
		CantUseTool = 5,
		NotEnoughMastery = 6
	}

	private ToolUseStatus toolUseStatus;

	private Item toolInUse;

	private AnimationComponentBase animation;

	private IWorkActivity toolActor;

	private float energySpendRate;

	private float insanitySpendRate;

	private CraftComponent subscribedCraftComponent;

	private bool isAnimationDrivenAction;

	private bool isControlTakenByAnimation;

	private bool isActionActive;

	public ToolUseStatus UseStatus => toolUseStatus;

	public Item ToolInUse => toolInUse;

	public bool IsControlTakenByAnimation => isControlTakenByAnimation;

	public IWorkActivity ToolActor => toolActor;

	public bool IsActionActive => isActionActive;

	public event Action OnInteractionStop;

	public ToolComponent(AnimationComponentBase animation)
	{
		this.animation = animation;
	}

	public bool TryStartInteraction(IWorkActivity toolActor, Item toolItem, Direction direction)
	{
		animation.OnToolLoopStarted += HandleAnimationLoopStart;
		animation.OnToolLoopFinished += HandleAnimationLoopFinish;
		this.toolActor = toolActor;
		SubscribeToCraftChanges();
		CacheSpendRates(toolItem);
		if (!CanProceedWork(toolItem))
		{
			StopInteraction();
			return false;
		}
		isAnimationDrivenAction = toolItem.Definition.IsAnimationDrivenTool;
		isActionActive = true;
		toolInUse = toolItem;
		animation.SetState(isAnimationDrivenAction ? GetAnimationStateForTool(toolInUse) : AnimationState.WorkHands);
		animation.SetDirection(direction);
		return true;
	}

	public void TryUpdateToolInUse(Item toolItem)
	{
		if (isActionActive && toolInUse != toolItem)
		{
			CacheSpendRates(toolItem);
			if (!CanProceedWork(toolItem))
			{
				StopInteraction();
				return;
			}
			toolInUse = toolItem;
			animation.SetState(isAnimationDrivenAction ? GetAnimationStateForTool(toolInUse) : AnimationState.WorkHands);
		}
	}

	public void UpdateInteraction()
	{
		if (isActionActive)
		{
			if (!CanProceedWork(toolInUse))
			{
				StopInteraction();
			}
			else if (!isAnimationDrivenAction)
			{
				ApplyAction();
			}
		}
	}

	public void StopInteraction()
	{
		if (isAnimationDrivenAction && isControlTakenByAnimation)
		{
			animation.OnToolLoopFinished += HandleLastLoopFinished;
			return;
		}
		ResetData();
		this.OnInteractionStop?.Invoke();
		void HandleLastLoopFinished(ItemType itemType)
		{
			animation.OnToolLoopFinished -= HandleLastLoopFinished;
			ResetData();
			this.OnInteractionStop?.Invoke();
		}
	}

	public void OnUseToolActionAnimationEvent()
	{
		if (isActionActive)
		{
			ApplyAction();
		}
	}

	private void SubscribeToCraftChanges()
	{
		UnsubscribeFromCraftChanges();
		if (toolActor is PlayerCraftActivity playerCraftActivity)
		{
			subscribedCraftComponent = playerCraftActivity.CraftComponent;
			subscribedCraftComponent.OnCraftStart += HandleCurrentCraftChanged;
		}
	}

	private void UnsubscribeFromCraftChanges()
	{
		if (subscribedCraftComponent != null)
		{
			subscribedCraftComponent.OnCraftStart -= HandleCurrentCraftChanged;
			subscribedCraftComponent = null;
		}
	}

	private void HandleCurrentCraftChanged()
	{
		if (isActionActive && toolInUse != null && toolActor != null && (subscribedCraftComponent == null || subscribedCraftComponent.CurrentCraftElement != null || subscribedCraftComponent.IsQueueDelayed))
		{
			CacheSpendRates(toolInUse);
		}
	}

	private void CacheSpendRates(Item toolItem)
	{
		energySpendRate = toolActor.GetEnergyCostPerTick(toolItem);
		insanitySpendRate = toolActor.GetInsanityCostPerTick(toolItem);
	}

	private bool CanProceedWork(Item toolItem)
	{
		if (!toolActor.CanUseTool(toolItem))
		{
			toolUseStatus = ToolUseStatus.CantUseTool;
			return false;
		}
		if (!toolActor.IsEnoughMastery())
		{
			toolUseStatus = ToolUseStatus.NotEnoughMastery;
			return false;
		}
		if (!toolActor.IsEnoughEnergy(toolItem, energySpendRate))
		{
			toolUseStatus = ToolUseStatus.OK;
			return false;
		}
		if (!toolActor.CanChangeInsanity(toolItem, insanitySpendRate))
		{
			toolUseStatus = ToolUseStatus.NotEnoughInsanity;
			return false;
		}
		if (!toolActor.IsEnoughDurability(toolItem))
		{
			toolUseStatus = ToolUseStatus.NotEnoughDurability;
			return false;
		}
		toolUseStatus = ToolUseStatus.OK;
		return true;
	}

	private void HandleAnimationLoopStart(ItemType itemType)
	{
		if (isAnimationDrivenAction)
		{
			isControlTakenByAnimation = true;
		}
	}

	private void HandleAnimationLoopFinish(ItemType itemType)
	{
		if (isAnimationDrivenAction)
		{
			isControlTakenByAnimation = false;
		}
	}

	private void ResetData()
	{
		animation.OnToolLoopStarted -= HandleAnimationLoopStart;
		animation.OnToolLoopFinished -= HandleAnimationLoopFinish;
		animation.SetState(AnimationState.Idle);
		isActionActive = false;
		toolInUse = null;
		energySpendRate = 0f;
		insanitySpendRate = 0f;
		UnsubscribeFromCraftChanges();
	}

	private void ApplyAction()
	{
		toolActor.ConsumeEnergy(toolInUse, energySpendRate);
		toolActor.ChangeInsanity(toolInUse, insanitySpendRate);
		UseTool();
	}

	private void UseTool()
	{
		toolActor.UseTool(ToolInUse, 1);
		if (toolInUse.TryGetProperty<DurabilitySerializedItemProperty>(out var property))
		{
			property.Durability -= toolInUse.Definition.durDecreaseOnUse;
			if (property.Durability.EqualsTo(0f))
			{
				BreakTool(toolInUse);
			}
		}
	}

	private void BreakTool(Item toolItem)
	{
	}

	private AnimationState GetAnimationStateForTool(Item tool)
	{
		return (AnimationState)(tool.Definition.type + 19);
	}
}
