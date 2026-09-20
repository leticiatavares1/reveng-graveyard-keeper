using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class DialogDataContainer : ScriptableObject
{
	public List<DialogData> dialogDataList = new List<DialogData>();

	private Dictionary<string, DialogData> hash = new Dictionary<string, DialogData>();

	private static DialogDataContainer cachedInstance;

	public static DialogDataContainer Instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = Resources.Load<DialogDataContainer>("Locales/DialogData");
				cachedInstance.Initialize();
			}
			return cachedInstance;
		}
	}

	private void Initialize()
	{
		hash.Clear();
		foreach (DialogData dialogData in dialogDataList)
		{
			hash.Add(dialogData.id, dialogData);
		}
	}

	public static DialogData Get(string localeKey)
	{
		if (Instance.hash.TryGetValue(localeKey, out var value))
		{
			return value;
		}
		Debug.LogError($"Cannot find {typeof(DialogData)} for localeKey [{localeKey}].");
		return null;
	}

	public static bool TryGet(string localeKey, out DialogData result)
	{
		return Instance.hash.TryGetValue(localeKey, out result);
	}

	public static bool IsVoiceOverMuted(string localeKey)
	{
		if (string.IsNullOrEmpty(localeKey))
		{
			return false;
		}
		string text = LLBase.ResolveAlias(localeKey);
		if (TryGet(text, out var result) && result.modificator == LL.LocModificator.VoiceOverMuted)
		{
			return true;
		}
		if (text != localeKey && TryGet(localeKey, out result) && result.modificator == LL.LocModificator.VoiceOverMuted)
		{
			return true;
		}
		return false;
	}
}
