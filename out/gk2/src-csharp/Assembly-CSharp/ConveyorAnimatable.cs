using UnityEngine;

public abstract class ConveyorAnimatable : MonoBehaviour
{
	public abstract ConveyorAnimatableType Type { get; }

	public virtual bool IsValid => true;

	public virtual void CreateCache()
	{
	}

	public virtual void RestoreCache()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
