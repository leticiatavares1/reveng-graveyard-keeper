using UnityEngine;

public class DropCollector : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<TriggerColliderComponentLinker>(out var component))
		{
			DropView dropView = component.Component as DropView;
			TryCollectDrop(dropView);
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if (other.TryGetComponent<TriggerColliderComponentLinker>(out var component))
		{
			DropView dropView = component.Component as DropView;
			TryCollectDrop(dropView);
		}
	}

	public static bool CanCollectDrop(DropView dropView)
	{
		if (dropView == null)
		{
			return false;
		}
		if (dropView.IsTimedCollecting)
		{
			return false;
		}
		if (dropView.Data.IsRemoving)
		{
			return false;
		}
		if (dropView.Data.DropType == DropType.WgoData)
		{
			return false;
		}
		if (dropView.Data.Size == ItemSize.Big)
		{
			return false;
		}
		if (dropView.IsDespawning || dropView.IsCollectDelayed)
		{
			return false;
		}
		return true;
	}

	private void TryCollectDrop(DropView dropView)
	{
		if (CanCollectDrop(dropView))
		{
			CollectDrop(dropView);
		}
	}

	private void CollectDrop(DropView dropView)
	{
		MainGame.PlayerData.CollectDrop(dropView);
	}
}
