using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class BalanceTabAttribute : Attribute
{
	public string TabName { get; }

	public int StartRow { get; }

	public BalanceTabAttribute(string tabName, int startRow = -1)
	{
		TabName = tabName;
		StartRow = startRow;
	}
}
