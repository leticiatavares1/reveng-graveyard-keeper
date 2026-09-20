using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIFightingSquadHudGroupWidget : LazyWidget<UIFightingSquadHudGroupWidgetData>
{
	[SerializeField]
	private Sprite[] banners;

	[SerializeField]
	private RectTransform widgetsParent;

	private List<UISquadHudWidget> drawnSquads = new List<UISquadHudWidget>();

	private List<UISquadHudSeparator> drawnSeparators = new List<UISquadHudSeparator>();

	public override void Hide()
	{
		base.Hide();
		Clear();
	}

	public override void Redraw()
	{
		base.Redraw();
		Clear();
		if (data?.FightingLevel == null)
		{
			return;
		}
		Transform newParent = ((widgetsParent != null) ? widgetsParent : base.transform);
		List<AlliesSpawn> alliesSpawns = data.FightingLevel.AlliesSpawns;
		List<AlliesSpawn> list = new List<AlliesSpawn>();
		for (int i = 0; i < alliesSpawns.Count; i++)
		{
			if (alliesSpawns[i] != null && alliesSpawns[i].Fighters.Count > 0)
			{
				list.Add(alliesSpawns[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j > 0)
			{
				UISquadHudSeparator elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UISquadHudSeparator>(newParent);
				drawnSeparators.Add(elementFromPool);
				elementFromPool.transform.SetAsLastSibling();
			}
			UISquadHudWidget elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<UISquadHudWidget>(newParent);
			drawnSquads.Add(elementFromPool2);
			elementFromPool2.Draw(new UISquadHudWidgetData(list[j], GetBanner(list[j].SquadSlotIndex)));
			elementFromPool2.transform.SetAsLastSibling();
		}
	}

	private Sprite GetBanner(int index)
	{
		if (banners == null || index < 0 || index >= banners.Length)
		{
			return null;
		}
		return banners[index];
	}

	private void Clear()
	{
		foreach (UISquadHudWidget drawnSquad in drawnSquads)
		{
			drawnSquad.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(drawnSquad);
		}
		drawnSquads.Clear();
		foreach (UISquadHudSeparator drawnSeparator in drawnSeparators)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(drawnSeparator);
		}
		drawnSeparators.Clear();
	}

	protected override void TestDraw()
	{
	}
}
