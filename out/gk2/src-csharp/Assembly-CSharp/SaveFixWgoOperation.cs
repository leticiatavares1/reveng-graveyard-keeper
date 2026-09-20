using System;

[Serializable]
public abstract class SaveFixWgoOperation : SaveFixOperation
{
	public virtual bool OccupiesUniqueId => true;

	public virtual bool TryGetTargetUniqueId(out SGuid uniqueId)
	{
		uniqueId = null;
		return false;
	}

	public virtual bool ContainsUniqueId(SGuid uniqueId)
	{
		if (TryGetTargetUniqueId(out var uniqueId2))
		{
			return uniqueId2 == uniqueId;
		}
		return false;
	}
}
