using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkerIcon : MonoBehaviour
{
	[Serializable]
	private class TalentViewData
	{
		public string talentId;

		public Sprite backSprite;

		public TextStyle talentStyle;
	}

	private const string BODY_SPRITE_PART_WITHOUT_ID = "_bdy_static_down";

	private const string ARMS_SPRITE_PART_WITHOUT_ID = "_arm_static_down";

	private const string HEAD_SPRITE_PART_WITHOUT_ID = "_hed_static_down";

	private const string BEARD_SPRITE_PART_WITHOUT_ID = "_brd_static_down";

	private const string HAIRSTYLE_SPRITE_PART_WITHOUT_ID = "_hrs_static_down";

	private const string BODY_OVER_SPRITE_PART_WITHOUT_ID = "_bdy_over_static_down";

	private const string STONE_SPRITE_ID = "1001_stn_static_down";

	[SerializeField]
	private GameObject talentParent;

	[SerializeField]
	private RectTransform workerIconParent;

	[SerializeField]
	private Vector2 workerIconPositionWithTalent = new Vector2(0f, 8f);

	[SerializeField]
	private Vector2 workerIconPositionWithoutTalent = new Vector2(0f, 2f);

	[SerializeField]
	private Image talentBackImage;

	[SerializeField]
	private TextMeshProUGUI talentIconLabel;

	[SerializeField]
	private TextMeshProUGUI masteryValueLabel;

	[SerializeField]
	private TextStyleComponent masteryValueTextStyleComponent;

	[SerializeField]
	private List<TalentViewData> viewDatas = new List<TalentViewData>();

	[SerializeField]
	private Image hed;

	[SerializeField]
	private Image brd;

	[SerializeField]
	private Image hrs;

	[SerializeField]
	private Image bdy;

	[SerializeField]
	private Image arms;

	[SerializeField]
	private Image hed2;

	[SerializeField]
	private Image brd2;

	[SerializeField]
	private Image hrs2;

	[SerializeField]
	private Image bdy2;

	[SerializeField]
	private Image arms2;

	[SerializeField]
	private Image stn;

	[SerializeField]
	private Image stn2;

	[SerializeField]
	private Image bdyOver;

	[SerializeField]
	private Image bdyOver2;

	private bool materialsInitialized;

	public RectTransform WorkerIconParent => workerIconParent;

	private void TryInitMaterials()
	{
		if (!materialsInitialized)
		{
			materialsInitialized = true;
			hed.material = new Material(hed.material);
			hed2.material = new Material(hed2.material);
			bdy.material = new Material(bdy.material);
			bdy2.material = new Material(bdy2.material);
			hrs.material = new Material(hrs.material);
			hrs2.material = new Material(hrs2.material);
			arms.material = new Material(arms.material);
			arms2.material = new Material(arms2.material);
			brd.material = new Material(brd.material);
			brd2.material = new Material(brd2.material);
			bdyOver.material = new Material(bdyOver.material);
			bdyOver2.material = new Material(bdyOver2.material);
			stn.material = new Material(stn.material);
			stn2.material = new Material(stn2.material);
		}
	}

	public void Show(IWorker worker, SkinPresetGK2 skinPreset, TalentDef talentDef)
	{
		if (talentDef == null)
		{
			SetTalentActive(active: false);
			base.gameObject.SetActive(value: true);
			return;
		}
		ShowSkin(skinPreset);
		TalentViewData talentViewData = viewDatas.Find((TalentViewData d) => d.talentId == talentDef.id);
		if (talentViewData == null)
		{
			Debug.LogError("No view data for talent " + talentDef.id);
			SetTalentActive(active: false);
			return;
		}
		SetTalentActive(active: true);
		masteryValueLabel.text = worker.GetMasteryLevelForTalentBranch(talentDef.id).ToString();
		talentIconLabel.text = talentDef.id.FontIcon();
		talentBackImage.sprite = talentViewData.backSprite;
		masteryValueTextStyleComponent.SetTextStyle(talentViewData.talentStyle);
		EnsureMasteryMouseTooltip("tt_craft_mastery");
	}

	public void SetTalentValue(TalentDef talentDef, int value)
	{
		SetTalentActive(active: true);
		TalentViewData talentViewData = viewDatas.Find((TalentViewData d) => d.talentId == talentDef.id);
		if (talentViewData == null)
		{
			Debug.LogError("No view data for talent " + talentDef.id);
			SetTalentActive(active: false);
			return;
		}
		SetTalentActive(active: true);
		masteryValueLabel.text = value.ToString();
		talentIconLabel.text = talentDef.id.FontIcon();
		talentBackImage.sprite = talentViewData.backSprite;
		masteryValueTextStyleComponent.SetTextStyle(talentViewData.talentStyle);
		EnsureMasteryMouseTooltip("tt_garden_mastery");
	}

	public void ShowWithoutTalent([CanBeNull] IWorker worker, SkinPresetGK2 skinPreset)
	{
		SetTalentActive(active: false);
		ShowSkin(skinPreset);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void SetTalentActive(bool active)
	{
		talentParent.SetActive(active);
		workerIconParent.anchoredPosition = (talentParent.activeSelf ? workerIconPositionWithTalent : workerIconPositionWithoutTalent);
	}

	private void EnsureMasteryMouseTooltip(string tooltipId)
	{
		if (!(talentBackImage == null))
		{
			UIMouseTooltip.Attach(talentBackImage.gameObject, tooltipId);
			talentIconLabel.raycastTarget = false;
			masteryValueLabel.raycastTarget = false;
		}
	}

	private void ShowSkin(SkinPresetGK2 skinPreset)
	{
		TryInitMaterials();
		hed.gameObject.SetActive(value: false);
		bdy.gameObject.SetActive(value: false);
		brd.gameObject.SetActive(value: false);
		arms.gameObject.SetActive(value: false);
		hrs.gameObject.SetActive(value: false);
		bdyOver.gameObject.SetActive(value: false);
		stn.gameObject.SetActive(value: false);
		if (skinPreset != null)
		{
			ApplySkinPart(hed, hed2, string.Format("{0}{1}", skinPreset.head.id, "_hed_static_down"), skinPreset, skinPreset.head);
			ApplySkinPart(bdy, bdy2, string.Format("{0}{1}", skinPreset.body.id, "_bdy_static_down"), skinPreset, skinPreset.body);
			if (skinPreset.body.id == 1002)
			{
				ApplySkinPart(bdyOver, string.Format("{0}{1}", skinPreset.body.id, "_bdy_over_static_down"));
			}
			if (skinPreset.isPlayerPreset)
			{
				ApplySkinPart(brd, brd2, string.Format("{0}{1}", skinPreset.beard.id, "_brd_static_down"), skinPreset, skinPreset.beard);
				ApplySkinPart(hrs, hrs2, string.Format("{0}{1}", skinPreset.hairstyle.id, "_hrs_static_down"), skinPreset, skinPreset.hairstyle);
				ApplySkinPart(arms, arms2, string.Format("{0}{1}", skinPreset.body.id, "_arm_static_down"), skinPreset, skinPreset.arms);
			}
			else
			{
				ApplySkinPart(stn, "1001_stn_static_down");
			}
		}
		base.gameObject.SetActive(value: true);
		hed2.gameObject.SetActive(hed.gameObject.activeSelf);
		hed2.sprite = hed.sprite;
		brd2.gameObject.SetActive(brd.gameObject.activeSelf);
		brd2.sprite = brd.sprite;
		hrs2.gameObject.SetActive(hrs.gameObject.activeSelf);
		hrs2.sprite = hrs.sprite;
		bdy2.gameObject.SetActive(bdy.gameObject.activeSelf);
		bdy2.sprite = bdy.sprite;
		arms2.gameObject.SetActive(arms.gameObject.activeSelf);
		arms2.sprite = arms.sprite;
		bdyOver2.gameObject.SetActive(bdyOver.gameObject.activeSelf);
		bdyOver2.sprite = bdyOver.sprite;
		stn2.gameObject.SetActive(stn.gameObject.activeSelf);
		stn2.sprite = stn.sprite;
	}

	private static void ApplySkinPart(Image image, string spriteName)
	{
		bool flag = LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(spriteName);
		image.gameObject.SetActive(flag);
		if (flag)
		{
			image.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
		}
	}

	private static void ApplySkinPart(Image image, Image colorImage, string spriteName, SkinPresetGK2 skinPreset, SkinPresetPartGK2 part)
	{
		ApplySkinPart(image, spriteName);
		if (image.gameObject.activeSelf)
		{
			skinPreset.TryToApply(colorImage, colorImage.name, part);
		}
	}
}
