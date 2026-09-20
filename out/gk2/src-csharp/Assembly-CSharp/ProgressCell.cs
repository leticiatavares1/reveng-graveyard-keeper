using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class ProgressCell : MonoBehaviour, IPoolable
{
	private enum ProgressCellType
	{
		LeftCorner = 0,
		RightCorner = 1,
		Middle = 3,
		Single = 4
	}

	[SerializeField]
	private ProgressCellVisualVariation middleVar;

	[SerializeField]
	private ProgressCellVisualVariation singleVar;

	[SerializeField]
	private ProgressCellVisualVariation sideLVar;

	[SerializeField]
	private ProgressCellVisualVariation sideRVar;

	[SerializeField]
	private Image starIcon;

	[SerializeField]
	private GameObject plusOneObject;

	private ProgressCellVisualVariation currentVar;

	private bool isHovered;

	private PerkDef perkDefBonus;

	private ItemDef itemDefBonus;

	public bool Show(int currentIndex, int collectionCount, bool isEmpty, bool isFailed = false, int quality = -1, PerkDef perkDef = null, ItemDef itemDef = null)
	{
		ProgressCellType progressCellType = GetProgressCellType(currentIndex, collectionCount);
		ProgressCellVisualVariation progressCellVisualVariation = currentVar;
		bool result = false;
		currentVar = GetProgressCellVariation(progressCellType);
		if (progressCellVisualVariation != currentVar)
		{
			if (progressCellVisualVariation != null)
			{
				progressCellVisualVariation.gameObject.SetActive(value: false);
			}
			result = true;
		}
		if (!currentVar.gameObject.activeSelf)
		{
			currentVar.gameObject.SetActive(value: true);
			result = true;
		}
		currentVar.back.localScale = new Vector3(1f, 1f, 1f);
		currentVar.success.gameObject.SetActive(!isEmpty && !isFailed);
		currentVar.failed.gameObject.SetActive(!isEmpty && isFailed);
		((RectTransform)currentVar.transform).RefreshContentFitter();
		UpdateStarIcon(quality);
		perkDefBonus = perkDef;
		itemDefBonus = itemDef;
		return result;
	}

	public void Hide()
	{
		DeactivateAllVariations();
		currentVar = null;
		base.gameObject.SetActive(value: false);
		perkDefBonus = null;
		itemDefBonus = null;
	}

	public void OnPoolableObjReleased()
	{
		Hide();
	}

	public void OnOver()
	{
	}

	public void OnOut()
	{
	}

	public void ShowUITooltip()
	{
		isHovered = true;
		UITooltip.ShowProgressTickBonus(this, perkDefBonus, itemDefBonus);
	}

	public void HideUITooltip(bool immediately = false)
	{
		isHovered = false;
		if (immediately)
		{
			UITooltip.HideImmediately();
		}
		else
		{
			UITooltip.Hide();
		}
	}

	private void UpdateStarIcon(int quality)
	{
		if (quality <= 0)
		{
			if (quality == 0)
			{
				starIcon.gameObject.SetActive(value: false);
				plusOneObject.SetActive(value: true);
			}
			else
			{
				starIcon.gameObject.SetActive(value: false);
				plusOneObject.SetActive(value: false);
			}
		}
		else
		{
			starIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + quality);
			starIcon.gameObject.SetActive(value: true);
			plusOneObject.SetActive(value: false);
		}
	}

	private ProgressCellType GetProgressCellType(int currentIndex, int collectionCount = 1)
	{
		if (collectionCount == 1)
		{
			return ProgressCellType.Single;
		}
		if (currentIndex == 0)
		{
			return ProgressCellType.LeftCorner;
		}
		if (currentIndex == collectionCount - 1)
		{
			return ProgressCellType.RightCorner;
		}
		return ProgressCellType.Middle;
	}

	private void DeactivateAllVariations()
	{
		middleVar.gameObject.SetActive(value: false);
		singleVar.gameObject.SetActive(value: false);
		sideLVar.gameObject.SetActive(value: false);
		sideRVar.gameObject.SetActive(value: false);
	}

	private ProgressCellVisualVariation GetProgressCellVariation(ProgressCellType progressCellType)
	{
		ProgressCellVisualVariation result = null;
		switch (progressCellType)
		{
		case ProgressCellType.LeftCorner:
			result = sideLVar;
			break;
		case ProgressCellType.RightCorner:
			result = sideRVar;
			break;
		case ProgressCellType.Middle:
			result = middleVar;
			break;
		case ProgressCellType.Single:
			result = singleVar;
			break;
		}
		return result;
	}
}
