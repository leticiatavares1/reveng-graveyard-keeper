using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Expressive;
using Expressive.Exceptions;
using Expressive.Expressions;
using Sirenix.Utilities;
using UnityEngine;

[Serializable]
public class SmartExpression
{
	[SerializeField]
	private string _expression = "";

	[SerializeField]
	private bool _simplified;

	[SerializeField]
	private float _simpified_float;

	private Expression _exp;

	private WorldGameObject _wgo;

	private WorldGameObject _character;

	[NonSerialized]
	private List<Item> _items;

	public float default_value;

	private static Dictionary<string, string> equatings = new Dictionary<string, string>
	{
		{ "\\$(\\w*) *\\+= *(.*)", "AddPpar" },
		{ "\\$(\\w*) *\\-= *(.*)", "DecPpar" },
		{ "\\$(\\w*) *\\*= *(.*)", "MultiplyPpar" },
		{ "\\$(\\w*) *\\/= *(.*)", "DividePpar" },
		{ "\\$(\\w*) *\\= *([^=].*)", "SetPpar" }
	};

	public bool has_expression => !string.IsNullOrEmpty(_expression);

	private static WorldGameObject player => MainGame.me.player;

	private static GameSave save => MainGame.me.save;

	public void FromString(string s)
	{
		s = s.Replace("&quot;", "\"");
		_expression = s;
		_exp = null;
		_simplified = float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _simpified_float);
	}

	public bool HasNoExpresion()
	{
		return string.IsNullOrEmpty(_expression);
	}

	private void CheckExpressionInit()
	{
		if (_exp != null || _simplified)
		{
			return;
		}
		_exp = new Expression(_expression);
		_exp.RegisterFunction("WGOpar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return 0;
			}
			return _wgo.GetParam(pars[0].EvaluateAsString(values));
		});
		_exp.RegisterFunction("Ppar", (IExpression[] pars, IDictionary<string, object> values) => player.GetParam(pars[0].EvaluateAsString(values)));
		_exp.RegisterFunction("Ife", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			float a = pars[0].EvaluateAsFloat(values);
			float b = pars[1].EvaluateAsFloat(values);
			return a.EqualsTo(b, 0.001f) ? ((object)1f) : ((object)0);
		});
		_exp.RegisterFunction("Ifn", (IExpression[] pars, IDictionary<string, object> values) => pars[0].EvaluateAsBoolean(values) ? 1f : 0f);
		_exp.RegisterFunction("GetDay", (IExpression[] pars, IDictionary<string, object> values) => save.day);
		_exp.RegisterFunction("IsDay", (IExpression[] pars, IDictionary<string, object> values) => (!TimeOfDay.me.is_night) ? 1f : 0f);
		_exp.RegisterFunction("IsNight", (IExpression[] pars, IDictionary<string, object> values) => TimeOfDay.me.is_night ? 1f : 0f);
		_exp.RegisterFunction("GetTime", (IExpression[] pars, IDictionary<string, object> values) => TimeOfDay.me.GetTimeK());
		_exp.RegisterFunction("GetTotalTime", (IExpression[] pars, IDictionary<string, object> values) => MainGame.game_time);
		_exp.RegisterFunction("GetSessionTime", (IExpression[] pars, IDictionary<string, object> values) => MainGame.session_time);
		_exp.RegisterFunction("SetPpar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			player.SetParam(pars[0].EvaluateAsString(values), pars[1].EvaluateAsFloat(values));
			return player.GetParam(pars[0].EvaluateAsString(values));
		});
		_exp.RegisterFunction("AddPpar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text17 = pars[0].EvaluateAsString(values);
			float num5 = player.GetParam(text17) + pars[1].EvaluateAsFloat(values);
			switch (text17)
			{
			case "hp":
				if (num5 > (float)MainGame.me.save.max_hp)
				{
					num5 = MainGame.me.save.max_hp;
				}
				break;
			case "energy":
				if (num5 < 0f)
				{
					num5 = 0f;
				}
				else if (num5 > (float)MainGame.me.save.max_energy)
				{
					num5 = MainGame.me.save.max_energy;
				}
				break;
			}
			player.SetParam(text17, num5);
			return num5;
		});
		_exp.RegisterFunction("DecPpar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text16 = pars[0].EvaluateAsString(values);
			float num4 = player.GetParam(text16) - pars[1].EvaluateAsFloat(values);
			switch (text16)
			{
			case "hp":
				if (num4 > (float)MainGame.me.save.max_hp)
				{
					num4 = MainGame.me.save.max_hp;
				}
				break;
			case "energy":
				if (num4 < 0f)
				{
					num4 = 0f;
				}
				else if (num4 > (float)MainGame.me.save.max_energy)
				{
					num4 = MainGame.me.save.max_energy;
				}
				break;
			}
			player.SetParam(text16, num4);
			return num4;
		});
		_exp.RegisterFunction("MultiplyPpar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text15 = pars[0].EvaluateAsString(values);
			float num3 = player.GetParam(text15) * pars[1].EvaluateAsFloat(values);
			switch (text15)
			{
			case "hp":
				if (num3 > (float)MainGame.me.save.max_hp)
				{
					num3 = MainGame.me.save.max_hp;
				}
				break;
			case "energy":
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				else if (num3 > (float)MainGame.me.save.max_energy)
				{
					num3 = MainGame.me.save.max_energy;
				}
				break;
			}
			player.SetParam(text15, num3);
			return num3;
		});
		_exp.RegisterFunction("DividePpar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text14 = pars[0].EvaluateAsString(values);
			float num2 = pars[1].EvaluateAsFloat(values);
			float param = player.GetParam(text14);
			if (num2.EqualsTo(0f))
			{
				return param;
			}
			param /= num2;
			switch (text14)
			{
			case "hp":
				if (param > (float)MainGame.me.save.max_hp)
				{
					param = MainGame.me.save.max_hp;
				}
				break;
			case "energy":
				if (param < 0f)
				{
					param = 0f;
				}
				else if (param > (float)MainGame.me.save.max_energy)
				{
					param = MainGame.me.save.max_energy;
				}
				break;
			}
			player.SetParam(text14, param);
			return param;
		});
		_exp.RegisterFunction("HasOverheadBody", delegate
		{
			Item overheadItem13 = player.components.character.GetOverheadItem();
			return (overheadItem13 == null || overheadItem13.IsEmpty()) ? ((object)false) : ((object)(overheadItem13.definition.type == ItemDefinition.ItemType.Body));
		});
		_exp.RegisterFunction("HasBodyInWGO", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return _wgo.GetBodyFromInventory() != null;
		});
		_exp.RegisterFunction("CanTakeWoodFromWGO", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return player.components.character.has_overhead ? ((object)false) : ((object)(_wgo.data.GetItemWithID("wood") != null));
		});
		_exp.RegisterFunction("CanTakeStoneFromWGO", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return player.components.character.has_overhead ? ((object)false) : ((object)(_wgo.data.GetItemWithID("stone") != null || _wgo.data.GetItemWithID("marble") != null));
		});
		_exp.RegisterFunction("CanTakeOreMetalFromWGO", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return player.components.character.has_overhead ? ((object)false) : ((object)(_wgo.data.GetItemWithID("ore_metal") != null));
		});
		_exp.RegisterFunction("HasItemInWGO", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			if (pars.Length == 0)
			{
				Debug.LogError("Function \"HasItemInWGO\" need parameters!");
				return false;
			}
			for (int j = 0; j < pars.Length; j++)
			{
				string text13 = pars[j].EvaluateAsString(values);
				if (string.IsNullOrEmpty(text13))
				{
					Debug.LogError("Function \"HasItemInWGO\" parameter is null!");
					return false;
				}
				if (_wgo.data.GetItemWithID(text13) != null)
				{
					return true;
				}
			}
			return false;
		});
		_exp.RegisterFunction("TakeItemFromWGO", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			if (_wgo.data.inventory.Count == 0)
			{
				Debug.LogError("Can not take smth from WGO: WGO inventory is empty!");
				return false;
			}
			int count = _wgo.data.inventory.Count;
			Item item2 = _wgo.data.inventory[count - 1];
			_wgo.data.RemoveItem(item2, 1);
			if (item2.definition.item_size == 2)
			{
				if (player.components.character.has_overhead)
				{
					player.components.character.DropOverheadItem(to_right: true);
				}
				player.components.character.SetOverheadItem(item2);
			}
			else
			{
				player.AddToInventory(item2.id, 1);
			}
			_wgo.Redraw();
			MainGame.me.player.components.interaction.UpdateNearestHint();
			return true;
		});
		_exp.RegisterFunction("CanPutOverheadToWGO", delegate
		{
			Item overheadItem12 = player.components.character.GetOverheadItem();
			if (overheadItem12 == null || overheadItem12.IsEmpty())
			{
				return false;
			}
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return _wgo.CanInsertItem(overheadItem12);
		});
		_exp.RegisterFunction("CanPutOverheadToCrematorium", delegate
		{
			Item overheadItem11 = player.components.character.GetOverheadItem();
			if (overheadItem11 == null || overheadItem11.IsEmpty())
			{
				return false;
			}
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return _wgo.GetParamInt("is_body_inserted") == 0 && _wgo.CanInsertItem(overheadItem11);
		});
		_exp.RegisterFunction("PutPromoZombie", delegate
		{
			Item overheadItem10 = player.components.character.GetOverheadItem();
			if (overheadItem10 == null || overheadItem10.IsEmpty())
			{
				return false;
			}
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			if (!_wgo.CanInsertItem(overheadItem10))
			{
				return false;
			}
			if (overheadItem10.id != "working_zombie_pseudoitem_1")
			{
				return false;
			}
			_wgo.AddToInventory(overheadItem10);
			player.components.character.SetOverheadItem(null);
			GDPoint componentInChildren = _wgo.GetComponentInChildren<GDPoint>(includeInactive: true);
			if (componentInChildren != null)
			{
				WorldMap.SpawnWGO(MainGame.me.world_root, "zombie_promo", componentInChildren.transform.position, "zombie_promo");
			}
			return true;
		});
		_exp.RegisterFunction("CanTakePromoZombie", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			return (_wgo.data.GetTotalCount("working_zombie_pseudoitem_1") < 1) ? ((object)false) : ((object)true);
		});
		_exp.RegisterFunction("TakePromoZombie", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			if (_wgo.data.GetTotalCount("working_zombie_pseudoitem_1") < 1)
			{
				return false;
			}
			if (MainGame.me.player_char.has_overhead)
			{
				Item overheadItem9 = MainGame.me.player_char.GetOverheadItem();
				MainGame.me.player.DropItem(overheadItem9);
				MainGame.me.player_char.SetOverheadItem(null);
			}
			Item item = null;
			foreach (Item item3 in _wgo.data.inventory)
			{
				if (item3 != null && item3.value >= 1 && !(item3.id != "working_zombie_pseudoitem_1"))
				{
					item = item3;
					break;
				}
			}
			if (item == null)
			{
				return false;
			}
			_wgo.data.inventory.Remove(item);
			MainGame.me.player_char.SetOverheadItem(item);
			WorldGameObject worldGameObjectByCustomTag3 = WorldMap.GetWorldGameObjectByCustomTag("zombie_promo");
			if (worldGameObjectByCustomTag3 != null)
			{
				worldGameObjectByCustomTag3.DestroyMe();
			}
			return true;
		});
		_exp.RegisterFunction("CanLinkOverheadZombieWorker", delegate
		{
			Item overheadItem8 = player.components.character.GetOverheadItem();
			if (overheadItem8 == null || overheadItem8.IsEmpty())
			{
				return false;
			}
			if (!overheadItem8.id.StartsWith("working_zombie_pseudoitem"))
			{
				return false;
			}
			if (_wgo == null)
			{
				return false;
			}
			if (_wgo.has_linked_worker)
			{
				return false;
			}
			DockPoint availableDockPointForZombie = _wgo.GetAvailableDockPointForZombie();
			return (availableDockPointForZombie == null || availableDockPointForZombie.tf == null) ? ((object)false) : ((object)true);
		});
		_exp.RegisterFunction("LinkOverheadZombieWorker", delegate
		{
			Item overheadItem7 = player.components.character.GetOverheadItem();
			if (overheadItem7 == null || overheadItem7.IsEmpty())
			{
				return false;
			}
			if (!overheadItem7.id.StartsWith("working_zombie_pseudoitem"))
			{
				return false;
			}
			WorldGameObject o2;
			bool is_success2;
			string text12 = WorldMap.SpawnZombieWorkerFromStock(_wgo, overheadItem7, out o2, out is_success2);
			if (!string.IsNullOrEmpty(text12))
			{
				Debug.LogError(text12);
			}
			if (!is_success2)
			{
				return false;
			}
			player.components.character.SetOverheadItem(null);
			if (_wgo != null)
			{
				switch (_wgo.obj_id)
				{
				case "zombie_sawmill_completed":
				{
					string craft_name = "zombie_sawmill_wood_production";
					if (!_wgo.components.craft.is_crafting)
					{
						Debug.LogError("FATAL ERROR: zombie_sawmill_completed has no craft!");
						_wgo.TryStartCraft(craft_name);
					}
					break;
				}
				case "zombie_mine_fence_left_front":
				{
					string craft_name = "zombie_mine_stone_production";
					if (!_wgo.components.craft.is_crafting)
					{
						Debug.LogError("FATAL ERROR: zombie_mine_fence_left_front has no craft!");
						_wgo.TryStartCraft(craft_name);
					}
					break;
				}
				case "zombie_mine_fence_front":
				{
					string craft_name = "zombie_mine_marble_production";
					try
					{
						if (((Vector2)_wgo.transform.position - new Vector2(-3932f, 6622f)).sqrMagnitude < 10f)
						{
							craft_name = "zombie_mine_stone_production";
						}
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
					if (!_wgo.components.craft.is_crafting)
					{
						Debug.LogError("FATAL ERROR: zombie_mine_fence_front has no craft!");
						_wgo.TryStartCraft(craft_name);
					}
					break;
				}
				}
			}
			return true;
		});
		_exp.RegisterFunction("HasLinkedZombieWorker", delegate
		{
			if (_wgo == null)
			{
				return false;
			}
			if (!_wgo.has_linked_worker)
			{
				return false;
			}
			return (!_wgo.components.craft.is_crafting) ? ((object)false) : ((object)true);
		});
		_exp.RegisterFunction("TakeLinkedZombieWorker", delegate
		{
			if (_wgo == null)
			{
				return false;
			}
			if (!_wgo.has_linked_worker)
			{
				return false;
			}
			if (!_wgo.components.craft.is_crafting)
			{
				return false;
			}
			if (MainGame.me.player_char.has_overhead)
			{
				Item overheadItem6 = MainGame.me.player_char.GetOverheadItem();
				MainGame.me.player.DropItem(overheadItem6);
				MainGame.me.player_char.SetOverheadItem(null);
			}
			if (string.IsNullOrEmpty(_wgo.linked_worker?.worker?.definition?.item_overhead))
			{
				Debug.LogError("FATAL ERROR: can not take worker from workbench: worker_item_id is NULL!");
				return false;
			}
			Worker worker2 = _wgo.linked_worker.worker;
			string text11 = WorldMap.RemoveZombieWorkerToStock(_wgo.linked_worker);
			if (!string.IsNullOrEmpty(text11))
			{
				Debug.LogError(text11);
			}
			MainGame.me.player_char.SetOverheadItem(worker2.GetOverheadItem());
			return true;
		});
		_exp.RegisterFunction("LinkOverheadWorkerToMine", delegate
		{
			Item overheadItem5 = player.components.character.GetOverheadItem();
			if (overheadItem5 == null || overheadItem5.IsEmpty())
			{
				return false;
			}
			if (!overheadItem5.id.StartsWith("working_zombie_pseudoitem"))
			{
				return false;
			}
			WorldGameObject o;
			bool is_success;
			string text10 = WorldMap.SpawnZombieWorkerFromStock(_wgo, overheadItem5, out o, out is_success);
			if (!string.IsNullOrEmpty(text10))
			{
				Debug.LogError(text10);
			}
			if (!is_success)
			{
				return false;
			}
			player.components.character.SetOverheadItem(null);
			WorldGameObject worldGameObjectByCustomTag2 = WorldMap.GetWorldGameObjectByCustomTag("mine_zombie");
			if (worldGameObjectByCustomTag2 == null)
			{
				Debug.LogError("FATAL ERROR: mine_zombie not found on map!");
			}
			else
			{
				worldGameObjectByCustomTag2.data.AddToParams("zombies_inside", 1f);
			}
			if (_wgo != null && _wgo.obj_id == "mine_zombie_bench" && !_wgo.components.craft.is_crafting)
			{
				Debug.LogError("FATAL ERROR: mine_zombie_bench has no craft!");
				_wgo.TryStartCraft("mine_zombie_bench_iron_production");
			}
			return true;
		});
		_exp.RegisterFunction("TakeLinkedWorkerFromMine", delegate
		{
			if (_wgo == null)
			{
				return false;
			}
			if (!_wgo.has_linked_worker)
			{
				return false;
			}
			if (!_wgo.components.craft.is_crafting)
			{
				return false;
			}
			if (MainGame.me.player_char.has_overhead)
			{
				Item overheadItem4 = MainGame.me.player_char.GetOverheadItem();
				MainGame.me.player.DropItem(overheadItem4);
				MainGame.me.player_char.SetOverheadItem(null);
			}
			if (string.IsNullOrEmpty(_wgo.linked_worker?.worker?.definition?.item_overhead))
			{
				Debug.LogError("FATAL ERROR: can not take worker from workbench: worker_item_id is NULL!");
				return false;
			}
			Worker worker = _wgo.linked_worker.worker;
			string text9 = WorldMap.RemoveZombieWorkerToStock(_wgo.linked_worker);
			if (!string.IsNullOrEmpty(text9))
			{
				Debug.LogError(text9);
			}
			MainGame.me.player_char.SetOverheadItem(worker.GetOverheadItem());
			WorldGameObject worldGameObjectByCustomTag = WorldMap.GetWorldGameObjectByCustomTag("mine_zombie");
			if (worldGameObjectByCustomTag == null)
			{
				Debug.LogError("FATAL ERROR: mine_zombie not found on map!");
			}
			else
			{
				worldGameObjectByCustomTag.data.AddToParams("zombies_inside", -1f);
			}
			return true;
		});
		_exp.RegisterFunction("PutOverheadToWGO", delegate
		{
			Item overheadItem3 = player.components.character.GetOverheadItem();
			if (overheadItem3 == null || overheadItem3.IsEmpty())
			{
				return false;
			}
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			if (!_wgo.CanInsertItem(overheadItem3))
			{
				Debug.LogError("Coudln't insert overhead item into WGO", _wgo);
				return false;
			}
			if (_wgo.AddToInventory(overheadItem3))
			{
				player.components.character.SetOverheadItem(null);
			}
			_wgo.Redraw();
			MainGame.me.player.components.interaction.UpdateNearestHint();
			return true;
		});
		_exp.RegisterFunction("PutBarmanToWGO", delegate
		{
			Item overheadItem2 = player.components.character.GetOverheadItem();
			if (overheadItem2 == null || overheadItem2.IsEmpty())
			{
				return false;
			}
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			if (!_wgo.CanInsertItem(overheadItem2))
			{
				Debug.LogError("Coudln't insert overhead item into WGO", _wgo);
				return false;
			}
			if (_wgo.AddToInventory(overheadItem2))
			{
				player.components.character.SetOverheadItem(null);
			}
			_wgo.Redraw();
			MainGame.me.player.components.interaction.UpdateNearestHint();
			GS.RunFlowScript("on_barman_placed");
			return true;
		});
		_exp.RegisterFunction("OpenCraft", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			GUIElements.me.OpenCraftGUI(_wgo);
			return true;
		});
		_exp.RegisterFunction("GetZoneQ", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			WorldZone zoneByID = WorldZone.GetZoneByID(pars[0].EvaluateAsString(values));
			return (zoneByID == null) ? 0f : zoneByID.GetTotalQuality();
		});
		_exp.RegisterFunction("AddBuff", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			BuffsLogics.AddBuff(pars[0].EvaluateAsString(values));
			return true;
		});
		_exp.RegisterFunction("AddBuffLen", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			BuffsLogics.AddBuff(pars[0].EvaluateAsString(values), pars[1].EvaluateAsFloat(values));
			return true;
		});
		_exp.RegisterFunction("RemoveBuff", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			BuffsLogics.RemoveBuff(pars[0].EvaluateAsString(values));
			return true;
		});
		_exp.RegisterFunction("BlackList", (IExpression[] pars, IDictionary<string, object> values) => save.game_logics.AddToBlackList(pars[0].EvaluateAsString(values)));
		_exp.RegisterFunction("ForceExecute", (IExpression[] pars, IDictionary<string, object> values) => MainGame.me.save.game_logics.ForceExecute(pars[0].EvaluateAsString(values)));
		_exp.RegisterFunction("ForceExecuteCond", (IExpression[] pars, IDictionary<string, object> values) => MainGame.me.save.game_logics.ForceExecuteCond(pars[0].EvaluateAsString(values)));
		_exp.RegisterFunction("UnlockTech", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			GUIElements.me.tech_dialog.Open(GameBalance.me.GetData<TechDefinition>(pars[0].EvaluateAsString(values)), null, forced_unlock: true);
			return true;
		});
		_exp.RegisterFunction("UnlockCraft", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			MainGame.me.save.UnlockCraft(pars[0].EvaluateAsString(values));
			return true;
		});
		_exp.RegisterFunction("IsNotCraftingNow", delegate
		{
			if (!_wgo.components.craft.enabled)
			{
				return true;
			}
			return _wgo.components.craft.is_crafting ? ((object)false) : ((object)true);
		});
		_exp.RegisterFunction("OpenPorterStationGUI", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			GUIElements.me.OpenPorterStationGUI(_wgo);
			return true;
		});
		_exp.RegisterFunction("OpenResurrectionGUI", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			GUIElements.me.OpenResurrectionGUI(_wgo);
			return true;
		});
		_exp.RegisterFunction("IsDLCAvailable", (IExpression[] pars, IDictionary<string, object> values) => DLCEngine.IsDLCAvailable((DLCEngine.DLCVersion)pars[0].EvaluateAsInt(values)));
		_exp.RegisterFunction("IsCraftUnlocked", (IExpression[] pars, IDictionary<string, object> values) => MainGame.me.save.unlocked_crafts.Contains(pars[0].EvaluateAsString(values)));
		_exp.RegisterFunction("CanTakeWaterFromPump", delegate
		{
			if (_wgo?.data == null)
			{
				return false;
			}
			return (_wgo.data.GetItemsCount("water") == 0) ? ((object)false) : ((object)true);
		});
		_exp.RegisterFunction("TakeWaterFromPump", delegate
		{
			int num = _wgo.data.GetItemsCount("water");
			if (num > 10)
			{
				num = 10;
			}
			_wgo.DropItem(new Item("water", num), Direction.ToPlayer);
			WorldMap.GetWorldGameObjectByObjId("water_well").TriggerSmartAnimation("work");
			_wgo.data.RemoveItem("water", num);
			return true;
		});
		_exp.RegisterFunction("TakeWaterFromRefugeeWell", delegate
		{
			if (!MainGame.me.player.components.character.player.TrySpendEnergy(3f))
			{
				return false;
			}
			_wgo.DropItem(new Item("water", 10), Direction.ToPlayer);
			_wgo.TriggerSmartAnimation("work");
			return true;
		});
		_exp.RegisterFunction("StartEvent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			string event_id = pars[0].EvaluateAsString(values);
			_wgo.FireEvent(event_id);
			return true;
		});
		_exp.RegisterFunction("PlayerHasOverheadItem", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item overheadItem = player.components.character.GetOverheadItem();
			if (overheadItem == null || overheadItem.IsEmpty())
			{
				return false;
			}
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			string text8 = pars[0].EvaluateAsString(values);
			return (overheadItem.id == text8 && !string.IsNullOrEmpty(text8)) ? ((object)true) : ((object)false);
		});
		_exp.RegisterFunction("UnlockRandomAlchemy", delegate
		{
			List<CraftDefinition> list = new List<CraftDefinition>();
			List<CraftDefinition> list2 = new List<CraftDefinition>();
			foreach (CraftDefinition craft_datum in GameBalance.me.craft_data)
			{
				if (craft_datum.id.StartsWith("mix:") && !craft_datum.id.Contains("goo") && !craft_datum.id.Contains(":_:"))
				{
					list2.Add(craft_datum);
					if (!MainGame.me.save.completed_one_time_crafts.Contains(craft_datum.id))
					{
						list.Add(craft_datum);
					}
				}
			}
			if (list.Count > 0)
			{
				CraftDefinition craftDefinition3 = list[UnityEngine.Random.Range(0, list.Count)];
				MainGame.me.save.completed_one_time_crafts.Add(craftDefinition3.id);
				MainGame.me.save.achievements.CheckKeyQuests("new_" + craftDefinition3.ach_key);
				string text7 = string.Empty;
				if (craftDefinition3.output == null || craftDefinition3.output.Count == 0)
				{
					text7 = craftDefinition3.id;
				}
				else
				{
					foreach (Item item4 in craftDefinition3.output)
					{
						if (!(item4.id == "r") && !(item4.id == "g") && !(item4.id == "b"))
						{
							text7 = item4.id;
							break;
						}
					}
					if (string.IsNullOrEmpty(text7))
					{
						text7 = craftDefinition3.id;
					}
				}
				TechDefinition tech2 = new TechDefinition
				{
					id = text7,
					crafts = { craftDefinition3.id },
					price = new GameRes()
				};
				GUIElements.me.tech_dialog.Open(tech2, delegate
				{
				}, forced_unlock: true, reveal_tech: false, show_tech_tree_after: false, pseudotech: true);
			}
			else
			{
				if (list2.Count == 0)
				{
					Debug.LogError("UnlockRandomAlchemy error: no mix crafts in Balance!");
					return false;
				}
				List<Item> items = ResModificator.ProcessItemsListBeforeDrop(list2[UnityEngine.Random.Range(0, list2.Count)].output, null, MainGame.me.player);
				MainGame.me.player.DropItems(items);
			}
			return false;
		});
		_exp.RegisterFunction("UnlockAlchemy", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text5 = pars[0].EvaluateAsString(values);
			CraftDefinition craftDefinition = null;
			for (int i = 0; i < GameBalance.me.craft_data.Count; i++)
			{
				CraftDefinition craftDefinition2 = GameBalance.me.craft_data[i];
				if (craftDefinition2.id == text5 && !MainGame.me.save.completed_one_time_crafts.Contains(text5))
				{
					MainGame.me.save.completed_one_time_crafts.Add(craftDefinition2.id);
					craftDefinition = craftDefinition2;
				}
			}
			if (craftDefinition != null)
			{
				string text6 = string.Empty;
				if (craftDefinition.output == null || craftDefinition.output.Count == 0)
				{
					text6 = craftDefinition.id;
				}
				else
				{
					foreach (Item item5 in craftDefinition.output)
					{
						if (!(item5.id == "r") && !(item5.id == "g") && !(item5.id == "b"))
						{
							text6 = item5.id;
							break;
						}
					}
					if (string.IsNullOrEmpty(text6))
					{
						text6 = craftDefinition.id;
					}
				}
				TechDefinition tech = new TechDefinition
				{
					id = text6,
					crafts = { craftDefinition.id },
					price = new GameRes()
				};
				GUIElements.me.tech_dialog.Open(tech, delegate
				{
				}, forced_unlock: true, reveal_tech: false, show_tech_tree_after: false, pseudotech: true);
			}
			return false;
		});
		_exp.RegisterFunction("OpenSoulContainerWindow", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			GUIElements.me.soul_container_gui.Open(_wgo);
			return true;
		});
		_exp.RegisterFunction("OpenSoulHealingWindow", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			GUIElements.me.soul_healer_gui.Open(_wgo);
			return true;
		});
		_exp.RegisterFunction("FillCraftsList", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			_wgo.components.craft.FillCraftsList();
			GUIElements.me.OpenCraftGUI(_wgo);
			return true;
		});
		_exp.RegisterFunction("OpenSoulWorkbenchWindow", delegate
		{
			if (_wgo == null)
			{
				Debug.LogError("WGO is null while evaluating expression: " + _exp);
				return false;
			}
			GUIElements.me.organ_enhancer_gui.Open(_wgo);
			return true;
		});
		_exp.RegisterFunction("GetItemPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_items.IsNullOrEmpty())
			{
				Debug.LogError($"Item is null while evaluation expression: {_exp}");
				return 0f;
			}
			string text4 = pars[0].EvaluateAsString(values);
			string param_name2 = pars[1].EvaluateAsString(values);
			foreach (Item item6 in _items)
			{
				if (!(item6.id != text4))
				{
					return item6.GetParam(param_name2);
				}
			}
			Debug.LogError($"Item with id {text4} not found in expression: {_exp}");
			return 0f;
		});
		_exp.RegisterFunction("SetItemPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_items.IsNullOrEmpty())
			{
				Debug.LogError($"Item is null or empty while evaluation expression: {_exp}");
				return 0f;
			}
			string text3 = pars[0].EvaluateAsString(values);
			string param_name = pars[1].EvaluateAsString(values);
			float value = pars[2].EvaluateAsFloat(values);
			foreach (Item item7 in _items)
			{
				if (!(item7.id != text3))
				{
					item7.SetParam(param_name, value);
				}
			}
			return true;
		});
		_exp.RegisterFunction("GetItemRS", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_items.IsNullOrEmpty())
			{
				Debug.LogError($"Item is null or empty while evaluation expression: {_exp}");
				return 0f;
			}
			string text2 = pars[0].EvaluateAsString(values);
			foreach (Item item8 in _items)
			{
				if (!(item8.id != text2))
				{
					return item8.GetRedSkullsValue();
				}
			}
			Debug.LogError($"Item with id {text2} not found in expression: {_exp}");
			return 0f;
		});
		_exp.RegisterFunction("GetItemWS", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (_items.IsNullOrEmpty())
			{
				Debug.LogError($"Item is null or empty while evaluation expression: {_exp}");
				return 0f;
			}
			string text = pars[0].EvaluateAsString(values);
			foreach (Item item9 in _items)
			{
				if (!(item9.id != text))
				{
					return item9.GetWhiteSkullsValue();
				}
			}
			Debug.LogError($"Item with id {text} not found in expression: {_exp}");
			return 0f;
		});
	}

	public bool EvaluateChance(WorldGameObject wgo = null, WorldGameObject character = null)
	{
		float num = EvaluateFloat(wgo, character);
		if (num < 0.01f)
		{
			return false;
		}
		return num > UnityEngine.Random.Range(0f, 1f);
	}

	public bool EvaluateBoolean(WorldGameObject wgo = null, WorldGameObject character = null)
	{
		if (string.IsNullOrEmpty(_expression))
		{
			return true;
		}
		if (_simplified)
		{
			return _simpified_float > 0f;
		}
		_wgo = wgo;
		_character = character;
		CheckExpressionInit();
		try
		{
			return Convert.ToBoolean(_exp.Evaluate());
		}
		catch (ExpressiveException ex)
		{
			if (ex.ToString().Contains("NullReference"))
			{
				throw;
			}
			Debug.LogError("Error in expression: " + ex?.ToString() + "\n" + _expression);
			return true;
		}
	}

	public float EvaluateFloat(WorldGameObject wgo = null, WorldGameObject character = null)
	{
		if (string.IsNullOrEmpty(_expression))
		{
			return default_value;
		}
		if (_simplified)
		{
			return _simpified_float;
		}
		_wgo = wgo;
		_character = character;
		CheckExpressionInit();
		try
		{
			return (float)Convert.ToDecimal(_exp.Evaluate());
		}
		catch (ExpressiveException ex)
		{
			if (ex.ToString().Contains("NullReference"))
			{
				throw;
			}
			Debug.LogError("Error in expression: " + ex?.ToString() + "\n" + _expression);
			return 1f;
		}
	}

	public void Evaluate(WorldGameObject wgo = null, WorldGameObject character = null)
	{
		if (string.IsNullOrEmpty(_expression) || _simplified)
		{
			return;
		}
		_wgo = wgo;
		_character = character;
		CheckExpressionInit();
		try
		{
			_exp.Evaluate();
		}
		catch (ExpressiveException ex)
		{
			if (ex.ToString().Contains("NullReference"))
			{
				throw;
			}
			Debug.LogError("Error in expression: " + ex?.ToString() + "\n" + _expression);
		}
	}

	public void Evaluate(List<Item> items)
	{
		if (string.IsNullOrEmpty(_expression) || _simplified)
		{
			return;
		}
		_items = items;
		CheckExpressionInit();
		try
		{
			_exp.Evaluate();
		}
		catch (ExpressiveException ex)
		{
			if (ex.ToString().Contains("NullReference"))
			{
				throw;
			}
			Debug.LogError("Error in expression: " + ex?.ToString() + "\n" + _expression);
		}
	}

	public static SmartExpression ParseExpression(string expression)
	{
		if (string.IsNullOrEmpty(expression))
		{
			return null;
		}
		expression = ParseRegex(expression);
		SmartExpression smartExpression = new SmartExpression();
		smartExpression.FromString(expression);
		return smartExpression;
	}

	public static string ParseRegex(string input)
	{
		foreach (string key in equatings.Keys)
		{
			input = Regex.Replace(input, key, equatings[key] + "(\"$1\", $2)");
		}
		input = Regex.Replace(input, "\\$(\\w*)", "Ppar(\"$1\")");
		input = Regex.Replace(input, "@(\\w*)", "WGOpar(\"$1\")");
		return input;
	}

	public string GetRawExpressionString()
	{
		return _expression;
	}
}
