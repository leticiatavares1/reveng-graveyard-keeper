using System.Collections.Generic;
using LazyBearTechnology;

public class TakeAllInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		WgoData data = assignedWgo.Data;
		foreach (Item item in data.Inventory.Data.Inventory)
		{
			List<Item> list = new List<Item>();
			MainGame.Instance.dropSystem.DropItem(item, data.WorldId, data.Position, list);
			foreach (Item item2 in list)
			{
				DropView dropView = null;
				foreach (GameScene loadedGameScene in LazySingleton<GameSceneManager>.Instance.LoadedGameScenes)
				{
					dropView = loadedGameScene.GetDropView(item2);
					if (dropView != null)
					{
						break;
					}
				}
				if (dropView != null)
				{
					dropView.MoveToCustomPosition(MainGame.PlayerController.transform.position);
				}
			}
		}
		data.Inventory.Clear();
		data.IsInteractable = false;
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		base.HasInteraction(interactor);
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_take_all", GameKey.Interaction)));
	}
}
