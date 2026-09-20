using System;

[Serializable]
public abstract class SaveFixOperation
{
	public bool isEnabled = true;

	public virtual int InfoCount => 1;

	public virtual string Summary
	{
		get
		{
			string summaryBody = SummaryBody;
			string text = Info();
			if (!string.IsNullOrEmpty(summaryBody))
			{
				return "[" + text + "]  " + summaryBody;
			}
			return "[" + text + "]";
		}
	}

	protected virtual string SummaryBody => null;

	public abstract string Info();

	public abstract void Apply(SaveFixContext ctx);
}
