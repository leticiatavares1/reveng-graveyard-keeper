using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class NestedLocalesMetaInfo
{
	public bool hasMetaInfo;

	public List<int> idsToInsert = new List<int>();

	public List<string> localeIDsToInsert = new List<string>();

	public void AddEntry(int idxToInsert, string localeIDToInsert)
	{
		hasMetaInfo = true;
		idsToInsert.Add(idxToInsert);
		localeIDsToInsert.Add(localeIDToInsert);
	}
}
