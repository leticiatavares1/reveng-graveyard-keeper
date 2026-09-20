using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class UISleepFade : UIBasicFade
{
	private const string BEARD_SPRITE_PART_WITHOUT_ID = "_brd_static_down";

	private const string HAIRSTYLE_SPRITE_PART_WITHOUT_ID = "_hrs_static_down";

	private static readonly int SleepAnim = Animator.StringToHash("SleepAnimType");

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private GameObject bed;

	[SerializeField]
	private Image hrs;

	[SerializeField]
	private Image brd;

	private Canvas canvas;

	private SleepAnimType sleepAnimType;

	public override void Init()
	{
		base.Init();
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
		hrs.material = new Material(hrs.material);
		brd.material = new Material(brd.material);
		canvas.sortingOrder = 49;
	}

	public void FadeIn(Action onComplete, FadeFlag fadeFlag = FadeFlag.Common, bool blockInterceptions = false, SleepAnimType sleepAnimType = SleepAnimType.None, bool instant = false)
	{
		this.sleepAnimType = sleepAnimType;
		ApplyPlayerSkin();
		if (instant)
		{
			FadeInInstant(fadeFlag);
			onComplete?.Invoke();
		}
		else
		{
			base.FadeIn(onComplete, fadeFlag, blockInterceptions);
		}
		if (sleepAnimType == SleepAnimType.None)
		{
			bed.SetActive(value: false);
			return;
		}
		bed.SetActive(value: true);
		animator.SetInteger(SleepAnim, (int)sleepAnimType);
	}

	public void FadeOut(Action onComplete, bool instant = false)
	{
		if (sleepAnimType != 0)
		{
			bed.SetActive(value: false);
		}
		if (instant)
		{
			FadeOutInstant();
			onComplete?.Invoke();
		}
		else
		{
			FadeOut(onComplete, FadeFlag.Common);
		}
	}

	private void ApplyPlayerSkin()
	{
		SkinPresetGK2 skinPreset = MainGame.PlayerController.View.PlayerAnimation.SkinPreset;
		ApplySkinPart(brd, string.Format("{0}{1}", skinPreset.beard.id, "_brd_static_down"), skinPreset, skinPreset.beard);
		ApplySkinPart(hrs, string.Format("{0}{1}", skinPreset.hairstyle.id, "_hrs_static_down"), skinPreset, skinPreset.hairstyle);
	}

	private static void ApplySkinPart(Image image, string spriteName, SkinPresetGK2 skinPreset, SkinPresetPartGK2 part)
	{
		bool flag = LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(spriteName);
		image.gameObject.SetActive(flag);
		if (flag)
		{
			image.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
			skinPreset.TryToApply(image, image.name, part);
		}
	}
}
