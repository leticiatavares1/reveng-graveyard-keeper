public class BubbleWidgetProgressData : BubbleWidgetData
{
	public delegate float ProgressDelegate();

	public ProgressDelegate progress_delegate;

	public int offset_x = 4;

	public int offset_y = 4;

	public BubbleWidgetProgressData(ProgressDelegate progress_delegate, int offset_x = 0, int offset_y = 2)
	{
		this.progress_delegate = progress_delegate;
		this.offset_x = offset_x;
		this.offset_y = offset_y;
		widget_id = WidgetID.Progress;
	}
}
