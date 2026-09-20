using System;
using UnityEngine;

[Serializable]
public class OrderBase
{
	[SerializeField]
	protected SGuid uniqueId;

	[SerializeField]
	protected SGuid targetWgoUniqueId = SGuid.Empty;

	[SerializeField]
	protected Item item;

	[SerializeField]
	protected SGuid executorUniqueId = SGuid.Empty;

	public Item Item => item;

	public SGuid TargetWgoUniqueId => targetWgoUniqueId;

	public SGuid ExecutorUniqueId
	{
		get
		{
			return executorUniqueId;
		}
		set
		{
			executorUniqueId = value;
		}
	}

	public ZombieWgoData ZombieWgoData => MainGame.ZombieSystemData.GetZombie(targetWgoUniqueId);

	public SGuid UniqueId => uniqueId;

	public OrderBase(SGuid targetWgoUniqueId, Item item)
	{
		uniqueId = new SGuid();
		this.targetWgoUniqueId.SetGuid(targetWgoUniqueId);
		this.item = new Item(item.id, item.Count);
	}

	public virtual int GetPriority()
	{
		return 0;
	}

	public virtual bool TryExecuteOrder(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		if (CanOrderBeExecuted(orderExecutor, out reasonIfNot))
		{
			ExecuteOrder(orderExecutor);
			return true;
		}
		return false;
	}

	public virtual void ExecuteOrder(IOrderExecutor orderExecutor)
	{
		Debug.LogError("ExecuteOrder is not implemented");
	}

	public virtual bool CanOrderBeExecuted(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		Debug.LogError("CanOrderBeExecuted is not implemented");
		reasonIfNot = "CanOrderBeExecuted is not implemented";
		return false;
	}

	public virtual string GetInteractionHint()
	{
		Debug.LogError("GetInteractionHint is not implemented");
		return string.Empty;
	}

	public virtual string GetStatusIcon()
	{
		Debug.LogError("GetStatusIcon is not implemented");
		return string.Empty;
	}
}
