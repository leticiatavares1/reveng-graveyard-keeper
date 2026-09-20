using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInspirationNotification : UIBaseNotification
{
	[Serializable]
	private class TalentViewData
	{
		public string talentId;

		public Sprite rightBack;

		public Sprite leftBack;
	}

	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI talentIconLabel;

	[SerializeField]
	private Image inspirationIcon;

	[SerializeField]
	private Image iconFrame;

	[SerializeField]
	private Color iconOutlineColor;

	[SerializeField]
	private Image backgroundRight;

	[SerializeField]
	private Image backgroundLeft;

	[SerializeField]
	private Sprite[] framesSprites;

	[SerializeField]
	private List<TalentViewData> viewDatas = new List<TalentViewData>();

	private TalentViewData currentViewData;

	public string InspirationTalentId { get; set; }

	public string InspirationId { get; set; }

	public override void Draw()
	{
		nameLabel.text = LLBase.L(InspirationId);
		InspirationDef data = GameBalance.Me.GetData<InspirationDef>(InspirationId);
		inspirationIcon.sprite = data.Icon;
		inspirationIcon.BlueColorReplace(iconOutlineColor);
		iconFrame.sprite = framesSprites[data.lvl - 1];
		talentIconLabel.text = InspirationTalentId.FontIcon();
		currentViewData = viewDatas.Find((TalentViewData d) => d.talentId == InspirationTalentId);
		backgroundRight.sprite = currentViewData.rightBack;
		backgroundLeft.sprite = currentViewData.leftBack;
		LazyAudio.Play("unlock");
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
