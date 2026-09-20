using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class ReplacementKeysMetadata
{
	public string id;

	public List<int> keysIndexes = new List<int>();

	public List<string> keys = new List<string>();
}
