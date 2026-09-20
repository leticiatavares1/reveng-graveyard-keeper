using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialListWindow : LazyWindow<UITutorialListWindowData>
{
	[SerializeField]
	private Transform itemsContainer;

	[SerializeField]
	private ScrollRect scrollRect;

	private readonly List<UITutorialListItemWidget> displayedItems = new List<UITutorialListItemWidget>();

	private bool openingTutorial;

	public override void Redraw()
	{
		base.Redraw();
		DrawItems();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		if (scrollRect != null)
		{
			scrollRect.DOKill();
			scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	public override void Close()
	{
		bool num = ((data != null) ? data.OpenSource : UITutorialListOpenSource.HUD) == UITutorialListOpenSource.PauseWindow && !openingTutorial;
		base.Close();
		if (num)
		{
			LazyUI.GetWindow<UIGamePauseWindow>().Open(null);
		}
	}

	public override void Hide()
	{
		HideDisplayedItems();
		openingTutorial = false;
		base.Hide();
	}

	private void DrawItems()
	{
		HideDisplayedItems();
		KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
		if (knowledgeSystem.viewedTutorials == null || itemsContainer == null || UIPrefabsPooler.Instance == null)
		{
			return;
		}
		foreach (string viewedTutorial in knowledgeSystem.viewedTutorials)
		{
			UITutorialListItemWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UITutorialListItemWidget>(itemsContainer);
			if (!(elementFromPool == null))
			{
				displayedItems.Add(elementFromPool);
				elementFromPool.Init();
				elementFromPool.Draw(new UITutorialListItemWidgetData(viewedTutorial, OpenTutorial));
			}
		}
	}

	private void HideDisplayedItems()
	{
		foreach (UITutorialListItemWidget displayedItem in displayedItems)
		{
			displayedItem.DeInit();
			displayedItem.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedItem);
		}
		displayedItems.Clear();
	}

	private void OpenTutorial(string tutorialId)
	{
		openingTutorial = true;
		Close();
		LazyUI.GetWindow<UITutorialWindow>().Open(new UITutorialWindowData(tutorialId, ReturnToTutorialList, canCloseFromAnyPage: true));
	}

	private void ReturnToTutorialList()
	{
		LazyUI.GetWindow<UITutorialListWindow>().Open(data);
	}

	protected override void PrintTips()
	{
		lazyButtonTips?.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Open(new UITutorialListWindowData(UITutorialListOpenSource.HUD));
	}
}
