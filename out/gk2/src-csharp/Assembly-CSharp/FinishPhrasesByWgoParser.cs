using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public static class FinishPhrasesByWgoParser
{
	private const string PHRASES_BY_WGO_ASSET_PATH = "Assets/AddressableAssets/Helpers/FinishPhrasesByWgo.asset";

	private const string PHRASES_BY_WGO_ASSET_DIRECTORY = "/AddressableAssets/Helpers";

	private static FinishPhrasesByWgoData phrasesByWgoData;

	private static Dictionary<string, PhrasesByWgo> phrasesByWgoId;

	public static FinishPhrasesByWgoData PhrasesByWgoData
	{
		get
		{
			if (phrasesByWgoData == null)
			{
				phrasesByWgoData = Addressables.LoadAssetAsync<FinishPhrasesByWgoData>("Assets/AddressableAssets/Helpers/FinishPhrasesByWgo.asset").WaitForCompletion();
			}
			return phrasesByWgoData;
		}
	}

	private static Dictionary<string, PhrasesByWgo> PhrasesByWgoId
	{
		get
		{
			if (phrasesByWgoId == null)
			{
				phrasesByWgoId = new Dictionary<string, PhrasesByWgo>();
				List<PhrasesByWgo> list = PhrasesByWgoData?.finishPhrasesByWgo;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						PhrasesByWgo phrasesByWgo = list[i];
						if (phrasesByWgo != null && !string.IsNullOrEmpty(phrasesByWgo.wgoId))
						{
							phrasesByWgoId[phrasesByWgo.wgoId] = phrasesByWgo;
						}
					}
				}
			}
			return phrasesByWgoId;
		}
	}

	public static bool HasFinishPhrases(string wgoId)
	{
		if (!string.IsNullOrEmpty(wgoId))
		{
			return PhrasesByWgoId.ContainsKey(wgoId);
		}
		return false;
	}

	public static bool TryGetFinishPhrases(string wgoId, out PhrasesByWgo phrasesByWgo)
	{
		phrasesByWgo = null;
		if (string.IsNullOrEmpty(wgoId))
		{
			return false;
		}
		return PhrasesByWgoId.TryGetValue(wgoId, out phrasesByWgo);
	}

	private static void InvalidatePhrasesIndex()
	{
		phrasesByWgoId = null;
	}
}
