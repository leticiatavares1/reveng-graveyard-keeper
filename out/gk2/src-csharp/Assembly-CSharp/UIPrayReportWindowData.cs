using System;
using LazyBearTechnology;

public class UIPrayReportWindowData : LazyWidgetDataBase
{
	private Action OnClosed { get; set; }

	public SermonResultData SermonResultData { get; private set; }

	public UIPrayReportWindowData(SermonResultData sermonResultData, Action onClosed)
	{
		SermonResultData = sermonResultData;
		OnClosed = onClosed;
	}

	public void HandleClosingWindow()
	{
		OnClosed?.Invoke();
	}
}
