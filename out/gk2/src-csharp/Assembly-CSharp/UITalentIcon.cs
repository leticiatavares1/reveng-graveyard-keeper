using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITalentIcon : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI talentIconImage;

	[SerializeField]
	private TextMeshProUGUI masteryLockLabel;

	[SerializeField]
	private TextStyleComponent masteryLockTextStyle;

	[SerializeField]
	private TextStyle normalStyle;

	[SerializeField]
	private TextStyle lockStyle;

	[SerializeField]
	private TextStyle startStyle;

	public void Draw(TalentDef talentDef, int masteryLock, bool isEnoughMastery)
	{
		masteryLockLabel.gameObject.SetActive(!isEnoughMastery);
		masteryLockTextStyle.SetTextStyle(isEnoughMastery ? normalStyle : lockStyle);
		masteryLockLabel.text = masteryLock.ToString();
		DrawTalentIcon(talentDef);
		base.gameObject.SetActive(value: true);
	}

	public void Draw(TalentDef talentDef, int masteryLock, bool isEnoughMastery, bool isStar)
	{
		if (talentDef == null)
		{
			Hide();
			return;
		}
		masteryLockLabel.gameObject.SetActive(value: true);
		masteryLockLabel.text = masteryLock.ToString();
		masteryLockTextStyle.SetTextStyle(isStar ? startStyle : (isEnoughMastery ? normalStyle : lockStyle));
		DrawTalentIcon(talentDef);
		base.gameObject.SetActive(value: true);
	}

	public void Draw(TalentDef talentDef, string masteryValue)
	{
		if (talentDef == null)
		{
			Hide();
			return;
		}
		masteryLockLabel.gameObject.SetActive(value: true);
		masteryLockLabel.text = masteryValue;
		masteryLockTextStyle.SetTextStyle(normalStyle);
		DrawTalentIcon(talentDef);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void DrawTalentIcon(TalentDef talentDef)
	{
		talentIconImage.text = talentDef.id.FontIcon();
	}
}
