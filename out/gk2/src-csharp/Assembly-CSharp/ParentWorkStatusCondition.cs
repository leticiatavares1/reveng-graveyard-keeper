using System;

[Serializable]
public class ParentWorkStatusCondition : ConditionalDrawerConditionBase
{
	public override ConditionalEventType EventType => ConditionalEventType.ParentWorkCondition;

	private bool IsCraftStarted(WgoData wgoData)
	{
		CraftComponent craftComponent = wgoData.CraftComponent;
		if (craftComponent == null)
		{
			return false;
		}
		return craftComponent.Status == CraftComponentStatus.Started;
	}

	private bool HasWorker(WgoData wgoData)
	{
		if (wgoData.Worker != null)
		{
			return !wgoData.Worker.Id.IsEmpty;
		}
		return false;
	}

	private bool IsInWork(WgoData wgoData)
	{
		if (!HasWorker(wgoData))
		{
			return false;
		}
		if (!IsCraftStarted(wgoData))
		{
			return false;
		}
		if (wgoData.Worker is ZombieWgoData { WorkerActivity: ZombieCraftActivity workerActivity })
		{
			return workerActivity.IsActive;
		}
		if (wgoData.Worker is PlayerController playerController)
		{
			if (playerController.PlayerWorkComponent != null && playerController.PlayerWorkComponent.Wgo.Data == wgoData && playerController.PlayerWorkComponent.WorkInProgress)
			{
				return playerController.PlayerWorkComponent.ToolComponent.IsActionActive;
			}
			return false;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		foreach (SGuid workbenchParent in context.WgoData.WorkbenchParents)
		{
			WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(workbenchParent);
			if (wgoData != null && wgoData.CraftComponent != null)
			{
				return IsInWork(wgoData);
			}
		}
		return false;
	}
}
