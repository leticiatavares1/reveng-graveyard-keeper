using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UICraftQueueElementWidgetData : LazyWidgetDataBase
{
	public Action OnPress { get; private set; }

	public Action OnOver { get; private set; }

	public Action OnOut { get; private set; }

	public Action OnHide { get; private set; }

	public Action OnPressPlusQueue { get; private set; }

	public Action OnPressMinusQueue { get; private set; }

	public Action OnInfCraftButtonPress { get; private set; }

	public Action<CraftElementBase> OnPressQueueUp { get; private set; }

	public Action<CraftElementBase> OnPressQueueDown { get; private set; }

	public Action<CraftElementBase> OnRemoveFromQueuePressed { get; private set; }

	public CraftElementBase CraftQueueElement { get; private set; }

	public List<CraftElementBase> CraftQueue { get; private set; }

	public bool IsMulticraftDisabled { get; private set; }

	public WgoData WgoData { get; private set; }

	public UICraftQueueElementWidgetData(WgoData wgoData, CraftElementBase craftQueueElement, List<CraftElementBase> craftQueue, Action onHide, Action onPress, Action onOver, Action onOut, Action<CraftElementBase> onQueueUp, Action<CraftElementBase> onQueueDown, Action<CraftElementBase> onRemoveFromQueuePressed)
	{
		OnPressPlusQueue = OnPlus;
		OnPressMinusQueue = OnMinus;
		OnInfCraftButtonPress = OnInf;
		OnPressQueueUp = onQueueUp;
		OnPressQueueDown = onQueueDown;
		OnRemoveFromQueuePressed = onRemoveFromQueuePressed;
		OnPress = onPress;
		OnOver = onOver;
		OnOut = onOut;
		OnHide = onHide;
		CraftQueueElement = craftQueueElement;
		CraftQueue = craftQueue;
		IsMulticraftDisabled = craftQueueElement.Def is CraftDef craftDef && craftDef.IsMultipleCraftsDisabled;
		WgoData = wgoData;
	}

	private void OnPlus()
	{
		AddCount(1);
	}

	private void OnMinus()
	{
		AddCount(-1);
	}

	public void AddCount(int delta)
	{
		if (delta == 0 || CraftQueueElement.IsInfinite)
		{
			return;
		}
		if (delta > 0)
		{
			if (CraftQueueElement.Count != 999)
			{
				CraftQueueElement.Count = Math.Min(CraftQueueElement.Count + delta, 999);
			}
		}
		else
		{
			CraftQueueElement.Count = Math.Max(CraftQueueElement.Count + delta, 0);
		}
	}

	private void OnInf()
	{
		CraftQueueElement.IsInfinite = !CraftQueueElement.IsInfinite;
	}
}
