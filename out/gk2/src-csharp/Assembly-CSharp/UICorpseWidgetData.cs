using System;
using LazyBearTechnology;
using UnityEngine;

public class UICorpseWidgetData : LazyWidgetDataBase
{
	public Action OnButtonPressed { get; private set; }

	public string IconId { get; private set; }

	public int RedSkulls { get; private set; }

	public int WhiteSkulls { get; private set; }

	public bool IsEmpty { get; private set; }

	public bool ButtonInteractable { get; private set; }

	public string DescriptionText { get; private set; }

	public string HeaderText { get; private set; }

	public string ButtonText { get; private set; }

	public WgoData WgoData { get; private set; }

	public Item Body { get; private set; }

	public ZombieWgoData ZombieWgoData { get; private set; }

	public bool IsZombie { get; private set; }

	public GameKey GameKeyToExhume { get; private set; }

	public Action<LazyButton> OnNonInteractableButtonOver { get; private set; }

	public int CollarRedSkullsLimit { get; set; } = -1;


	public UICorpseWidgetData(GameKey gameKeyToExhume)
	{
		IsEmpty = true;
		HeaderText = LLBase.L("ui_grave_corpse_widget_header");
		DescriptionText = LLBase.L("ui_put_body");
		ButtonText = LLBase.L("btn_take_body_two_lines");
		IconId = "i_body";
		GameKeyToExhume = gameKeyToExhume;
	}

	public UICorpseWidgetData(Item body, WgoData wgoData, Action onButtonPressed, bool buttonInteractable, GameKey gameKeyToExhume, string headerText = null, string descriptionText = null, string buttonText = null, Action<LazyButton> onNonInteractableButtonOver = null)
	{
		GameKeyToExhume = gameKeyToExhume;
		IsEmpty = false;
		ButtonInteractable = buttonInteractable;
		OnButtonPressed = onButtonPressed;
		WgoData = wgoData;
		Body = body;
		DescriptionText = descriptionText;
		HeaderText = headerText;
		ButtonText = buttonText;
		IconId = body.Definition.iconId;
		RedSkulls = 0;
		WhiteSkulls = 0;
		foreach (Item item in body.Inventory)
		{
			RedSkulls += item.Definition.redSkulls * item.Count;
			WhiteSkulls += item.Definition.whiteSkulls * item.Count;
		}
		RedSkulls = Mathf.Clamp(RedSkulls, 0, 999);
		WhiteSkulls = Mathf.Clamp(WhiteSkulls, 0, 999);
		ZombieWgoData = MainGame.ZombieSystemData.GetZombie(body.UniqueId);
		IsZombie = ZombieWgoData != null;
		OnNonInteractableButtonOver = onNonInteractableButtonOver;
	}
}
