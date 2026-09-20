using UnityEngine;

public class DropSearcher : MonoBehaviour
{
	[SerializeField]
	private Transform collectorObjectTransform;

	public Transform CollectorObjectTransform => collectorObjectTransform;

	public void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<TriggerColliderComponentLinker>(out var component))
		{
			TryFindAndMoveDropToCollector(component);
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if (other.TryGetComponent<TriggerColliderComponentLinker>(out var component))
		{
			TryFindAndMoveDropToCollector(component);
		}
	}

	private void TryFindAndMoveDropToCollector(TriggerColliderComponentLinker linker)
	{
		DropView dropView = linker.Component as DropView;
		if (DropCollector.CanCollectDrop(dropView) && (dropView.Data.IsResDrop || MainGame.PlayerData.inventory.CanAddItemToInventory(dropView.Data.Item.id, 1)))
		{
			dropView.TryMoveToCollector(collectorObjectTransform);
		}
	}
}
