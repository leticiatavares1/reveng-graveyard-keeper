using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeCharReputationWidget : TechTreeElementBaseWidget
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI idLabel;

	[SerializeField]
	private TextMeshProUGUI repLabel;

	[SerializeField]
	private TextStyle repEnough;

	[SerializeField]
	private TextStyle repNotEnough;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public override void Redraw()
	{
		base.Redraw();
		if (data.techDef.techDefType == TechDefType.CharRep)
		{
			WGODef dataOrNull = GameBalance.Me.GetDataOrNull<WGODef>(data.techDef.wgoRepLock.List[0].type);
			if (dataOrNull != null)
			{
				icon.sprite = dataOrNull.Portrait;
			}
			else
			{
				icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.techDef.customIconId);
			}
		}
		else
		{
			icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.techDef.customIconId);
		}
		icon.enabled = icon.sprite != null;
		idLabel.gameObject.SetActive(icon.sprite == null);
		icon.SetNativeSize();
		button.interactable = false;
		repLabel.transform.parent.gameObject.SetActive(value: true);
		icon.transform.parent.gameObject.SetActive(value: true);
		hiddenObj.SetActive(value: false);
		visibleObj.SetActive(value: false);
		availableObj.SetActive(value: false);
		unlockedObj.SetActive(value: false);
		TechState visualTechState = data.VisualTechState;
		if (data.techDef.EnoughResources)
		{
			repEnough.ApplyStyle(repLabel);
		}
		else
		{
			repNotEnough.ApplyStyle(repLabel);
		}
		if (data.techDef.techDefType == TechDefType.CharRep)
		{
			idLabel.text = data.techDef.CharReputationLock.List[0].type;
			repLabel.text = data.techDef.CharReputationLock.List[0].value.ToString();
		}
		else
		{
			idLabel.text = data.techDef.districtReputationLock.List[0].type;
			repLabel.text = data.techDef.districtReputationLock.List[0].value.ToString();
		}
		switch (visualTechState)
		{
		case TechState.Hidden:
			hiddenObj.SetActive(value: true);
			repLabel.transform.parent.gameObject.SetActive(value: false);
			icon.transform.parent.gameObject.SetActive(value: false);
			break;
		case TechState.Visible:
			visibleObj.SetActive(value: true);
			break;
		case TechState.Available:
			visibleObj.SetActive(value: true);
			break;
		case TechState.Unlocked:
			unlockedObj.SetActive(value: true);
			repLabel.transform.parent.gameObject.SetActive(value: false);
			break;
		}
		icon.BlueColorReplace(toReplace);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new TechTreeCharReputationWidgetData());
	}
}
