using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PerksWidget : LazyWidget<PerksWidgetData>
{
	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private GameObject emptyObject;

	private readonly List<PerkWidgetFull> displayedPerks = new List<PerkWidgetFull>();

	private List<PerksWidgetSeparator> displayedSeparators = new List<PerksWidgetSeparator>();

	public event Action onContentChanged;

	protected override void SetData(PerksWidgetData data)
	{
		base.SetData(data);
		data.OnPerkAdded += AddPerkWidget;
		data.OnPerkRemoved += RemovePerkWidget;
		data.OnPerkUpdated += UpdatePerk;
	}

	public override void Redraw()
	{
		base.Redraw();
		int count = data.Perks.Count;
		for (int i = 0; i < count; i++)
		{
			PerkWidgetFull elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<PerkWidgetFull>(content.transform);
			elementFromPool.transform.SetAsLastSibling();
			PerkWidgetData perkWidgetData = new PerkWidgetData(data.Perks[i], isActive: true, null, null, null);
			elementFromPool.Draw(perkWidgetData);
			displayedPerks.Add(elementFromPool);
			if (i < count - 1)
			{
				PerksWidgetSeparator elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<PerksWidgetSeparator>(content.transform);
				elementFromPool2.gameObject.SetActive(value: true);
				elementFromPool2.transform.SetAsLastSibling();
				displayedSeparators.Add(elementFromPool2);
				elementFromPool.NextItemSeparator = elementFromPool2;
			}
		}
		emptyObject.SetActive(count <= 0);
	}

	public override void Hide()
	{
		if (data != null)
		{
			data.OnPerkAdded -= AddPerkWidget;
			data.OnPerkRemoved -= RemovePerkWidget;
			data.OnPerkUpdated -= UpdatePerk;
		}
		base.Hide();
		foreach (PerkWidgetFull displayedPerk in displayedPerks)
		{
			displayedPerk.NextItemSeparator = null;
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedPerk);
		}
		foreach (PerksWidgetSeparator displayedSeparator in displayedSeparators)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedSeparator);
		}
		displayedPerks.Clear();
		displayedSeparators.Clear();
	}

	public void DisableGamepadNavigation()
	{
		GamepadNavigationItem[] componentsInChildren = GetComponentsInChildren<GamepadNavigationItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Active = false;
		}
	}

	public void EnableGamepadNavigation()
	{
		GamepadNavigationItem[] componentsInChildren = GetComponentsInChildren<GamepadNavigationItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Active = true;
		}
	}

	private void AddPerkWidget(PerkData perkData)
	{
		if (displayedPerks.Count > 0)
		{
			PerksWidgetSeparator elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<PerksWidgetSeparator>(content.transform);
			elementFromPool.gameObject.SetActive(value: true);
			elementFromPool.transform.SetParent(content.transform);
			elementFromPool.transform.SetAsLastSibling();
			displayedSeparators.Add(elementFromPool);
			List<PerkWidgetFull> list = displayedPerks;
			list[list.Count - 1].NextItemSeparator = elementFromPool;
		}
		PerkWidgetFull elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<PerkWidgetFull>(content.transform);
		elementFromPool2.transform.SetParent(content.transform);
		PerkWidgetData perkWidgetData = new PerkWidgetData(perkData, isActive: true, null, null, null);
		elementFromPool2.Draw(perkWidgetData);
		displayedPerks.Add(elementFromPool2);
		elementFromPool2.transform.SetAsLastSibling();
		emptyObject.SetActive(data.Perks.Count <= 0);
		this.onContentChanged?.Invoke();
	}

	private void RemovePerkWidget(PerkData perkData)
	{
		PerkWidgetFull perkWidgetFull = displayedPerks.Find((PerkWidgetFull x) => x.PerkData == perkData);
		if (perkWidgetFull != null)
		{
			if (perkWidgetFull.NextItemSeparator != null)
			{
				displayedSeparators.Remove(perkWidgetFull.NextItemSeparator);
				UIPrefabsPooler.Instance.ReleaseElementToPool(perkWidgetFull.NextItemSeparator);
			}
			UIPrefabsPooler.Instance.ReleaseElementToPool(perkWidgetFull);
			displayedPerks.Remove(perkWidgetFull);
		}
		emptyObject.SetActive(data.Perks.Count <= 0);
		this.onContentChanged?.Invoke();
	}

	private void UpdatePerk(PerkData perkData)
	{
		displayedPerks.Find((PerkWidgetFull x) => x.PerkData == perkData)?.Redraw();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new PerksWidgetData());
	}
}
