using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TechDefinition : BalanceBaseObject
{
	public enum TechState
	{
		Purchased,
		Unavailable,
		AvailableForPurchase,
		Hidden,
		Invisible
	}

	public const int MAX_VISIBLE_UNLOCKS = 3;

	public int branch_type;

	public GameRes price;

	[NonSerialized]
	public List<TechDefinition> parents = new List<TechDefinition>();

	[NonSerialized]
	public List<TechDefinition> children = new List<TechDefinition>();

	public List<string> crafts = new List<string>();

	public List<string> works = new List<string>();

	public List<string> phrases = new List<string>();

	public List<string> perks = new List<string>();

	[SerializeField]
	private string[] _parents;

	public int x;

	public float y;

	public string icon;

	public string flowscript;

	public bool hidden;

	public bool invisible;

	public DLCEngine.DLCVersion requires_dlc;

	private static bool _initialized;

	private List<TechUnlock> _unlocks_list;

	public static List<string> TECH_POINTS = new List<string> { "r", "g", "b", "v", "gratitude_points" };

	public List<TechUnlock> GetUnlocksList()
	{
		if (_unlocks_list == null)
		{
			_unlocks_list = new List<TechUnlock>();
			foreach (string craft in crafts)
			{
				_unlocks_list.Add(new TechUnlock(craft, TechUnlock.TechUnlockType.Craft));
			}
			foreach (string work in works)
			{
				_unlocks_list.Add(new TechUnlock(work, TechUnlock.TechUnlockType.Work));
			}
			foreach (string phrase in phrases)
			{
				_unlocks_list.Add(new TechUnlock(phrase, TechUnlock.TechUnlockType.Phrase));
			}
			foreach (string perk in perks)
			{
				_unlocks_list.Add(new TechUnlock(perk, TechUnlock.TechUnlockType.Perk));
			}
		}
		return _unlocks_list;
	}

	public List<TechUnlock> GetVisibleUnlocksList()
	{
		List<TechUnlock> unlocksList = GetUnlocksList();
		List<TechUnlock> list = new List<TechUnlock>();
		foreach (TechUnlock item in unlocksList)
		{
			if (item.visible)
			{
				if (list.Count >= 3)
				{
					Debug.LogError("Can't draw more than " + 3 + " tech unlocks (MAX_VISIBLE_UNLOCKS const), tech_id = " + id);
					break;
				}
				list.Add(item);
			}
		}
		return list;
	}

	private void InitParentsAndChildren()
	{
		string[] array = _parents;
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				TechDefinition data = GameBalance.me.GetData<TechDefinition>(text);
				if (data == null)
				{
					Debug.LogError("no tech definition with id: " + text);
					continue;
				}
				data.AddChild(this);
				AddParent(data);
			}
		}
	}

	private void AddParent(TechDefinition tech)
	{
		if (!parents.Contains(tech))
		{
			parents.Add(tech);
		}
	}

	private void AddChild(TechDefinition tech)
	{
		if (!children.Contains(tech))
		{
			children.Add(tech);
		}
	}

	public void ApplyTech()
	{
		foreach (string perk in perks)
		{
			if (perk[0] == '@')
			{
				MainGame.me.save.UnlockPerk(perk.Substring(1));
			}
			else
			{
				MainGame.me.save.UnlockPerk(perk);
			}
		}
		if (!string.IsNullOrEmpty(flowscript))
		{
			GS.RunFlowScript(flowscript);
		}
	}

	public TechState GetState()
	{
		if (!DLCEngine.IsDLCAvailable(requires_dlc))
		{
			return TechState.Invisible;
		}
		if (invisible && !MainGame.me.save.visible_techs.Contains(id))
		{
			return TechState.Invisible;
		}
		if (hidden && !MainGame.me.save.revealed_techs.Contains(id))
		{
			return TechState.Hidden;
		}
		foreach (TechDefinition parent in parents)
		{
			if (parent.GetState() == TechState.Hidden)
			{
				return TechState.Hidden;
			}
		}
		if (MainGame.me.save.unlocked_techs.Contains(id))
		{
			return TechState.Purchased;
		}
		if (!MainGame.me.save.CanBuyTech(id))
		{
			return TechState.Unavailable;
		}
		return TechState.AvailableForPurchase;
	}

	public static void LinkTechs()
	{
		if (_initialized)
		{
			return;
		}
		foreach (TechDefinition techs_datum in GameBalance.me.techs_data)
		{
			techs_datum.InitParentsAndChildren();
			techs_datum.GetUnlocksList();
		}
		_initialized = true;
	}

	public void ResetLanguageCache()
	{
		if (_unlocks_list == null)
		{
			return;
		}
		foreach (TechUnlock item in _unlocks_list)
		{
			item.ResetLanguageCache();
		}
	}
}
