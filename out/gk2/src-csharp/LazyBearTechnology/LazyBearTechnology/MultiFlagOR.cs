using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

public class MultiFlagOR<T> : MultiFlagBase<T> where T : Enum
{
	protected override bool DefaultValueForMissingFlag => false;

	protected override bool ExceptResultOnMatch => true;

	protected override bool ExceptResultDefault => false;

	protected override bool ComparedFlagsResultOnMatch => true;

	protected override bool ComparedFlagsResultDefault => false;

	protected override bool IsExceptMatch(bool flagValue)
	{
		return flagValue;
	}

	protected override bool IsComparedFlagsMatch(bool flagValue)
	{
		return flagValue;
	}

	protected override void CalcResult()
	{
		bool flag = false;
		foreach (KeyValuePair<T, bool> flag2 in flags)
		{
			flag |= flag2.Value;
			if (flag)
			{
				break;
			}
		}
		SetResultFlagAndNotify(flag);
	}
}
