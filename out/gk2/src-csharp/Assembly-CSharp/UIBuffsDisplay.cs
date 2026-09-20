using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIBuffsDisplay : LazyWidget<UIBuffsDisplayData>
{
	[SerializeField]
	private UIBuffElement uiBuffElementPrefab;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private Vector2 sizeBig;

	[SerializeField]
	private Vector2 sizeSmall;

	private List<UIBuffElement> activeElements = new List<UIBuffElement>();

	private List<UIBuffElement> elementsPool = new List<UIBuffElement>();

	public RectTransform RectTransform => base.transform as RectTransform;

	public RectTransform Content => content;

	public override void Init()
	{
		uiBuffElementPrefab.gameObject.SetActive(value: false);
	}

	protected override void SetData(UIBuffsDisplayData data)
	{
		base.SetData(data);
		data.OnBuffAdded += AddBuffToDisplay;
		data.OnBuffRemoved += RemoveBuffFromDisplay;
		data.OnBuffUpdated += UpdateBuffOnDisplay;
		data.OnBuffFxRequested += PlayAppliedFx;
	}

	public override void Redraw()
	{
		base.Redraw();
		for (int i = 0; i < data.UIBuffElementsData.Count; i++)
		{
			AddBuffToDisplay(data.UIBuffElementsData[i], playAppliedFx: false);
		}
		content.sizeDelta = ((GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Big) ? sizeBig : sizeSmall);
	}

	public override void Hide()
	{
		if (data != null)
		{
			data.OnBuffAdded -= AddBuffToDisplay;
			data.OnBuffRemoved -= RemoveBuffFromDisplay;
			data.OnBuffUpdated -= UpdateBuffOnDisplay;
			data.OnBuffFxRequested -= PlayAppliedFx;
		}
		for (int num = activeElements.Count - 1; num >= 0; num--)
		{
			activeElements[num].gameObject.SetActive(value: false);
			elementsPool.Add(activeElements[num]);
			activeElements.Remove(activeElements[num]);
		}
		if (activeElements.Count == 0)
		{
			content.gameObject.SetActive(value: false);
		}
		base.Hide();
	}

	private void AddBuffToDisplay(UIBuffElementData elementData)
	{
		AddBuffToDisplay(elementData, playAppliedFx: true);
	}

	private void AddBuffToDisplay(UIBuffElementData elementData, bool playAppliedFx)
	{
		UIBuffElement @new = GetNew();
		@new.gameObject.SetActive(value: true);
		@new.Draw(elementData);
		if (playAppliedFx)
		{
			PlayAppliedFx(@new, elementData.PerkData.Definition);
		}
		activeElements.Add(@new);
		if (!content.gameObject.activeSelf)
		{
			content.gameObject.SetActive(value: true);
		}
	}

	private void PlayAppliedFx(string buffId)
	{
		UIBuffElement uIBuffElement = activeElements.Find((UIBuffElement x) => x.Id == buffId);
		if (!(uIBuffElement == null))
		{
			PlayAppliedFx(uIBuffElement, data.UIBuffElementsData.Find((UIBuffElementData x) => x.PerkData.id == buffId)?.PerkData.Definition);
		}
	}

	private void PlayAppliedFx(UIBuffElement element, PerkDef def)
	{
		element.PlayHudFx();
		if (def == null || string.IsNullOrEmpty(def.worldFxPrefabId))
		{
			return;
		}
		PlayerView playerView = MainGame.PlayerController?.View;
		if (playerView == null)
		{
			WorldFX.Spawn(MainGame.PlayerData.position.Value, def.worldFxPrefabId);
			return;
		}
		WorldFX worldFX = WorldFX.Spawn(playerView.transform.position, def.worldFxPrefabId);
		if (!(worldFX == null))
		{
			worldFX.Follow(playerView.transform);
		}
	}

	private void RemoveBuffFromDisplay(string buffId)
	{
		UIBuffElement uIBuffElement = activeElements.Find((UIBuffElement x) => x.Id == buffId);
		if (uIBuffElement != null)
		{
			uIBuffElement.gameObject.SetActive(value: false);
			activeElements.Remove(uIBuffElement);
			elementsPool.Add(uIBuffElement);
		}
		if (activeElements.Count == 0)
		{
			content.gameObject.SetActive(value: false);
		}
	}

	private void UpdateBuffOnDisplay(string buffId)
	{
		activeElements.Find((UIBuffElement x) => x.Id == buffId)?.Redraw();
	}

	private UIBuffElement GetNew()
	{
		if (elementsPool.Count == 0)
		{
			return uiBuffElementPrefab.Copy(null, activate: false);
		}
		return elementsPool.PopLast();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		UIBuffsDisplayData uIBuffsDisplayData = new UIBuffsDisplayData(new List<PerkType> { PerkType.Buff });
		uIBuffsDisplayData.UIBuffElementsData = new List<UIBuffElementData>
		{
			new UIBuffElementData(new PerkData("test1"), 20f, hasHiddenTimer: false, isInfinite: false),
			new UIBuffElementData(new PerkData("test2"), 100f, hasHiddenTimer: false, isInfinite: false),
			new UIBuffElementData(new PerkData("test3"), 70f, hasHiddenTimer: true, isInfinite: false),
			new UIBuffElementData(new PerkData("test3"), -1f, hasHiddenTimer: true, isInfinite: true)
		};
		Draw(uIBuffsDisplayData);
	}
}
