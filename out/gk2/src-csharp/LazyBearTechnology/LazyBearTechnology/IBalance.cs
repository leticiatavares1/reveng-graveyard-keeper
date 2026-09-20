using System.Collections;
using System.Collections.Generic;

namespace LazyBearTechnology;

public interface IBalance
{
	void InitBalance();

	void ClearBalance();

	Dictionary<string, IList> GetAllDataListsAndGoogleTabs();

	Dictionary<string, IList> GetAllTabs();
}
