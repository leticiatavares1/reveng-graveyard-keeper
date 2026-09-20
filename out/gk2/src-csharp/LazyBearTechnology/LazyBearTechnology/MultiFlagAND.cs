using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

public class MultiFlagAND<T> : MultiFlagBase<T> where T : Enum
{
	protected override bool DefaultValueForMissingFlag => true;

	protected override bool ExceptResultOnMatch => false;

	protected override bool ExceptResultDefault => true;

	protected override bool ComparedFlagsResultOnMatch => false;

	protected override bool ComparedFlagsResultDefault => true;

	protected override bool IsExceptMatch(bool flagValue)
	{
		return !flagValue;
	}

	protected override bool IsComparedFlagsMatch(bool flagValue)
	{
		return !flagValue;
	}

	protected override void CalcResult()
	{
		bool flag = true;
		foreach (KeyValuePair<T, bool> flag2 in flags)
		{
			flag &= flag2.Value;
			if (!flag)
			{
				break;
			}
		}
		SetResultFlagAndNotify(flag);
	}
}
