using UnityEngine;
using UnityEngine.UI;

public class UIProgressCellsInfoWidgetCell : MonoBehaviour
{
	[SerializeField]
	private UIProgressCellsInfoWidgetCellVariation commonVarEmpty;

	[SerializeField]
	private UIProgressCellsInfoWidgetCellVariation commonVarGreen;

	[SerializeField]
	private UIProgressCellsInfoWidgetCellVariation percentVar;

	[SerializeField]
	private Image percentProgressBar;

	[SerializeField]
	private UIProgressCellsInfoWidgetCellVariation chanceVar;

	private UIProgressCellsInfoWidgetCellVariation currentVar;

	public void Show(bool isEmpty, bool isChance, float fillValue = 1f)
	{
		commonVarGreen.gameObject.SetActive(value: false);
		commonVarEmpty.gameObject.SetActive(value: false);
		chanceVar.gameObject.SetActive(value: false);
		percentVar.gameObject.SetActive(value: false);
		if (isEmpty)
		{
			currentVar = commonVarEmpty;
		}
		else if (isChance)
		{
			currentVar = chanceVar;
		}
		else if (fillValue >= 1f)
		{
			currentVar = commonVarGreen;
		}
		else
		{
			currentVar = percentVar;
			percentProgressBar.fillAmount = fillValue;
		}
		currentVar.gameObject.SetActive(value: true);
		((RectTransform)currentVar.transform).RefreshContentFitter();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
