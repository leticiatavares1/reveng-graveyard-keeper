using LazyBearTechnology;

public class ProgressBarWidget_SimplifiedData : LazyWidgetDataBase
{
	public int CellCount;

	public int GreenValue;

	public int RedValue;

	public ProgressBarWidget_SimplifiedData(int cellCount, int greenValue, int redValue = 0)
	{
		CellCount = cellCount;
		GreenValue = greenValue;
		RedValue = redValue;
	}
}
