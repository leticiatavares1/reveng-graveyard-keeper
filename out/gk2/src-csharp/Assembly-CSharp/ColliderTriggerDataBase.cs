using System;
using UnityEngine;

[Serializable]
public abstract class ColliderTriggerDataBase
{
	[SerializeField]
	[Range(0f, 100f)]
	protected float chance;

	[SerializeField]
	protected float delayFrom;

	[SerializeField]
	protected float delayTo;

	protected float lastTriggerTime = -1f;

	protected float rolledDelay = -1f;

	public void TrySetTrigger()
	{
		if (IsSetupCompleted() && chance >= UnityEngine.Random.Range(0f, 100f) && Time.time >= lastTriggerTime + rolledDelay)
		{
			lastTriggerTime = Time.time;
			rolledDelay = UnityEngine.Random.Range(delayFrom, delayTo);
			TriggerSetAction();
		}
	}

	public void TryResetTrigger()
	{
		if (IsSetupCompleted())
		{
			TriggerResetAction();
		}
	}

	protected virtual bool IsSetupCompleted()
	{
		return true;
	}

	protected abstract void TriggerSetAction();

	protected abstract void TriggerResetAction();
}
