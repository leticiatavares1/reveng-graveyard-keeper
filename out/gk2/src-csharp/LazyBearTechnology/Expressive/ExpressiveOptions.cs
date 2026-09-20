using System;

namespace Expressive;

[Flags]
public enum ExpressiveOptions
{
	None = 1,
	[Obsolete("This will be removed in future versions with IgnoreCaseForParsing, IgnoreCaseForEquality and IgnoreCaseAll replacing it.")]
	IgnoreCase = 2,
	NoCache = 4,
	RoundAwayFromZero = 8,
	IgnoreCaseForParsing = 0x10,
	IgnoreCaseForEquality = 0x20,
	IgnoreCaseAll = 0x30,
	All = 0x3E
}
