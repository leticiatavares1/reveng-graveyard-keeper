using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class ProgressCellCraft : MonoBehaviour
{
	[SerializeField]
	private ProgressCellVisualVariation commonVar;

	[SerializeField]
	private ProgressCellVisualVariation starVar;

	[SerializeField]
	private ProgressCellVisualVariation autopsyVar;

	[SerializeField]
	private Image starIcon;

	[SerializeField]
	private Image autopsyFailIcon;

	[SerializeField]
	private Image autopsySuccess;

	[SerializeField]
	private GameObject plusOneObject;

	private ProgressCellVisualVariation currentVar;

	public GameObject PlusOneObject => plusOneObject;

	public void Show(int currentIndex, int collectionCount, bool isEmpty, bool isFailed = false, int quality = -1, int autopsyQuality = -1, int gardenQuality = -1)
	{
		commonVar.gameObject.SetActive(value: false);
		starVar.gameObject.SetActive(value: false);
		autopsyVar.gameObject.SetActive(value: false);
		plusOneObject.SetActive(value: false);
		if (gardenQuality == 0)
		{
			plusOneObject.SetActive(value: true);
			autopsyFailIcon.gameObject.SetActive(value: false);
			autopsySuccess.gameObject.SetActive(value: false);
			currentVar = starVar;
		}
		else if (gardenQuality > 0)
		{
			starIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + gardenQuality);
			starIcon.gameObject.SetActive(value: true);
			autopsyFailIcon.gameObject.SetActive(value: false);
			autopsySuccess.gameObject.SetActive(value: false);
			currentVar = starVar;
		}
		else if (quality > 0)
		{
			starIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + quality);
			starIcon.gameObject.SetActive(value: true);
			autopsyFailIcon.gameObject.SetActive(value: false);
			autopsySuccess.gameObject.SetActive(value: false);
			currentVar = starVar;
		}
		else if (autopsyQuality > 0)
		{
			if (autopsyQuality == 1)
			{
				autopsyFailIcon.gameObject.SetActive(value: true);
				autopsySuccess.gameObject.SetActive(value: false);
			}
			else
			{
				autopsyFailIcon.gameObject.SetActive(value: false);
				autopsySuccess.gameObject.SetActive(value: true);
			}
			starIcon.gameObject.SetActive(value: false);
			currentVar = autopsyVar;
		}
		else
		{
			autopsyFailIcon.gameObject.SetActive(value: false);
			autopsySuccess.gameObject.SetActive(value: false);
			starIcon.gameObject.SetActive(value: false);
			currentVar = commonVar;
		}
		currentVar.gameObject.SetActive(value: true);
		currentVar.back.localScale = new Vector3(1f, 1f, 1f);
		currentVar.success.gameObject.SetActive(!isEmpty && !isFailed);
		currentVar.failed.gameObject.SetActive(!isEmpty && isFailed);
		((RectTransform)currentVar.transform).RefreshContentFitter();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
