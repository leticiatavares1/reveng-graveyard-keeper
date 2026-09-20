using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class OverrideTextStyleData
{
	public List<string> languages = new List<string>();

	public TextStyleData data;
}
