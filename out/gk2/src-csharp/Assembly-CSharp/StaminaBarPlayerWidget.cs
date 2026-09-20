using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarPlayerWidget : LazyWidget<StaminaBarPlayerWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private RectTransform barImageParentRectTransform;

	[SerializeField]
	private Gradient gradient;

	[SerializeField]
	private float animationTime;

	[SerializeField]
	private Image animatableImage;

	private bool isAnimating;

	private float animatingTimer;

	protected override void SetData(StaminaBarPlayerWidgetData data)
	{
		base.SetData(data);
		data.onNotEnoughStamina = SetAnimatingState;
		data.SubscribeToDataChanges();
	}

	public override void Redraw()
	{
		base.Redraw();
		slider.maxValue = data.StaminaResSystem.Max;
		int num = BarWigdetUtils.ClampSliderValueToViewableState(Mathf.CeilToInt(barImageParentRectTransform.rect.width), Mathf.RoundToInt(data.StaminaResSystem.Get()), Mathf.RoundToInt(data.StaminaResSystem.Max));
		slider.value = num;
	}

	public override void CustomUpdate()
	{
		if (isAnimating)
		{
			PlayNotEnoughEffect();
		}
		int num = BarWigdetUtils.ClampSliderValueToViewableState(Mathf.CeilToInt(barImageParentRectTransform.rect.width), Mathf.RoundToInt(data.StaminaResSystem.Get()), Mathf.RoundToInt(data.StaminaResSystem.Max));
		slider.value = num;
	}

	private void PlayNotEnoughEffect()
	{
		animatingTimer += Time.deltaTime;
		animatableImage.color = gradient.Evaluate(animatingTimer / animationTime);
		if (animatingTimer >= animationTime)
		{
			isAnimating = false;
			animatingTimer = 0f;
		}
	}

	private void SetAnimatingState()
	{
		isAnimating = true;
	}

	public override void Hide()
	{
		base.Hide();
		data.UnsubscribeFromDataChanges();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new StaminaBarPlayerWidgetData());
	}
}
