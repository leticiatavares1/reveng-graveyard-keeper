public class BubbleWidgetProgress : BubbleWidget<BubbleWidgetProgressData>
{
	public UI2DSprite progress_bar;

	public UI2DSprite back;

	public override void Draw(BubbleWidgetProgressData data)
	{
		if (!initialized)
		{
			Init();
		}
		base.data = data;
		ui_widget.width = progress_bar.width + data.offset_x * 2;
		ui_widget.height = progress_bar.height + data.offset_y * 2;
		UpdateWidget();
	}

	public override void UpdateWidget()
	{
		float num = data.progress_delegate();
		progress_bar.fillAmount = num;
		if (num < 0f)
		{
			ui_widget.alpha = 0f;
		}
		else if (ui_widget.alpha.EqualsTo(0f))
		{
			ui_widget.alpha = 1f;
		}
	}
}
