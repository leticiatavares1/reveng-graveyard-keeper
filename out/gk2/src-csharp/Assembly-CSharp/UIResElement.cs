using System;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResElement : LazyWidget<UIResElementData>
{
	private const float FOLLOW_TARGET_TIME = 0.5f;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TextStyleComponent labelTextStyle;

	[SerializeField]
	private TextStyle commonTextStyle;

	[SerializeField]
	private TextStyle energyTextStyle;

	[SerializeField]
	private TextStyle insanityTextStyle;

	[SerializeField]
	private float appearingTime;

	[SerializeField]
	private float moveTime;

	[SerializeField]
	private float idleTime;

	[SerializeField]
	private float hidingTime;

	[SerializeField]
	private Vector3 moveOffset;

	[SerializeField]
	private Ease easeType;

	private Sequence sequence;

	private Action<UIResElement> onAnimationCompleted;

	private Action<UIResElementData> onShowTimeComplete;

	private Vector3 originalPosition;

	private float currentFlyingTime;

	public UIResElementData Data => data;

	public void Init(Action<UIResElement> onAnimationCompleted)
	{
		this.onAnimationCompleted = onAnimationCompleted;
	}

	public override void Redraw()
	{
		base.Redraw();
		onShowTimeComplete = data.OnShowTimeComplete;
		DoAppearAnimation();
	}

	private void DoAppearAnimation()
	{
		FormLabelAndApplyStyle();
		if (data.DisplayingType == UIGameResDisplayingType.AppearOverTargetType)
		{
			label.alpha = 0f;
			label.transform.localPosition = Vector3.zero;
			sequence = DOTween.Sequence();
			sequence.Join(label.DOFade(1f, appearingTime));
			sequence.Join(label.transform.DOLocalMove(moveOffset, moveTime).SetEase(easeType));
			sequence.AppendInterval(idleTime);
			sequence.Append(label.DOFade(0f, hidingTime).OnComplete(OnAnimationEnded));
		}
		else
		{
			originalPosition = CameraSystem.WorldToScreenPoint(data.StartPosition);
			currentFlyingTime = 0f;
			label.alpha = 1f;
			label.transform.localPosition = Vector3.zero;
		}
	}

	public void ForceHide()
	{
		sequence?.Kill();
		OnAnimationEnded();
	}

	private void OnAnimationEnded()
	{
		onShowTimeComplete?.Invoke(data);
		onAnimationCompleted?.Invoke(this);
	}

	private void LateUpdate()
	{
		if (data.HasTarget && data.Target == null)
		{
			ForceHide();
			return;
		}
		if (data.DisplayingType == UIGameResDisplayingType.AppearOverTargetType)
		{
			if (data.IsUITarget)
			{
				base.transform.position = data.StartPosition;
			}
			else
			{
				base.transform.position = CameraSystem.WorldToScreenPoint(data.HasTarget ? data.Target.position : data.StartPosition);
			}
			return;
		}
		Vector3 b = (data.IsUITarget ? data.Target.position : CameraSystem.WorldToScreenPoint(data.Target.position));
		currentFlyingTime += Time.deltaTime;
		float num = currentFlyingTime / 0.5f;
		base.transform.position = Vector3.Lerp(originalPosition, b, num);
		if (num >= 1f)
		{
			OnAnimationEnded();
		}
	}

	private void FormLabelAndApplyStyle()
	{
		int showValue = data.GetShowValue();
		string text = ((showValue != 0) ? (showValue + data.IconId.FontIcon()) : data.IconId.FontIcon());
		label.text = ((showValue > 0) ? ("+" + text) : text);
		string iconId = data.IconId;
		if (!(iconId == "energy"))
		{
			if (iconId == "insanity")
			{
				labelTextStyle.SetTextStyle(insanityTextStyle);
			}
			else
			{
				labelTextStyle.SetTextStyle(commonTextStyle);
			}
		}
		else
		{
			labelTextStyle.SetTextStyle(energyTextStyle);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIResElementData("hint_inspiration", UIGameResDisplayingType.AppearOverTargetType, "hint_inspiration", 2f, null, isUITarget: false));
	}
}
