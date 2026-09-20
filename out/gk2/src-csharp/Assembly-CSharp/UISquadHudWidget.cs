using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UISquadHudWidget : LazyWidget<UISquadHudWidgetData>
{
	[SerializeField]
	private Image bannerIcon;

	[SerializeField]
	private RectTransform elementsParent;

	private List<UISquadHudElementWidget> drawnElements = new List<UISquadHudElementWidget>();

	public override void Hide()
	{
		base.Hide();
		Clear();
	}

	public override void Redraw()
	{
		base.Redraw();
		Clear();
		if (data == null)
		{
			return;
		}
		if (bannerIcon != null)
		{
			bannerIcon.sprite = data.Banner;
			bannerIcon.gameObject.SetActive(data.Banner != null);
		}
		if (data.AlliesSpawn == null)
		{
			return;
		}
		Transform newParent = ((elementsParent != null) ? elementsParent : base.transform);
		foreach (WgoData fighter in data.AlliesSpawn.Fighters)
		{
			if (fighter != null)
			{
				UISquadHudElementWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UISquadHudElementWidget>(newParent);
				drawnElements.Add(elementFromPool);
				elementFromPool.Draw(new UISquadHudElementWidgetData(fighter));
				elementFromPool.transform.SetAsLastSibling();
			}
		}
	}

	private void Clear()
	{
		foreach (UISquadHudElementWidget drawnElement in drawnElements)
		{
			if (!(drawnElement == null))
			{
				drawnElement.Hide();
				UIPrefabsPooler.Instance.ReleaseElementToPool(drawnElement);
			}
		}
		drawnElements.Clear();
	}

	protected override void TestDraw()
	{
	}
}
