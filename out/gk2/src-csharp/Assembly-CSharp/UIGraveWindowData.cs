using LazyBearTechnology;

public class UIGraveWindowData : LazyWidgetDataBase
{
	public UICorpseWidgetData CorpseWidgetData { get; private set; }

	public UIGraveElementWidgetData TombstoneWidgetData { get; private set; }

	public UIGraveElementWidgetData FenceWidgetData { get; private set; }

	public float Quality { get; private set; }

	public WgoData WgoData { get; private set; }

	public UIGraveWindowData(Wgo wgo)
	{
		WgoData = wgo.Data;
		Quality = wgo.Data.Quality;
		Item item = null;
		foreach (Item item2 in wgo.Data.Inventory.Data.Inventory)
		{
			if (item2.Definition.itemGroupIds.Contains("body"))
			{
				item = item2;
				break;
			}
		}
		if (item != null)
		{
			CorpseWidgetData = new UICorpseWidgetData(item, wgo.Data, TryExhumeBody, CanExhume(), GameKey.ExtractBody, LLBase.L("ui_grave_corpse_widget_header"), LLBase.L("ui_grave_corpse_text"), LLBase.L("exhume"), ShowExhumeButtonTooltip);
		}
		else
		{
			CorpseWidgetData = new UICorpseWidgetData(GameKey.ExtractBody);
		}
		TombstoneWidgetData = new UIGraveElementWidgetData(wgo, GraveElementType.Top);
		FenceWidgetData = new UIGraveElementWidgetData(wgo, GraveElementType.Bot);
	}

	private bool CanExhume()
	{
		foreach (WgoPartData additionalWgoPartsDatum in WgoData.AdditionalWgoPartsData)
		{
			ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(additionalWgoPartsDatum.id);
			if (dataOrNull != null && (dataOrNull.itemGroupIds.Contains("gravetop") || dataOrNull.itemGroupIds.Contains("gravebot")))
			{
				return false;
			}
		}
		return true;
	}

	private void TryExhumeBody()
	{
		if (!CorpseWidgetData.IsEmpty && CorpseWidgetData.Body != null && CanExhume())
		{
			UIDialogWindow window = LazyUI.GetWindow<UIDialogWindow>();
			UIDialogWindowData uIDialogWindowData = null;
			uIDialogWindowData = ((!HasExhumeCertificate()) ? new UIDialogWindowData(new Item("exhume_certificate"), LLBase.L("exhume"), LLBase.L("exhume_confirmation"), "", 0, 1, delegate
			{
				LazyUI.GetWindow<UIDialogWindow>().Close();
			}) : new UIDialogWindowData(new Item("exhume_certificate"), LLBase.L("exhume"), LLBase.L("exhume_confirmation"), LLBase.L("exhume_confirmation_bot"), MainGame.PlayerData.inventory.Data.GetTotalCountInInventory("exhume_certificate"), 1, ExhumeBody, delegate
			{
				LazyUI.GetWindow<UIDialogWindow>().Close();
			}));
			uIDialogWindowData.ShowCloseButton = false;
			window.Open(uIDialogWindowData);
		}
	}

	private void ExhumeBody()
	{
		MainGame.PlayerData.inventory.RemoveItemById("exhume_certificate", 1);
		MainGame.Instance.GameSave.worldData.ChangeWgoData(WgoData, "grave_exhume");
		LazyUI.GetWindow<UIDialogWindow>().Close();
		LazyUI.GetWindow<UIGraveWindow>().Close();
	}

	private bool HasExhumeCertificate()
	{
		return MainGame.PlayerData.inventory.Data.GetTotalCountInInventory("exhume_certificate") > 0;
	}

	private void ShowExhumeButtonTooltip(LazyButton exhumeButton)
	{
		UITooltip.ShowExhumeButtonWidget(exhumeButton);
	}
}
