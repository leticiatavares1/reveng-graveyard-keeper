using LazyBearTechnology;
using UnityEngine;

public class RiverDumpInteractionHandler : WGOInteractionHandlerBase
{
	private RiverBodyReceiver receiver;

	public override IWGOInteractionHandler Init(Wgo wgo)
	{
		receiver = wgo.GetComponentInChildren<RiverBodyReceiver>(includeInactive: true);
		if (receiver == null)
		{
			Debug.LogError("[RiverDumpInteractionHandler] RiverBodyReceiver not found on [" + wgo.name + "]");
		}
		return base.Init(wgo);
	}

	public override void OnInteractionTargetEnter(PlayerController interactor)
	{
		receiver?.SetDynamicBubbleEnabled(enabled: true);
		base.OnInteractionTargetEnter(interactor);
	}

	public override void OnInteractionTargetExit()
	{
		receiver?.SetDynamicBubbleEnabled(enabled: false);
		base.OnInteractionTargetExit();
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (!TryGetInsertableOverheadBodyItem(out var overheadItem) || receiver == null || !receiver.HasFlowSpline)
		{
			return false;
		}
		Vector3 overheadItemWorldPosition = interactor.View.PlayerAnimation.OverheadItemWorldPosition;
		if (!MainGame.Instance.riverDropSystem.BeginDump(overheadItem, receiver, overheadItemWorldPosition))
		{
			return false;
		}
		MainGame.PlayerData.RemoveOverheadItem(overheadItem);
		MainGame.PlayerData.SubRes("cur_bodies_count", 1f);
		if (MainGame.PlayerData.CurrentWorldZoneData != null && MainGame.PlayerData.CurrentWorldZoneData.Definition.id == "morgue")
		{
			GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
		}
		assignedWgo.DrawWidgets();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (HasInsertableOverheadBodyItem() && receiver != null)
		{
			return receiver.HasFlowSpline;
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		if (HasInsertableOverheadBodyItem())
		{
			string text = "hint_drop_body";
			if (LLBase.L(text) == text)
			{
				text = "hint_place_body";
			}
			return new InteractionInfos(new InteractionInfo(LocalizeHintWithActionIcon(text, GameKey.Interaction)));
		}
		return new InteractionInfos();
	}

	private bool HasInsertableOverheadBodyItem()
	{
		Item overheadItem;
		return TryGetInsertableOverheadBodyItem(out overheadItem);
	}

	private bool TryGetInsertableOverheadBodyItem(out Item overheadItem)
	{
		return MainGame.PlayerData.TryGetOverheadItem((Item item) => !item.HasItemsByItemType(ItemType.Demon) && item.Definition.itemGroupIds.Contains("corpse"), out overheadItem);
	}
}
