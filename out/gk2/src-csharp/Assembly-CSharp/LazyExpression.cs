using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Expressive;
using Expressive.Exceptions;
using Expressive.Expressions;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class LazyExpression : LazyExpressionBase
{
	private class CallbackAvailableMarker
	{
	}

	private static Dictionary<string, string> equatings = new Dictionary<string, string>
	{
		{ "\\$(\\w*) *\\+= *(.*)", "AddPPar" },
		{ "\\$(\\w*) *\\-= *(.*)", "DecPPar" },
		{ "\\$(\\w*) *\\*= *(.*)", "MultiplyPPar" },
		{ "\\$(\\w*) *\\/= *(.*)", "DividePPar" },
		{ "\\$(\\w*) *\\= *([^=].*)", "SetPPar" },
		{ "\\@(\\w*) *\\+= *(.*)", "AddWGOPar" },
		{ "\\@(\\w*) *\\-= *(.*)", "DecWGOPar" },
		{ "\\@(\\w*) *\\*= *(.*)", "MultiplyWGOPar" },
		{ "\\@(\\w*) *\\/= *(.*)", "DivideWGOPar" },
		{ "\\@(\\w*) *\\= *([^=].*)", "SetWGOPar" },
		{ "\\%(\\w*) *\\+= *(.*)", "AddWorldPar" },
		{ "\\%(\\w*) *\\-= *(.*)", "DecWorldPar" },
		{ "\\%(\\w*) *\\*= *(.*)", "MultiplyWorldPar" },
		{ "\\%(\\w*) *\\/= *(.*)", "DivideWorldPar" },
		{ "\\%(\\w*) *\\= *([^=].*)", "SetWorldPar" },
		{ "\\#(\\w*) *\\+= *(.*)", "AddWorkerPar" },
		{ "\\#(\\w*) *\\-= *(.*)", "DecWorkerPar" },
		{ "\\#(\\w*) *\\*= *(.*)", "MultiplyWorkerPar" },
		{ "\\#(\\w*) *\\/= *(.*)", "DivideWorkerPar" },
		{ "\\#(\\w*) *\\= *([^=].*)", "SetWorkerPar" }
	};

	private readonly float defaultFloatValue;

	[NonSerialized]
	private LazyExpressionContext context;

	private static Context s_sharedContext;

	private static Context SharedContext
	{
		get
		{
			if (s_sharedContext == null)
			{
				s_sharedContext = new Context(ExpressiveOptions.None);
				RegisterCustomFunctions(s_sharedContext);
			}
			return s_sharedContext;
		}
	}

	public LazyExpression()
	{
	}

	public LazyExpression(string expression)
	{
		FromString(expression);
	}

	protected override void CheckExpressionInit()
	{
		if (expression == null && !base.HasPureValue)
		{
			expression = new Expression(expressionString, SharedContext);
		}
	}

	protected override string ParseRegex(string input)
	{
		foreach (string key in equatings.Keys)
		{
			input = Regex.Replace(input, key, equatings[key] + "(\"$1\", $2)");
		}
		input = Regex.Replace(input, "\\$(\\w*)", "PPar(\"$1\")");
		input = Regex.Replace(input, "@(\\w*)", "WGOPar(\"$1\")");
		input = Regex.Replace(input, "\\%(\\w*)", "WorldPar(\"$1\")");
		input = Regex.Replace(input, "#(\\w*)", "WorkerPar(\"$1\")");
		return input;
	}

	public float EvaluateFloat(LazyExpressionContext ctx)
	{
		context = ctx;
		return EvaluateFloatInternal();
	}

	public int EvaluateInt(LazyExpressionContext ctx)
	{
		return (int)EvaluateFloat(ctx);
	}

	public bool EvaluateBool(LazyExpressionContext ctx)
	{
		context = ctx;
		return EvaluateBoolInternal();
	}

	public string Evaluate(LazyExpressionContext ctx)
	{
		context = ctx;
		return EvaluateStringInternal();
	}

	public Item EvaluateItem(LazyExpressionContext ctx)
	{
		context = ctx;
		return EvaluateItemInternal();
	}

	public float EvaluateFloat()
	{
		context = LazyExpressionContext.Empty;
		return EvaluateFloatInternal();
	}

	public int EvaluateInt()
	{
		return (int)EvaluateFloat();
	}

	public bool EvaluateBool()
	{
		context = LazyExpressionContext.Empty;
		return EvaluateBoolInternal();
	}

	public string Evaluate()
	{
		context = LazyExpressionContext.Empty;
		return EvaluateStringInternal();
	}

	public int EvaluateInt(WgoData wgoData)
	{
		return EvaluateInt(LazyExpressionContext.From(wgoData));
	}

	public int EvaluateInt(ICombatEntity combatEntity)
	{
		return EvaluateInt(LazyExpressionContext.From(combatEntity));
	}

	public int EvaluateInt(Item item)
	{
		return EvaluateInt(LazyExpressionContext.From(item));
	}

	public int EvaluateInt(ICraftable craftable)
	{
		return EvaluateInt(LazyExpressionContext.From(craftable));
	}

	public float EvaluateFloat(WgoData wgoData)
	{
		return EvaluateFloat(LazyExpressionContext.From(wgoData));
	}

	public float EvaluateFloat(ICraftable craftable)
	{
		return EvaluateFloat(LazyExpressionContext.From(craftable));
	}

	public float EvaluateFloat(ICombatEntity combatEntity)
	{
		return EvaluateFloat(LazyExpressionContext.From(combatEntity));
	}

	public float EvaluateFloat(Item item)
	{
		return EvaluateFloat(LazyExpressionContext.From(item));
	}

	public float EvaluateFloat(WorldZoneData worldZoneData)
	{
		return EvaluateFloat(LazyExpressionContext.From(worldZoneData));
	}

	public bool EvaluateBool(WgoData wgoData)
	{
		return EvaluateBool(LazyExpressionContext.From(wgoData));
	}

	public string Evaluate(WgoData wgoData)
	{
		return Evaluate(LazyExpressionContext.From(wgoData));
	}

	public string Evaluate(Item item)
	{
		return Evaluate(LazyExpressionContext.From(item));
	}

	public string EvaluateWithAttackerAttacksMe(ICombatEntity combatEntity)
	{
		return Evaluate(LazyExpressionContext.WithAttackerAttacksMe(combatEntity));
	}

	public string EvaluateValueDelta(int valueDelta)
	{
		return Evaluate(LazyExpressionContext.ValueDelta(valueDelta));
	}

	public Item EvaluateItem(WgoData wgoData)
	{
		return EvaluateItem(LazyExpressionContext.From(wgoData));
	}

	private object EvaluateInScope(Action customCallback = null)
	{
		LazyExpressionEvaluationScope.Begin(context, customCallback);
		try
		{
			return expression.Evaluate();
		}
		finally
		{
			LazyExpressionEvaluationScope.End();
		}
	}

	private float EvaluateFloatInternal()
	{
		if (!base.HasExpression)
		{
			return defaultFloatValue;
		}
		if (base.HasPureValue && pureValueType == PureValueType.Float)
		{
			return pureValueFloat;
		}
		CheckExpressionInit();
		try
		{
			return (float)Convert.ToDecimal(EvaluateInScope());
		}
		catch (ExpressiveException e)
		{
			HandleEvaluateError(e);
		}
		return defaultFloatValue;
	}

	private bool EvaluateBoolInternal()
	{
		if (!base.HasExpression)
		{
			return false;
		}
		if (base.HasPureValue && pureValueType == PureValueType.Bool)
		{
			return pureValueBool;
		}
		if (base.HasPureValue && pureValueType == PureValueType.Float)
		{
			return pureValueFloat > 0f;
		}
		CheckExpressionInit();
		try
		{
			return Convert.ToBoolean(EvaluateInScope());
		}
		catch (ExpressiveException e)
		{
			HandleEvaluateError(e);
		}
		return false;
	}

	private string EvaluateStringInternal()
	{
		if (!base.HasExpression)
		{
			return string.Empty;
		}
		if (base.HasPureValue && pureValueType == PureValueType.String)
		{
			return expressionString;
		}
		CheckExpressionInit();
		try
		{
			return Convert.ToString(EvaluateInScope());
		}
		catch (ExpressiveException e)
		{
			HandleEvaluateError(e);
		}
		return string.Empty;
	}

	private Item EvaluateItemInternal()
	{
		if (!base.HasExpression)
		{
			return null;
		}
		CheckExpressionInit();
		try
		{
			return EvaluateInScope() as Item;
		}
		catch (ExpressiveException e)
		{
			HandleEvaluateError(e);
		}
		return null;
	}

	public bool EvaluateChance()
	{
		float num = EvaluateFloat();
		if (num < 0.01f)
		{
			return false;
		}
		return num > UnityEngine.Random.Range(0f, 1f);
	}

	public bool EvaluateWithCallback(Action callback)
	{
		if (!base.HasExpression)
		{
			return false;
		}
		CheckExpressionInit();
		try
		{
			return EvaluateInScope(callback) is CallbackAvailableMarker;
		}
		catch (ExpressiveException e)
		{
			HandleEvaluateError(e);
		}
		return false;
	}

	protected override bool HasExpressionSymptom(string strToCheck)
	{
		if (string.IsNullOrEmpty(strToCheck))
		{
			return false;
		}
		if (!base.HasExpressionSymptom(strToCheck) && !strToCheck.Contains('$'))
		{
			return strToCheck.Contains('@');
		}
		return true;
	}

	private static void RegisterCustomFunctions(Context ctx)
	{
		ctx.RegisterFunction("If2", (IExpression[] pars, IDictionary<string, object> values) => (!pars[0].EvaluateAsBoolean(values)) ? ((object)false) : pars[1].Evaluate(values));
		ctx.RegisterFunction("True", (IExpression[] pars, IDictionary<string, object> values) => true);
		ctx.RegisterFunction("False", (IExpression[] pars, IDictionary<string, object> values) => false);
		ctx.RegisterFunction("PPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string type12 = pars[0].EvaluateAsString(values);
			return MainGame.PlayerData.GetRes(type12);
		});
		ctx.RegisterFunction("AddPPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string type11 = pars[0].EvaluateAsString(values);
			float value24 = pars[1].EvaluateAsFloat(values);
			MainGame.PlayerData.AddRes(type11, value24);
			return MainGame.PlayerData.GetRes(type11);
		});
		ctx.RegisterFunction("DecPPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string type10 = pars[0].EvaluateAsString(values);
			float num25 = pars[1].EvaluateAsFloat(values);
			MainGame.PlayerData.AddRes(type10, 0f - num25);
			return MainGame.PlayerData.GetRes(type10);
		});
		ctx.RegisterFunction("SetPPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string type9 = pars[0].EvaluateAsString(values);
			float value23 = pars[1].EvaluateAsFloat(values);
			MainGame.PlayerData.SetRes(type9, value23);
			return MainGame.PlayerData.GetRes(type9);
		});
		ctx.RegisterFunction("WorldPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id50 = pars[0].EvaluateAsString(values);
			return MainGame.Instance.GameSave.WorldData.GetGameRes(id50);
		});
		ctx.RegisterFunction("AddWorldPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id49 = pars[0].EvaluateAsString(values);
			float value22 = pars[1].EvaluateAsFloat(values);
			MainGame.Instance.GameSave.WorldData.AddGameRes(id49, value22);
			return MainGame.Instance.GameSave.WorldData.GetGameRes(id49);
		});
		ctx.RegisterFunction("DecWorldPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id48 = pars[0].EvaluateAsString(values);
			float num24 = pars[1].EvaluateAsFloat(values);
			MainGame.Instance.GameSave.WorldData.AddGameRes(id48, 0f - num24);
			return MainGame.Instance.GameSave.WorldData.GetGameRes(id48);
		});
		ctx.RegisterFunction("MultiplyWorldPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id47 = pars[0].EvaluateAsString(values);
			float value21 = pars[1].EvaluateAsFloat(values);
			MainGame.Instance.GameSave.WorldData.MultiplyGameRes(id47, value21);
			return MainGame.Instance.GameSave.WorldData.GetGameRes(id47);
		});
		ctx.RegisterFunction("DivideWorldPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id46 = pars[0].EvaluateAsString(values);
			float num23 = pars[1].EvaluateAsFloat(values);
			MainGame.Instance.GameSave.WorldData.MultiplyGameRes(id46, 1f / num23);
			return MainGame.Instance.GameSave.WorldData.GetGameRes(id46);
		});
		ctx.RegisterFunction("SetWorldPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id45 = pars[0].EvaluateAsString(values);
			float value20 = pars[1].EvaluateAsFloat(values);
			MainGame.Instance.GameSave.WorldData.SetGameRes(id45, value20);
			return MainGame.Instance.GameSave.WorldData.GetGameRes(id45);
		});
		ctx.RegisterFunction("GetDeltaValue", (IExpression[] pars, IDictionary<string, object> values) => LazyExpressionEvaluationScope.DeltaValue);
		ctx.RegisterFunction("AttackerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			ICombatEntity combatEntity = LazyExpressionEvaluationScope.CombatEntity;
			if (combatEntity == null || (combatEntity is UnityEngine.Object object10 && object10 == null))
			{
				return 0;
			}
			string resId = pars[0].EvaluateAsString(values);
			return combatEntity.GetCombatEntityGameRes(resId);
		});
		ctx.RegisterFunction("AddInspiration", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string inspirationId = pars[0].EvaluateAsString(values);
			int counterToAdd = pars[1].EvaluateAsInt(values);
			MainGame.Instance.GameSave.talentSystemData.AddToInspiration(inspirationId, counterToAdd);
			return true;
		});
		ctx.RegisterFunction("AddTalentValue", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string talentId = pars[0].EvaluateAsString(values);
			int value19 = pars[1].EvaluateAsInt(values);
			return MainGame.Instance.GameSave.talentSystemData.AddTalentValue(talentId, value19);
		});
		ctx.RegisterFunction("TeleportTo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoIdTeleportTo = pars[0].EvaluateAsString(values);
			string environmentPreset2 = ((pars.Length > 1) ? pars[1].EvaluateAsString(values) : string.Empty);
			string soundOnTeleport2 = ((pars.Length > 2) ? pars[2].EvaluateAsString(values) : string.Empty);
			return PlayerController.Teleport(new WgoTeleportData(wgoIdTeleportTo, environmentPreset2, soundOnTeleport2));
		});
		ctx.RegisterFunction("TeleportToGD", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string gdPointId3 = pars[0].EvaluateAsString(values);
			string environmentPreset = ((pars.Length > 1) ? pars[1].EvaluateAsString(values) : string.Empty);
			string soundOnTeleport = ((pars.Length > 2) ? pars[2].EvaluateAsString(values) : string.Empty);
			return PlayerController.Teleport(new GDPointTeleportData(MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(gdPointId3), environmentPreset, soundOnTeleport));
		});
		ctx.RegisterFunction("AddPerk", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id44 = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.perkSystemData.AddPerk(id44);
			return true;
		});
		ctx.RegisterFunction("RemovePerk", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id43 = pars[0].EvaluateAsString(values);
			bool silent = pars[1].EvaluateAsBoolean(values);
			MainGame.Instance.GameSave.perkSystemData.RemovePerk(id43, silent);
			return true;
		});
		ctx.RegisterFunction("DropBurialRewards", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			Item item17 = null;
			foreach (Item item18 in LazyExpressionEvaluationScope.WgoData.Inventory.Data.Inventory)
			{
				if (item18.Definition.itemGroupIds.Contains("body"))
				{
					item17 = item18;
					break;
				}
			}
			if (item17 == null)
			{
				return false;
			}
			List<Item> list8 = new List<Item>();
			foreach (Item item19 in item17.Inventory)
			{
				if (item19.Definition.itemGroupIds.Contains("burial_reward"))
				{
					list8.Add(item19);
				}
			}
			foreach (Item item20 in list8)
			{
				MainGame.Instance.dropSystem.DropItem(new Item(item20.id, item20.Count), LazyExpressionEvaluationScope.WgoData.WorldId, LazyExpressionEvaluationScope.WgoData.GetDropPos(item20));
			}
			return true;
		});
		ctx.RegisterFunction("GSRun", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			GlobalScriptsManager.RunFlowScript(pars[0].EvaluateAsString(values), null);
			return true;
		});
		ctx.RegisterFunction("GSFireEvent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string scriptName = pars[0].EvaluateAsString(values);
			string eventName3 = pars[1].EvaluateAsString(values);
			GlobalScriptsManager.FireEvent(scriptName, eventName3);
			return true;
		});
		ctx.RegisterFunction("WGOFireEvent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text44 = pars[0].EvaluateAsString(values);
			string eventName2 = pars[1].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(text44);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't trigger fire event on wgo with tag:[" + text44 + "]. There is no such wgo!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.FireEvent(eventName2);
			return true;
		});
		ctx.RegisterFunction("FireEventOnWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string eventName = pars[0].EvaluateAsString(values);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't trigger fire event on wgo. LazyExpressionEvaluationScope.WgoData is not set!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.FireEvent(eventName);
			return true;
		});
		ctx.RegisterFunction("AddInteractionEvent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text43 = pars[0].EvaluateAsString(values);
			string id42 = pars[1].EvaluateAsString(values);
			bool isFake = pars.Length > 2 && pars[2].EvaluateAsBoolean(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(text43);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't add interaction event to wgo with tag:[" + text43 + "]. There is no such wgo!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.AddInteractionEvent(id42, isFake);
			return true;
		});
		ctx.RegisterFunction("RemoveInteractionEvent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text42 = pars[0].EvaluateAsString(values);
			string id41 = pars[1].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(text42);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't remove interaction event from wgo with tag:[" + text42 + "]. There is no such wgo!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.RemoveInteractionEvent(id41);
			return true;
		});
		ctx.RegisterFunction("SetTimeWithoutSleep", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			float timeWithoutSleep = pars[0].EvaluateAsFloat(values);
			MainGame.Instance.GameSave.playerData.energySystem.timeWithoutSleep = timeWithoutSleep;
			return true;
		});
		ctx.RegisterFunction("HasPlayerOvrhdItemByGrp", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string overheadItemGroup = pars[0].EvaluateAsString(values);
			return LazyExpressionEvaluationScope.AnyOverheadItem((Item item) => item.Definition.itemGroupIds.Contains(overheadItemGroup));
		});
		ctx.RegisterFunction("PlayerOvrhdItemIsZombie()", (IExpression[] pars, IDictionary<string, object> values) => LazyExpressionEvaluationScope.AnyOverheadItem((Item item) => MainGame.ZombieSystemData.Cache.ContainsKey(item.UniqueId.Guid)));
		ctx.RegisterFunction("HasPlayerOvrhdItem", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string overheadItemId = pars[0].EvaluateAsString(values);
			return string.IsNullOrEmpty(overheadItemId) ? ((object)false) : ((object)LazyExpressionEvaluationScope.AnyOverheadItem((Item item) => item.id == overheadItemId));
		});
		ctx.RegisterFunction("PlayerHasOvrhdAndCanInsert", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WgoData == null) ? ((object)false) : ((object)LazyExpressionEvaluationScope.AnyOverheadItem((Item item) => LazyExpressionEvaluationScope.WgoData.Inventory.CanAddItemToInventory(item))));
		ctx.RegisterFunction("AddItemToWgoInv", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id40 = pars[0].EvaluateAsString(values);
			int value18 = pars[1].EvaluateAsInt(values);
			LazyExpressionEvaluationScope.WgoData.Inventory.Data.AddItemToInventory(new Item(id40, value18));
			return true;
		});
		ctx.RegisterFunction("NoOvhdItem", (IExpression[] pars, IDictionary<string, object> values) => MainGame.PlayerData?.HasFreeOverheadSlot ?? true);
		ctx.RegisterFunction("HasOverheadWithSkulls", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string skullsName = pars[0].EvaluateAsString(values);
			int skullsCount = pars[1].EvaluateAsInt(values);
			return LazyExpressionEvaluationScope.AnyOverheadItem(delegate(Item item)
			{
				int num21 = 0;
				int num22 = 0;
				foreach (Item item21 in item.Inventory)
				{
					num21 += item21.Definition.redSkulls * item21.Count;
					num22 += item21.Definition.whiteSkulls * item21.Count;
				}
				num21 = Mathf.Clamp(num21, 0, 999);
				num22 = Mathf.Clamp(num22, 0, 999);
				return (skullsName == "red" && num21 >= skullsCount) || (skullsName == "white" && num22 >= skullsCount);
			});
		});
		ctx.RegisterFunction("PlayerHasIntrctSeed", delegate
		{
			Item interactingItem2 = MainGame.PlayerData.interactingItem;
			return (interactingItem2 != null && !string.IsNullOrEmpty(interactingItem2.id) && interactingItem2.IsSeed) ? ((object)true) : ((object)false);
		});
		ctx.RegisterFunction("CanPlayerFertilize", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null || LazyExpressionEvaluationScope.WgoData.CraftComponent.IsStarted)
			{
				return false;
			}
			Item interactingItem = MainGame.PlayerData.interactingItem;
			if (interactingItem == null || !interactingItem.IsFertilizer)
			{
				return false;
			}
			CraftDef craftDef = null;
			foreach (CraftDef item22 in GameBalance.Me.gardenCraftsPerItemCache[interactingItem.Definition])
			{
				if (item22.craftsIn.Contains(LazyExpressionEvaluationScope.WgoData.Definition.id))
				{
					craftDef = item22;
					break;
				}
			}
			if (craftDef == null)
			{
				return false;
			}
			foreach (LazyExpression onCraftEndExpression in craftDef.onCraftEndExpressions)
			{
				if (onCraftEndExpression.HasExpression && onCraftEndExpression.expressionString.Contains("AddPerkWgoSelf"))
				{
					string id39 = onCraftEndExpression.expressionString.Replace("AddPerkWgoSelf", "").Trim(')', '(', '"');
					if (LazyExpressionEvaluationScope.WgoData.HasPerk(id39))
					{
						return false;
					}
				}
			}
			return GardenInteractionHandler.HasFreeFertilizerPerkSlot(LazyExpressionEvaluationScope.WgoData);
		});
		ctx.RegisterFunction("HasPlayerItemInInv", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string itemId7 = pars[0].EvaluateAsString(values);
			int count4 = pars[1].EvaluateAsInt(values);
			return MainGame.PlayerData.Inventory.Data.HasItemQuantityInInventory(itemId7, count4) || MainGame.PlayerData.toolBeltInventory.Data.HasItemQuantityInInventory(itemId7, count4);
		});
		ctx.RegisterFunction("HasPlayerItemEquip", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string itemId6 = pars[0].EvaluateAsString(values);
			int count3 = pars[1].EvaluateAsInt(values);
			return MainGame.PlayerData.toolBeltInventory.Data.HasItemQuantityInInventory(itemId6, count3);
		});
		ctx.RegisterFunction("GetEquippedShovelIcon", delegate
		{
			Item item16 = MainGame.PlayerData?.toolBeltInventory?.GetItemByType(ItemType.Shovel);
			return (item16 == null || item16.IsEmpty) ? "i_shovel_0" : item16.Definition.iconId;
		});
		ctx.RegisterFunction("IsPlayerControlsEnabled", (IExpression[] pars, IDictionary<string, object> values) => MainGame.PlayerController.IsControlsEnabled);
		ctx.RegisterFunction("SetPlayerDamageImmunity", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			bool isImmuneToDamage = pars[0].EvaluateAsBoolean(values);
			MainGame.PlayerData.hpComponent.IsImmuneToDamage = isImmuneToDamage;
			return true;
		});
		ctx.RegisterFunction("UnlockPhrase", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text41 = pars[0].EvaluateAsString(values);
			if (!string.IsNullOrEmpty(text41))
			{
				MainGame.Instance.GameSave.knowledgeSystem.UnlockPhrase(text41);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("UnlockHudDaySprite", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text40 = pars[0].EvaluateAsString(values);
			if (!string.IsNullOrEmpty(text40) && !MainGame.Instance.GameSave.knowledgeSystem.unlockedCustomHudDaySprites.Contains(text40))
			{
				MainGame.Instance.GameSave.knowledgeSystem.unlockedCustomHudDaySprites.Add(text40);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("UnlockCustomizationPart", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id38 = pars[0].EvaluateAsString(values);
			int type8 = pars[1].EvaluateAsInt(values);
			return MainGame.PlayerData.customization.UnlockCustomizationPart(id38, (CustomizablePartType)type8);
		});
		ctx.RegisterFunction("UnlockCustomizationColor", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int type7 = pars[0].EvaluateAsInt(values);
			string paletteName = pars[1].EvaluateAsString(values);
			int partSkinId = pars[2].EvaluateAsInt(values);
			return MainGame.PlayerData.customization.UnlockCustomizationColor((PlayerColorCustomizationType)type7, paletteName, partSkinId);
		});
		ctx.RegisterFunction("SuperUnlockBodyCustomization", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id37 = pars[0].EvaluateAsString(values);
			return MainGame.PlayerData.customization.SuperUnlockBodyCustomization(id37, PlayerSkinHelper.CharacterCustomizationData, LazyExpressionEvaluationScope.Item);
		});
		ctx.RegisterFunction("BlackListPhrase", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text39 = pars[0].EvaluateAsString(values);
			if (!string.IsNullOrEmpty(text39))
			{
				MainGame.Instance.GameSave.knowledgeSystem.AddPhraseToBlackList(text39);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("RemovePhraseFromBlackList", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text38 = pars[0].EvaluateAsString(values);
			if (!string.IsNullOrEmpty(text38))
			{
				MainGame.Instance.GameSave.knowledgeSystem.RemovePhraseFromBlackList(text38);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("IncreasePlayerInventory", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int num20 = pars[0].EvaluateAsInt(values);
			if (num20 <= 0)
			{
				Debug.LogError("Increase inventory value can not be less or equal to 0");
				return false;
			}
			MainGame.PlayerData.IncreaseInventorySize(num20);
			return false;
		});
		ctx.RegisterFunction("IncreaseOverheadStackLimit", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int num19 = pars[0].EvaluateAsInt(values);
			if (num19 <= 0)
			{
				Debug.LogError("Increase overhead stack limit value can not be less or equal to 0");
				return false;
			}
			MainGame.PlayerData.AddRes("extra_overhead", num19);
			return true;
		});
		ctx.RegisterFunction("SetOverheadStackLimit", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int num18 = pars[0].EvaluateAsInt(values);
			if (num18 < 1)
			{
				Debug.LogError("Overhead stack limit can not be less than 1");
				return false;
			}
			MainGame.PlayerData.SetRes("extra_overhead", num18 - 1);
			return true;
		});
		ctx.RegisterFunction("ReducePlayerInventory", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int num17 = pars[0].EvaluateAsInt(values);
			if (num17 <= 0)
			{
				Debug.LogError("Reduce inventory value can not be less or equal to 0");
				return false;
			}
			MainGame.PlayerData.ReduceInventorySize(num17);
			return false;
		});
		ctx.RegisterFunction("AddPlayerHP", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int num16 = pars[0].EvaluateAsInt(values);
			HPComponent hpComponent = MainGame.PlayerData.hpComponent;
			if (num16 > 0)
			{
				hpComponent.AddHp(num16);
				return true;
			}
			if (num16 < 0)
			{
				hpComponent.ApplyDamage(Mathf.Abs(num16));
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("InsertOvrhdItem", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			Item overheadItemForMutation2 = LazyExpressionEvaluationScope.GetOverheadItemForMutation();
			if (overheadItemForMutation2 == null)
			{
				return false;
			}
			MainGame.PlayerData.InsertOverheadItemTo(LazyExpressionEvaluationScope.WgoData, overheadItemForMutation2);
			return true;
		});
		ctx.RegisterFunction("InsertOvrhdItemWithSkulls", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			Item overheadItemForMutation = LazyExpressionEvaluationScope.GetOverheadItemForMutation();
			if (overheadItemForMutation == null)
			{
				return false;
			}
			MainGame.PlayerData.InsertOverheadItemTo(LazyExpressionEvaluationScope.WgoData, overheadItemForMutation);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerInsertBodyWithSkulls, LazyExpressionEvaluationScope.WgoData.id + ":" + overheadItemForMutation.id);
			return true;
		});
		ctx.RegisterFunction("TakeOvrhdItem", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			if (!MainGame.PlayerData.HasFreeOverheadSlot)
			{
				return false;
			}
			string itemId5 = pars[0].EvaluateAsString(values);
			List<Item> list7 = LazyExpressionEvaluationScope.WgoData.Inventory.RemoveItemById(itemId5, 1);
			return (list7.Count > 0) ? ((object)MainGame.PlayerData.TryAddOverheadItemNoReplace(list7[0])) : ((object)false);
		});
		ctx.RegisterFunction("TakeOvrhdItemByGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			if (!MainGame.PlayerData.HasFreeOverheadSlot)
			{
				return false;
			}
			string groupId6 = pars[0].EvaluateAsString(values);
			List<Item> list6 = LazyExpressionEvaluationScope.WgoData.Inventory.RemoveItemByGroup(groupId6, 1);
			return (list6.Count > 0) ? ((object)MainGame.PlayerData.TryAddOverheadItemNoReplace(list6[0])) : ((object)false);
		});
		ctx.RegisterFunction("HasItemInInvByGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string item15 = pars[0].EvaluateAsString(values);
			foreach (Item item23 in LazyExpressionEvaluationScope.WgoData.Inventory.Data.Inventory)
			{
				if (item23.Definition.itemGroupIds.Contains(item15))
				{
					return true;
				}
			}
			return false;
		});
		ctx.RegisterFunction("RemoveItemByGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			WgoData wgoData11 = LazyExpressionEvaluationScope.WgoData;
			if (wgoData11 == null)
			{
				return false;
			}
			string groupId5 = pars[0].EvaluateAsString(values);
			int num15 = pars[1].EvaluateAsInt(values);
			if (wgoData11.Inventory.Data.TryGetItemInInventoryByGroupId(groupId5, out var itemResult) && itemResult.Count >= num15)
			{
				wgoData11.Inventory.RemoveItemFromInventoryByUID(itemResult, num15);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("HasItemInInv", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string itemId4 = pars[0].EvaluateAsString(values);
			int count2 = pars[1].EvaluateAsInt(values);
			return LazyExpressionEvaluationScope.WgoData.Inventory.Data.HasItemQuantityInInventory(itemId4, count2);
		});
		ctx.RegisterFunction("DropItem", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id36 = pars[0].EvaluateAsString(values);
			int value17 = pars[1].EvaluateAsInt(values);
			PlayerData playerData3 = MainGame.PlayerData;
			MainGame.Instance.dropSystem.DropItem(new Item(id36, value17), MainGame.PlayerData.currentGameSceneId, playerData3.position.Value + new Vector3(playerData3.Direction.x, 0f, playerData3.Direction.y));
			return true;
		});
		ctx.RegisterFunction("DropItemByGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string item13 = pars[0].EvaluateAsString(values);
			Item item14 = null;
			foreach (Item item24 in LazyExpressionEvaluationScope.WgoData.Inventory.Data.Inventory)
			{
				if (item24.Definition.itemGroupIds.Contains(item13))
				{
					item14 = item24;
					break;
				}
			}
			if (item14 == null)
			{
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.Inventory.RemoveItemFromInventoryByUID(item14);
			Vector3 normalized = (MainGame.PlayerData.position.Value - LazyExpressionEvaluationScope.WgoData.Position).normalized;
			MainGame.Instance.dropSystem.DropItem(item14, LazyExpressionEvaluationScope.WgoData.WorldId, LazyExpressionEvaluationScope.WgoData.Position + normalized);
			return true;
		});
		ctx.RegisterFunction("DropHappiness", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int h = pars[0].EvaluateAsInt(values);
			TechPointsSpawner.CreateSpawner(MainGame.PlayerData.position.Value, 0, 0, 0, h);
			return true;
		});
		ctx.RegisterFunction("UseItem", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string itemId3 = pars[0].EvaluateAsString(values);
			PlayerData playerData2 = MainGame.PlayerData;
			Item itemById2 = playerData2.inventory.GetItemById(itemId3);
			playerData2.UseItem(itemById2);
			return true;
		});
		ctx.RegisterFunction("RemoveItem", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string itemId2 = pars[0].EvaluateAsString(values);
			PlayerData playerData = MainGame.PlayerData;
			Item itemById = playerData.inventory.GetItemById(itemId2);
			playerData.RemoveItem(itemById);
			return true;
		});
		ctx.RegisterFunction("ChangeWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string newWgoId2 = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.WorldData.ChangeWgoData(LazyExpressionEvaluationScope.WgoData, newWgoId2);
			return true;
		});
		ctx.RegisterFunction("SpawnWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId7 = pars[0].EvaluateAsString(values);
			float x2 = pars[1].EvaluateAsFloat(values);
			float y2 = pars[2].EvaluateAsFloat(values);
			float z2 = pars[3].EvaluateAsFloat(values);
			string gameSceneId4 = pars[4].EvaluateAsString(values);
			string customTag6 = pars[5].EvaluateAsString(values);
			MainGame.Instance.GameSave.WorldData.AddWgoData(wgoId7, new Vector3(x2, y2, z2), gameSceneId4, customTag6, out var _);
			return true;
		});
		ctx.RegisterFunction("SpawnWgoOnGDPoint", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId6 = pars[0].EvaluateAsString(values);
			string text37 = pars[1].EvaluateAsString(values);
			string gameSceneId3 = pars[2].EvaluateAsString(values);
			string customTag5 = pars[3].EvaluateAsString(values);
			GDPointData gDPointDataById3 = MainGame.Instance.GameSave.WorldData.gdPointsData.GetGDPointDataById(text37);
			if (gDPointDataById3 == null)
			{
				Debug.LogError("No gd point:[" + text37 + "] can't spawn wgo");
				return false;
			}
			MainGame.Instance.GameSave.WorldData.AddWgoData(wgoId6, gDPointDataById3.Position, gameSceneId3, customTag5, out var _);
			return true;
		});
		ctx.RegisterFunction("SpawnConveyorWgoOnGDPoint", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId5 = pars[0].EvaluateAsString(values);
			string text36 = pars[1].EvaluateAsString(values);
			string gameSceneId2 = pars[2].EvaluateAsString(values);
			string customTag4 = pars[3].EvaluateAsString(values);
			GDPointData gDPointDataById2 = MainGame.Instance.GameSave.WorldData.gdPointsData.GetGDPointDataById(text36);
			if (gDPointDataById2 == null)
			{
				Debug.LogError("No gd point:[" + text36 + "] can't spawn wgo");
				return false;
			}
			MainGame.Instance.GameSave.WorldData.AddWgoData(wgoId5, gDPointDataById2.Position, gameSceneId2, customTag4, out var wgoData8);
			if (wgoData8 is ConveyorWgoData conveyorWgoData3)
			{
				Wgo wgoViewGlobal3 = GameScene.GetWgoViewGlobal(conveyorWgoData3.UniqueId);
				if (wgoViewGlobal3 != null)
				{
					ConveyorBuildPointer.TryMakeConnectionsOutsideBuildSystem(wgoViewGlobal3);
				}
				else
				{
					MainGame.Instance.conveyorSystem.AddConveyorObject(conveyorWgoData3.ConveyorComponent);
				}
			}
			return true;
		});
		ctx.RegisterFunction("SpawnConveyorWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId4 = pars[0].EvaluateAsString(values);
			float x = pars[1].EvaluateAsFloat(values);
			float y = pars[2].EvaluateAsFloat(values);
			float z = pars[3].EvaluateAsFloat(values);
			string gameSceneId = pars[4].EvaluateAsString(values);
			string customTag3 = pars[5].EvaluateAsString(values);
			MainGame.Instance.GameSave.WorldData.AddWgoData(wgoId4, new Vector3(x, y, z), gameSceneId, customTag3, out var wgoData7);
			if (wgoData7 is ConveyorWgoData conveyorWgoData2)
			{
				Wgo wgoViewGlobal2 = GameScene.GetWgoViewGlobal(conveyorWgoData2.UniqueId);
				if (wgoViewGlobal2 != null)
				{
					ConveyorBuildPointer.TryMakeConnectionsOutsideBuildSystem(wgoViewGlobal2);
				}
				else
				{
					MainGame.Instance.conveyorSystem.AddConveyorObject(conveyorWgoData2.ConveyorComponent);
				}
			}
			return true;
		});
		ctx.RegisterFunction("SpawnConveyorWgoOnThis", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text35 = pars[0].EvaluateAsString(values);
			bool flag7 = false;
			if (pars.Length > 1)
			{
				flag7 = pars[1].EvaluateAsBoolean(values);
			}
			WgoData wgoData5 = LazyExpressionEvaluationScope.WgoData;
			if (wgoData5 == null)
			{
				Debug.LogError("Current WgoData is null. Can not spawn ConveyorWgo [" + text35 + "]");
				return false;
			}
			MainGame.Instance.GameSave.WorldData.AddWgoData(text35, new Vector3(wgoData5.Position.x, wgoData5.Position.y, wgoData5.Position.z), wgoData5.WorldId, "", out var wgoData6);
			if (flag7)
			{
				wgoData6.ApplyWgoPartState(wgoData5.MainWgoPartData.variationId, wgoData5.MainWgoPartData.rotationIndex);
			}
			if (wgoData6 is ConveyorWgoData conveyorWgoData)
			{
				Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(conveyorWgoData.UniqueId);
				if (wgoViewGlobal != null)
				{
					ConveyorBuildPointer.TryMakeConnectionsOutsideBuildSystem(wgoViewGlobal);
				}
				else
				{
					MainGame.Instance.conveyorSystem.AddConveyorObject(conveyorWgoData.ConveyorComponent);
				}
			}
			MainGame.WorldData.RemoveWgoDataFromGameScene(wgoData5);
			return true;
		});
		ctx.RegisterFunction("ReplaceWgoByIds", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id35 = pars[0].EvaluateAsString(values);
			string newWgoId = pars[1].EvaluateAsString(values);
			MainGame.Instance.GameSave.WorldData.ChangeWgoData(MainGame.Instance.GameSave.WorldData.GetWgoData(id35), newWgoId);
			return true;
		});
		ctx.RegisterFunction("SetWgoHiddenState", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text34 = pars[0].EvaluateAsString(values);
			bool flag6 = pars[1].EvaluateAsBoolean(values);
			WgoData wgoData4 = MainGame.Instance.GameSave.WorldData.GetWgoData(text34);
			if (wgoData4 == null)
			{
				Debug.LogError($"Can not set wgo [{text34}] hidden state [{flag6}]. Wgo is null");
				return false;
			}
			wgoData4.IsHidden = flag6;
			return true;
		});
		ctx.RegisterFunction("SetWgoHiddenStateByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text33 = pars[0].EvaluateAsString(values);
			bool flag5 = pars[1].EvaluateAsBoolean(values);
			WgoData wgoDataByCustomTag5 = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text33);
			if (wgoDataByCustomTag5 == null)
			{
				Debug.LogError($"Can not set wgo with tag [{text33}] hidden state [{flag5}]. Wgo is null");
				return false;
			}
			wgoDataByCustomTag5.IsHidden = flag5;
			return true;
		});
		ctx.RegisterFunction("CanAddItemToWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string itemId = pars[0].EvaluateAsString(values);
			int count = pars[1].EvaluateAsInt(values);
			return (LazyExpressionEvaluationScope.WgoData == null) ? ((object)false) : ((object)LazyExpressionEvaluationScope.WgoData.Inventory.CanAddItemToInventory(itemId, count));
		});
		ctx.RegisterFunction("AddWgoPart", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string wgoPartId2 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData.AddWgoPart(wgoPartId2);
			return true;
		});
		ctx.RegisterFunction("RemoveWgoPart", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string wgoPartId = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData.RemoveWgoPart(wgoPartId);
			return true;
		});
		ctx.RegisterFunction("RemoveAllWgoParts", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.RemoveAllWgoParts();
			return true;
		});
		ctx.RegisterFunction("SetWgoPartStateAll", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			LazyExpressionEvaluationScope.WgoData.ApplyWgoPartState(pars[0].EvaluateAsString(values), (pars.Length > 1) ? pars[1].EvaluateAsInt(values) : LazyExpressionEvaluationScope.WgoData.MainWgoPartData.rotationIndex);
			return true;
		});
		ctx.RegisterFunction("SetWgoVariation", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text32 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.worldData.GetWgoData(text32);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't remove interaction event from wgo with id:[" + text32 + "]. There is no such wgo!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.ApplyWgoPartState(pars[1].EvaluateAsString(values), (pars.Length > 2) ? pars[2].EvaluateAsInt(values) : LazyExpressionEvaluationScope.WgoData.MainWgoPartData.rotationIndex);
			return true;
		});
		ctx.RegisterFunction("GetItemByGrp", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string item12 = pars[0].EvaluateAsString(values);
			foreach (Item item25 in LazyExpressionEvaluationScope.WgoData.Inventory.Data.Inventory)
			{
				if (item25.Definition.itemGroupIds.Contains(item12))
				{
					return item25;
				}
			}
			return false;
		});
		ctx.RegisterFunction("GetItemById", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string text31 = pars[0].EvaluateAsString(values);
			foreach (Item item26 in LazyExpressionEvaluationScope.WgoData.Inventory.Data.Inventory)
			{
				if (item26.id == text31)
				{
					return item26;
				}
			}
			return false;
		});
		ctx.RegisterFunction("OpenAutopsyWindow", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			OpenAutopsyWindow(LazyExpressionEvaluationScope.WgoData);
			return true;
		});
		ctx.RegisterFunction("OpenAutopsyWindow_CB", delegate
		{
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.worldData.GetWgoData("autopsy_table_1");
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return (object)null;
			}
			bool flag4 = LazyExpressionEvaluationScope.CustomCallback != null;
			Action callback = (flag4 ? LazyExpressionEvaluationScope.CustomCallback : null);
			OpenAutopsyWindow(LazyExpressionEvaluationScope.WgoData, delegate
			{
				callback?.Invoke();
			});
			LazyExpressionEvaluationScope.CustomCallback = null;
			return new CallbackAvailableMarker();
		});
		ctx.RegisterFunction("OpenResurrectionWindow", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			LazyUI.GetWindow<UIResurrectionWindow>().Open(new UIResurrectionWindowData(LazyExpressionEvaluationScope.WgoData));
			return true;
		});
		ctx.RegisterFunction("OpenDemoEndWindow", delegate
		{
			LazyUI.GetWindow<UIDemoEndWindow>().Open(null);
			return true;
		});
		ctx.RegisterFunction("OpenCustomizationWindow", delegate
		{
			UICustomizationWindowData data9 = new UICustomizationWindowData
			{
				CurrentData = PlayerCustomizationData.Copy(MainGame.PlayerData.customization)
			};
			UICustomizationWindow customizationWindow = LazyUI.GetWindow<UICustomizationWindow>();
			customizationWindow.Open(data9);
			customizationWindow.OnCustomizationApplied += OnCustomizationApplied;
			return true;
			void OnCustomizationApplied(PlayerCustomizationData data)
			{
				MainGame.PlayerData.ApplyCustomization(data);
				customizationWindow.OnCustomizationApplied -= OnCustomizationApplied;
			}
		});
		ctx.RegisterFunction("OpenNotesWindow", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			UINotesWindow window3 = LazyUI.GetWindow<UINotesWindow>();
			string id34 = pars[0].EvaluateAsString(values);
			UINotesWindowData data8 = new UINotesWindowData(GameBalance.Me.GetData<ItemDef>(id34));
			window3.Open(data8);
			return true;
		});
		ctx.RegisterFunction("UnlockTechTab", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int techTreeTab2 = pars[0].EvaluateAsInt(values);
			MainGame.Instance.GameSave.knowledgeSystem.UnlockTechTab((TechTreeTab)techTreeTab2);
			return true;
		});
		ctx.RegisterFunction("LockTechTab", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int techTreeTab = pars[0].EvaluateAsInt(values);
			MainGame.Instance.GameSave.knowledgeSystem.LockTechTab((TechTreeTab)techTreeTab);
			return true;
		});
		ctx.RegisterFunction("RevealTech", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string techId = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.RevealTech(techId);
			return true;
		});
		ctx.RegisterFunction("AddTech", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id33 = pars[0].EvaluateAsString(values);
			GameBalance.Me.GetData<TechDef>(id33).Unlock(free: true);
			return true;
		});
		ctx.RegisterFunction("RemoveTech", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id32 = pars[0].EvaluateAsString(values);
			GameBalance.Me.GetData<TechDef>(id32).RemoveTech();
			return true;
		});
		ctx.RegisterFunction("IsTechUnlocked", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text30 = pars[0].EvaluateAsString(values);
			return !string.IsNullOrEmpty(text30) && MainGame.Instance.GameSave.knowledgeSystem.unlockedTechs.Contains(text30);
		});
		ctx.RegisterFunction("UnlockCraft", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string craftId3 = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.UnlockCraft(craftId3);
			return true;
		});
		ctx.RegisterFunction("UnlockOrgan", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int itemType = pars[0].EvaluateAsInt(values);
			MainGame.Instance.GameSave.knowledgeSystem.UnlockOrgan((ItemType)itemType);
			return true;
		});
		ctx.RegisterFunction("UnlockTownBuilding", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string buildingId2 = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.UnlockTownBuilding(buildingId2);
			return true;
		});
		ctx.RegisterFunction("LockTownBuilding", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string buildingId = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.LockTownBuilding(buildingId);
			return true;
		});
		ctx.RegisterFunction("UnlockMapFightIcon", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string icon2 = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.UnlockMapFightIcon(icon2);
			return true;
		});
		ctx.RegisterFunction("LockMapFightIcon", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string icon = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.LockMapFightIcon(icon);
			return true;
		});
		ctx.RegisterFunction("RevealTalentLevelUp", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string talentLevelUpId = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.knowledgeSystem.RevealTalentLevelUp(talentLevelUpId);
			return true;
		});
		ctx.RegisterFunction("IsNotCraftingNow", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WgoData == null) ? ((object)false) : ((object)(!LazyExpressionEvaluationScope.WgoData.CraftComponent.IsStarted)));
		ctx.RegisterFunction("AddMoneyToVendor", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string vendorId2 = pars[0].EvaluateAsString(values);
			int money = pars[1].EvaluateAsInt(values);
			MainGame.Instance.GameSave.vendorSystem.AddMoneyToVendor(vendorId2, money);
			return true;
		});
		ctx.RegisterFunction("LvlUpVendor", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string vendorId = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.vendorSystem.ForceLevelUpVendor(vendorId);
			return true;
		});
		ctx.RegisterFunction("ChangeCharTabLockedState", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int page = pars[0].EvaluateAsInt(values);
			if (pars[1].EvaluateAsBoolean(values))
			{
				MainGame.Instance.GameSave.knowledgeSystem.LockCharTab((CharacterWindowData.CharPage)page);
			}
			else
			{
				MainGame.Instance.GameSave.knowledgeSystem.UnlockCharTab((CharacterWindowData.CharPage)page);
			}
			return true;
		});
		ctx.RegisterFunction("AddRepWGO", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id31 = pars[0].EvaluateAsString(values);
			int value16 = pars[1].EvaluateAsInt(values);
			WGODef data7 = GameBalance.Me.GetData<WGODef>(id31);
			MainGame.Instance.GameSave.playerData.AddNPCRep(data7.repResName, value16);
			return true;
		});
		ctx.RegisterFunction("SetRepWGO", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id30 = pars[0].EvaluateAsString(values);
			int value15 = pars[1].EvaluateAsInt(values);
			WGODef data6 = GameBalance.Me.GetData<WGODef>(id30);
			MainGame.Instance.GameSave.playerData.SetNPCRep(data6.repResName, value15);
			return true;
		});
		ctx.RegisterFunction("AddRep", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string repRes2 = pars[0].EvaluateAsString(values);
			int value14 = pars[1].EvaluateAsInt(values);
			MainGame.Instance.GameSave.playerData.AddNPCRep(repRes2, value14);
			return true;
		});
		ctx.RegisterFunction("SetRep", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string repRes = pars[0].EvaluateAsString(values);
			int value13 = pars[1].EvaluateAsInt(values);
			MainGame.Instance.GameSave.playerData.SetNPCRep(repRes, value13);
			return true;
		});
		ctx.RegisterFunction("HasCraft", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WgoData == null) ? ((object)false) : ((object)(LazyExpressionEvaluationScope.WgoData.CraftComponent.CurrentCraftElement != null)));
		ctx.RegisterFunction("StartCraft", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string craftId2 = pars[0].EvaluateAsString(values);
			if (GameBalance.GetCraftDef(craftId2) == null)
			{
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.CraftComponent.AddToQueue(new CraftElement(craftId2, 1, new CraftParamsData(craftId2, LazyExpressionEvaluationScope.WgoData)));
			return true;
		});
		ctx.RegisterFunction("StartCraftOnWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text28 = pars[0].EvaluateAsString(values);
			string text29 = pars[1].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text28) || string.IsNullOrEmpty(text29))
			{
				Debug.LogError("wgoId or craftId is null. wgoId: " + text28 + ", craftId: " + text29);
				return false;
			}
			WgoData wgoData3 = MainGame.Instance.GameSave.WorldData.GetWgoData(text28);
			wgoData3.CraftComponent.AddToQueue(new CraftElement(text29, 1, new CraftParamsData(text29, wgoData3)));
			return true;
		});
		ctx.RegisterFunction("CrIn_SummQualGrd10", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			CraftElementBase craftElementBase2 = LazyExpressionEvaluationScope.WgoData.CraftComponent.CraftElementsQueue[LazyExpressionEvaluationScope.WgoData.CraftComponent.CraftElementsQueue.Count - 1];
			if (craftElementBase2 == null)
			{
				return 0;
			}
			string item11 = pars[0].EvaluateAsString(values);
			float num14 = 0f;
			foreach (NeedItemData requirement in craftElementBase2.Requirements)
			{
				if (!requirement.IsGroup && requirement.ItemDef != null && requirement.ItemDef.itemGroupIds.Contains(item11))
				{
					num14 += (float)requirement.ItemDef.quality;
				}
			}
			return num14 / 10f;
		});
		ctx.RegisterFunction("CrIn_SummQualGrd", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			CraftElementBase craftElementBase = LazyExpressionEvaluationScope.WgoData.CraftComponent.CraftElementsQueue[LazyExpressionEvaluationScope.WgoData.CraftComponent.CraftElementsQueue.Count - 1];
			if (craftElementBase == null)
			{
				return 0;
			}
			string item10 = pars[0].EvaluateAsString(values);
			float num13 = 0f;
			foreach (NeedItemData requirement2 in craftElementBase.Requirements)
			{
				if (!requirement2.IsGroup && requirement2.ItemDef != null && requirement2.ItemDef.itemGroupIds.Contains(item10))
				{
					num13 += (float)requirement2.ItemDef.quality;
				}
			}
			return num13;
		});
		ctx.RegisterFunction("OpenGardenWindow", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			UIGardenBedWindow window2 = LazyUI.GetWindow<UIGardenBedWindow>();
			UIGardenBedWindowData data5 = new UIGardenBedWindowData(LazyExpressionEvaluationScope.WgoData);
			window2.Open(data5);
			return true;
		});
		ctx.RegisterFunction("OpenSurveyResultWindow", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string text27 = pars[0].EvaluateAsString(values);
			SurveyDef surveyDefOrNull = GameBalance.GetSurveyDefOrNull("surv:" + text27);
			if (LazyExpressionEvaluationScope.WgoData.CraftComponent.CurrentCraftElement is CraftElementSurvey craftElementSurvey && !string.IsNullOrEmpty(craftElementSurvey.SelectedSurveyItemId))
			{
				text27 = craftElementSurvey.SelectedSurveyItemId;
			}
			else if (surveyDefOrNull != null)
			{
				List<ItemDef> surveyedItemDefs = surveyDefOrNull.GetSurveyedItemDefs();
				if (surveyedItemDefs.Count > 0)
				{
					text27 = surveyedItemDefs[0].id;
				}
			}
			LazyUI.GetWindow<UISurveyResultWindow>().Open(new UISurveyResultWindowData(GameBalance.Me.GetData<ItemDef>(text27)));
			return true;
		});
		ctx.RegisterFunction("TryOpenTechTree", delegate
		{
			if (!MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.TechTree))
			{
				LazyUI.GetWindow<CharacterWindow>().Open(new CharacterWindowData(MainGame.Instance.GameSave, CharacterWindowData.CharPage.TechTree));
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("TryOpenQuestTree", delegate
		{
			if (!MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.QuestTree))
			{
				LazyUI.GetWindow<CharacterWindow>().Open(new CharacterWindowData(MainGame.Instance.GameSave, CharacterWindowData.CharPage.QuestTree));
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("ShowTechUnlock", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text26 = pars[0].EvaluateAsString(values);
			TechDef data4 = GameBalance.Me.GetData<TechDef>(text26);
			if (!MainGame.Instance.GameSave.knowledgeSystem.IsTechUnlocked(data4.id))
			{
				Debug.LogError("Tech:[" + text26 + "] is not unlocked!!!");
				return false;
			}
			if (data4.techDefType != 0)
			{
				Debug.LogError("Tech:[" + text26 + "] is not available to show unlock window!!!");
				return false;
			}
			UITechTreeElementWindow window = LazyUI.GetWindow<UITechTreeElementWindow>();
			UIDialogWindowData.ButtonData item9 = new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UITechTreeElementWindow>().Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
			window.Open(new UITechTreeElementWindowData(data4, new List<UIDialogWindowData.ButtonData> { item9 }, null, "ui_new_tech_unlocked"));
			return true;
		});
		ctx.RegisterFunction("AddWGOPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id29 = pars[0].EvaluateAsString(values);
			float value12 = pars[1].EvaluateAsFloat(values);
			LazyExpressionEvaluationScope.WgoData.AddGameRes(id29, value12);
			return true;
		});
		ctx.RegisterFunction("DecWGOPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id28 = pars[0].EvaluateAsString(values);
			float num12 = pars[1].EvaluateAsFloat(values);
			LazyExpressionEvaluationScope.WgoData.AddGameRes(id28, 0f - num12);
			return true;
		});
		ctx.RegisterFunction("MultiplyWGOPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id27 = pars[0].EvaluateAsString(values);
			float value11 = pars[1].EvaluateAsFloat(values);
			LazyExpressionEvaluationScope.WgoData.MultiplyGameRes(id27, value11);
			return true;
		});
		ctx.RegisterFunction("DivideWGOPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id26 = pars[0].EvaluateAsString(values);
			float num11 = pars[1].EvaluateAsFloat(values);
			LazyExpressionEvaluationScope.WgoData.MultiplyGameRes(id26, 1f / num11);
			return true;
		});
		ctx.RegisterFunction("SetWGOPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id25 = pars[0].EvaluateAsString(values);
			float value10 = pars[1].EvaluateAsFloat(values);
			LazyExpressionEvaluationScope.WgoData.SetGameRes(id25, value10);
			return true;
		});
		ctx.RegisterFunction("WGOPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			string id24 = pars[0].EvaluateAsString(values);
			return LazyExpressionEvaluationScope.WgoData.GetGameRes(id24);
		});
		ctx.RegisterFunction("SetWGOParByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text25 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text25);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't set par to wgo with tag:[" + text25 + "]. There is no such wgo!");
				return false;
			}
			string id23 = pars[1].EvaluateAsString(values);
			float value9 = pars[2].EvaluateAsFloat(values);
			LazyExpressionEvaluationScope.WgoData.SetGameRes(id23, value9);
			return true;
		});
		ctx.RegisterFunction("Spawning", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			string craftId = LazyExpressionEvaluationScope.WgoData.CraftComponent.CurrentCraftElement.CraftId;
			SpawnStage spawnStage = LazyExpressionEvaluationScope.WgoData.SpawnWGOComponent.FindStageById(craftId);
			if (spawnStage == null)
			{
				Debug.LogError("Spawning null stage for id:[" + craftId + "]!!!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.worldData.ReplaceWgoData(LazyExpressionEvaluationScope.WgoData, spawnStage.variations.GetRandom().wgoPartId);
			if (!string.IsNullOrEmpty(spawnStage.craftId))
			{
				if (GameBalance.GetCraftDef(spawnStage.craftId) == null)
				{
					return false;
				}
				LazyExpressionEvaluationScope.WgoData.CraftComponent.AddToQueue(new CraftElement(spawnStage.craftId, 1, new CraftParamsData(spawnStage.craftId, LazyExpressionEvaluationScope.WgoData)));
			}
			return true;
		});
		ctx.RegisterFunction("SetRandomSpawnWgoStages", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			List<string> list4 = new List<string>();
			foreach (IExpression e in pars)
			{
				list4.Add(e.EvaluateAsString(values));
			}
			string random = list4.GetRandom();
			AsyncOperationHandle<SpawnConfiguration> asyncOperationHandle = Addressables.LoadAssetAsync<SpawnConfiguration>("Assets/AddressableAssets/SpawnerConfigurations/" + random + ".asset");
			SpawnConfiguration spawnConfiguration = asyncOperationHandle.WaitForCompletion();
			if (spawnConfiguration == null)
			{
				Debug.LogError("Can not find SpawnConfiguration with id " + random);
				return false;
			}
			List<SpawnStage> list5 = new List<SpawnStage>();
			list5.AddRange(spawnConfiguration.SpawnStages);
			LazyExpressionEvaluationScope.WgoData.SpawnWGOComponent.SpawnStages = list5;
			asyncOperationHandle.Release();
			return true;
		});
		ctx.RegisterFunction("SetRandomVariation", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			LazyExpressionEvaluationScope.WgoData.ApplyRandomState();
			return true;
		});
		ctx.RegisterFunction("AddPerkWgoSelf", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			string id22 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData.AddPerk(id22);
			return true;
		});
		ctx.RegisterFunction("RemovePerkWgoSelf", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			string id21 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData.RemovePerk(id21);
			return true;
		});
		ctx.RegisterFunction("RemoveAllPerksWgoSelf", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			LazyExpressionEvaluationScope.WgoData.RemoveAllPerks();
			return true;
		});
		ctx.RegisterFunction("AddPerkToWgoByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text24 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text24);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't add perk to wgo with tag:[" + text24 + "]. There is no such wgo!");
				return false;
			}
			string id20 = pars[1].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData.AddPerk(id20);
			return true;
		});
		ctx.RegisterFunction("RemovePerkFromWgoByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text23 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text23);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't remove perk from wgo with tag:[" + text23 + "]. There is no such wgo!");
				return false;
			}
			string id19 = pars[1].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData.RemovePerk(id19);
			return true;
		});
		ctx.RegisterFunction("SetWgoInteractionState", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text22 = pars[0].EvaluateAsString(values);
			bool flag3 = pars[1].EvaluateAsBoolean(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text22);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError($"Can't set interactableState with tag: [{text22}] to: [{flag3}]. There is no such wgo!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.IsInteractable = flag3;
			return true;
		});
		ctx.RegisterFunction("SetWgoGameResInt", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text21 = pars[0].EvaluateAsString(values);
			string id18 = pars[1].EvaluateAsString(values);
			int value8 = pars[2].EvaluateAsInt(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text21);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't set res with tag: [" + text21 + "]. There is no such wgo!");
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.SetGameRes(id18, value8);
			return true;
		});
		ctx.RegisterFunction("DestroyWgoData", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text20 = pars[0].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text20);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Can't destroy wgo data with tag: [" + text20 + "]. There is no such wgo!");
				return false;
			}
			MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(LazyExpressionEvaluationScope.WgoData);
			return true;
		});
		ctx.RegisterFunction("CreateTownBuilding", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			MainGame.Instance.GameSave.townSystem.CreateTownBuildingOnWgo(LazyExpressionEvaluationScope.WgoData);
			return true;
		});
		ctx.RegisterFunction("DestroySignboard", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			Debug.Log($"Destroying signboard for wgo:[{LazyExpressionEvaluationScope.WgoData.UniqueId}]");
			MainGame.WorldData.RemoveWgoDataFromGameScene(LazyExpressionEvaluationScope.WgoData.UniqueId);
			return true;
		});
		ctx.RegisterFunction("RepairHousesById", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text19 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text19))
			{
				Debug.LogError("[RepairHousesById] WSO ID is null or empty");
				return false;
			}
			int stageIdx2 = ((pars.Length != 0) ? pars[1].EvaluateAsInt(values) : (-1));
			if (MainGame.Instance.GameSave.worldData.Cache.wsoDataByIdCache.TryGetValue(text19, out var value7))
			{
				foreach (WsoData item27 in value7)
				{
					TownUtils.RepairHouse(item27, stageIdx2);
				}
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("RepairHousesByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text18 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text18))
			{
				Debug.LogError("[RepairHousesById] WSO tag is null or empty");
				return false;
			}
			int stageIdx = ((pars.Length != 0) ? pars[1].EvaluateAsInt(values) : (-1));
			if (MainGame.Instance.GameSave.worldData.Cache.wsoDataByCustomTagsCache.TryGetValue(text18, out var value6))
			{
				foreach (WsoData item28 in value6)
				{
					TownUtils.RepairHouse(item28, stageIdx);
				}
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("SetResWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text17 = pars[0].EvaluateAsString(values);
			string id17 = pars[1].EvaluateAsString(values);
			float value4 = pars[2].EvaluateAsFloat(values);
			if (string.IsNullOrEmpty(text17))
			{
				Debug.LogError("WGO ID is null or empty");
				return false;
			}
			if (MainGame.Instance.GameSave.worldData.Cache.wgoDataByIdsCache.TryGetValue(text17, out var value5))
			{
				value5[0].SetGameRes(id17, value4);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("LazyExpressionOnWgo", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text15 = pars[0].EvaluateAsString(values);
			string text16 = pars[1].EvaluateAsString(values);
			LazyExpressionEvaluationScope.WgoData = MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(text15);
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				Debug.LogError("Unable to call LE on wgo data with custom tag: [" + text15 + "]. There is no such wgo!");
				return false;
			}
			return new LazyExpression(text16).EvaluateBool(LazyExpressionEvaluationScope.WgoData);
		});
		ctx.RegisterFunction("TryPlacePlantOrder", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WgoData == null) ? ((object)false) : ((object)GardenInteractionHandler.TryPlacePlantOrder(LazyExpressionEvaluationScope.WgoData)));
		ctx.RegisterFunction("TryPlaceGatherOrder", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WgoData == null) ? ((object)false) : ((object)GardenInteractionHandler.TryPlaceGatherOrder(LazyExpressionEvaluationScope.WgoData)));
		ctx.RegisterFunction("TryFinishPanicReductionMachineCraft", delegate
		{
			WgoData wgoDataByCustomTag4 = MainGame.WorldData.GetWgoDataByCustomTag("panic_reduction_machine");
			if (wgoDataByCustomTag4 == null)
			{
				Debug.LogWarning("Can wgo with tag [panic_reduction_machine] to finish it craft");
				return false;
			}
			if (wgoDataByCustomTag4.CraftComponent.CurrentCraftElement != null)
			{
				wgoDataByCustomTag4.CraftComponent.TryFinishCurCraft();
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("KillerIsPlayer", delegate
		{
			if (LazyExpressionEvaluationScope.CombatEntityAttacksMe == null)
			{
				return 0;
			}
			ICombatEntity combatEntityAttacksMe = LazyExpressionEvaluationScope.CombatEntityAttacksMe;
			return (combatEntityAttacksMe == null || (combatEntityAttacksMe is UnityEngine.Object object9 && object9 == null)) ? ((object)0) : ((object)((combatEntityAttacksMe is PlayerPhysicalBody) ? 1 : 0));
		});
		ctx.RegisterFunction("GetChurchObjectQuality", delegate
		{
			WgoData wgoData2 = LazyExpressionEvaluationScope.WgoData;
			if (wgoData2 == null)
			{
				return 0;
			}
			int num10 = 0;
			string id16 = wgoData2.id;
			if (!(id16 == "zmb_choir_place"))
			{
				if (id16 == "zmb_organ_place")
				{
					num10 += ConstDef.Get("church_organ_default_quality").IntValue;
				}
			}
			else
			{
				num10 += ConstDef.Get("church_choir_default_quality").IntValue;
			}
			foreach (Item item29 in wgoData2.Inventory.GetItemsByGroupId("zombie"))
			{
				ZombieWgoData zombie = MainGame.Instance.GameSave.zombieSystemData.GetZombie(item29.UniqueId);
				if (zombie != null)
				{
					num10 = ((zombie.GetGameResInt("zperk_musician_master") <= 0) ? ((zombie.GetGameResInt("zperk_musician_advanced") <= 0) ? ((zombie.GetGameResInt("zperk_musician_basic") <= 0) ? (num10 + 1) : (num10 + 2)) : (num10 + 4)) : (num10 + 7));
				}
			}
			return num10;
		});
		ctx.RegisterFunction("AddCustomRectToWZ", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id15 = pars[0].EvaluateAsString(values);
			int num8 = pars[1].EvaluateAsInt(values);
			int num9 = pars[2].EvaluateAsInt(values);
			WorldZoneData worldZoneDataById4 = MainGame.Instance.GameSave.worldData.GetWorldZoneDataById(id15);
			if (worldZoneDataById4 == null)
			{
				return false;
			}
			Vector2 position2 = new Vector2(LazyExpressionEvaluationScope.WgoData.Position.x, LazyExpressionEvaluationScope.WgoData.Position.z);
			Rect rect2 = new Rect(position2, Vector2.Scale(BuildConsts.BUILD_GRID_SIZE_WORLD_UNIT, new Vector2(num8, num9)));
			worldZoneDataById4.AddCustomQualityRect(rect2);
			return true;
		});
		ctx.RegisterFunction("RemoveCustomRectFromWZ", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			string id14 = pars[0].EvaluateAsString(values);
			int num6 = pars[1].EvaluateAsInt(values);
			int num7 = pars[2].EvaluateAsInt(values);
			WorldZoneData worldZoneDataById3 = MainGame.Instance.GameSave.worldData.GetWorldZoneDataById(id14);
			if (worldZoneDataById3 == null)
			{
				return false;
			}
			Vector2 position = new Vector2(LazyExpressionEvaluationScope.WgoData.Position.x, LazyExpressionEvaluationScope.WgoData.Position.z);
			Rect rect = new Rect(position, Vector2.Scale(BuildConsts.BUILD_GRID_SIZE_WORLD_UNIT, new Vector2(num6, num7)));
			return worldZoneDataById3.RemoveCustomQualityRect(rect);
		});
		ctx.RegisterFunction("WZQual", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text14 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text14))
			{
				Debug.LogError("WorldZone id is null or empty");
				return 0f;
			}
			WorldZoneData worldZoneDataById2 = MainGame.Instance.GameSave.worldData.GetWorldZoneDataById(text14);
			if (worldZoneDataById2 == null)
			{
				Debug.LogError("WorldZone by id [" + text14 + "] not found");
				return 0f;
			}
			return worldZoneDataById2.GetTotalQuality();
		});
		ctx.RegisterFunction("AddTownQuality", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			int num5 = pars[0].EvaluateAsInt(values);
			MainGame.Instance.GameSave.townSystem.Quality += num5;
			return true;
		});
		ctx.RegisterFunction("WZQualPowerSource", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WorldZoneData == null) ? ((object)0) : ((object)LazyExpressionEvaluationScope.WorldZoneData.GetTotalQuality(WorldZoneWgoQualityType.ConveyorPowerSource)));
		ctx.RegisterFunction("WZQualConveyorCells", (IExpression[] pars, IDictionary<string, object> values) => (LazyExpressionEvaluationScope.WorldZoneData == null) ? ((object)0) : ((object)LazyExpressionEvaluationScope.WorldZoneData.GetTotalQuality(WorldZoneWgoQualityType.ConveyorCells)));
		ctx.RegisterFunction("WZAddAdditionalQuality", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text13 = pars[0].EvaluateAsString(values);
			int num4 = pars[1].EvaluateAsInt(values);
			if (string.IsNullOrEmpty(text13))
			{
				Debug.LogError("WorldZone id is null or empty");
				return 0f;
			}
			WorldZoneData worldZoneDataById = MainGame.Instance.GameSave.worldData.GetWorldZoneDataById(text13);
			if (worldZoneDataById == null)
			{
				Debug.LogError("WorldZone by id [" + text13 + "] not found");
				return 0f;
			}
			worldZoneDataById.AdditionalQuality += num4;
			return true;
		});
		ctx.RegisterFunction("IsBuildPlaceUnlocked", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text12 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text12))
			{
				return true;
			}
			return MainGame.Instance.GameSave.knowledgeSystem.unlockedBuildings.Contains(text12) ? ((object)true) : ((object)false);
		});
		ctx.RegisterFunction("IsBuildingLocked", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text11 = pars[0].EvaluateAsString(values);
			return !string.IsNullOrEmpty(text11) && MainGame.Instance.GameSave.knowledgeSystem.lockedBuildings.Contains(text11);
		});
		ctx.RegisterFunction("UnlockBuilding", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text10 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text10))
			{
				Debug.LogError("Can't unlock building " + text10 + ", it's value a null or empty");
				return false;
			}
			MainGame.Instance.GameSave.knowledgeSystem.UnlockBuilding(text10);
			return true;
		});
		ctx.RegisterFunction("LockBuilding", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text9 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text9))
			{
				Debug.LogError("Can't lock building " + text9 + ", it's value a null or empty");
				return false;
			}
			MainGame.Instance.GameSave.knowledgeSystem.LockBuilding(text9);
			return true;
		});
		ctx.RegisterFunction("AddToKnownMapZones", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text8 = pars[0].EvaluateAsString(values);
			if (string.IsNullOrEmpty(text8))
			{
				return false;
			}
			if (MainGame.Instance.GameSave.knowledgeSystem.knownMapZones.Contains(text8))
			{
				return false;
			}
			MainGame.Instance.GameSave.knowledgeSystem.knownMapZones.Add(text8);
			return true;
		});
		ctx.RegisterFunction("StartQuest", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id13 = pars[0].EvaluateAsString(values);
			float delayTime6 = 0f;
			if (pars.Length > 1)
			{
				delayTime6 = pars[1].EvaluateAsFloat(values);
			}
			MainGame.Instance.GameSave.questSystemData.StartQuest(id13, delayTime6);
			return true;
		});
		ctx.RegisterFunction("StartQuestDemoDependent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			pars[0].EvaluateAsString(values);
			string id12 = pars[1].EvaluateAsString(values);
			float delayTime5 = 0f;
			if (pars.Length > 2)
			{
				delayTime5 = pars[2].EvaluateAsFloat(values);
			}
			MainGame.Instance.GameSave.questSystemData.StartQuest(id12, delayTime5);
			return true;
		});
		ctx.RegisterFunction("AwaitQuest", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id11 = pars[0].EvaluateAsString(values);
			float delayTime4 = 0f;
			if (pars.Length > 1)
			{
				delayTime4 = pars[1].EvaluateAsFloat(values);
			}
			MainGame.Instance.GameSave.questSystemData.AwaitQuest(id11, delayTime4);
			return true;
		});
		ctx.RegisterFunction("AwaitQuestDemoDependent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			pars[0].EvaluateAsString(values);
			string id10 = pars[1].EvaluateAsString(values);
			float delayTime3 = 0f;
			if (pars.Length > 2)
			{
				delayTime3 = pars[2].EvaluateAsFloat(values);
			}
			MainGame.Instance.GameSave.questSystemData.AwaitQuest(id10, delayTime3);
			return true;
		});
		ctx.RegisterFunction("CompleteQuest", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id9 = pars[0].EvaluateAsString(values);
			float delayTime2 = 0f;
			if (pars.Length > 1)
			{
				delayTime2 = pars[1].EvaluateAsFloat(values);
			}
			MainGame.Instance.GameSave.questSystemData.CompleteQuest(id9, delayTime2);
			return true;
		});
		ctx.RegisterFunction("CompleteQuestDemoDependent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			pars[0].EvaluateAsString(values);
			string id8 = pars[1].EvaluateAsString(values);
			float delayTime = 0f;
			if (pars.Length > 2)
			{
				delayTime = pars[2].EvaluateAsFloat(values);
			}
			MainGame.Instance.GameSave.questSystemData.CompleteQuest(id8, delayTime);
			return true;
		});
		ctx.RegisterFunction("CancelQuest", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id7 = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.questSystemData.CancelQuest(id7);
			return true;
		});
		ctx.RegisterFunction("ChangeQuestHiddenState", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id6 = pars[0].EvaluateAsString(values);
			bool flag2 = pars[1].EvaluateAsBoolean(values);
			MainGame.Instance.GameSave.questSystemData.ChangeQuestHiddenState(id6, !flag2);
			return true;
		});
		ctx.RegisterFunction("ChangeQuestUnknownState", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id5 = pars[0].EvaluateAsString(values);
			bool flag = pars[1].EvaluateAsBoolean(values);
			MainGame.Instance.GameSave.questSystemData.ChangeQuestUnknownState(id5, !flag);
			return true;
		});
		ctx.RegisterFunction("IsQuestCompleted", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id4 = pars[0].EvaluateAsString(values);
			return MainGame.Instance.GameSave.questSystemData.IsQuestInStatus(id4, QuestStatus.Completed);
		});
		ctx.RegisterFunction("ShowTutorialWindow", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			UITutorialWindowData data3 = new UITutorialWindowData(pars[0].EvaluateAsString(values));
			LazyUI.GetWindow<UITutorialWindow>().Open(data3);
			return true;
		});
		ctx.RegisterFunction("ShowTutorialWindowWithCallback", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string pageId = pars[0].EvaluateAsString(values);
			string exprOnClose = pars[1].EvaluateAsString(values);
			UITutorialWindowData data2 = new UITutorialWindowData(pageId)
			{
				OnCompleteCallback = delegate
				{
					new LazyExpression(exprOnClose).Evaluate();
				}
			};
			LazyUI.GetWindow<UITutorialWindow>().Open(data2);
			return true;
		});
		ctx.RegisterFunction("AttachArrowTutorial", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoCustomTag = pars[0].EvaluateAsString(values);
			LazySingleton<UITutorialArrow>.Instance.Attach(wgoCustomTag);
			return true;
		});
		ctx.RegisterFunction("UnAttachArrowTutorial", delegate
		{
			LazySingleton<UITutorialArrow>.Instance.UnAttach();
			return true;
		});
		ctx.RegisterFunction("EnableTutorialMode", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			List<string> list3 = new List<string>();
			for (int k = 0; k < pars.Length; k++)
			{
				string text7 = pars[k].EvaluateAsString(values);
				if (!string.IsNullOrEmpty(text7))
				{
					WgoData wgoDataByCustomTag3 = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(text7);
					if (wgoDataByCustomTag3 != null)
					{
						list3.Add(wgoDataByCustomTag3.UniqueId.Id);
					}
				}
			}
			MainGame.PlayerData.SetTutorialModeState(isActive: true);
			MainGame.PlayerData.AddToTutorialModeExcludedList(list3);
			return true;
		});
		ctx.RegisterFunction("DisableTutorialMode", delegate
		{
			MainGame.PlayerData.SetTutorialModeState(isActive: false);
			return true;
		});
		ctx.RegisterFunction("TutorialModeRemoveFromExclude", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			List<string> list2 = new List<string>();
			for (int j = 0; j < pars.Length; j++)
			{
				string text6 = pars[j].EvaluateAsString(values);
				if (!string.IsNullOrEmpty(text6))
				{
					WgoData wgoDataByCustomTag2 = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(text6);
					if (wgoDataByCustomTag2 != null)
					{
						list2.Add(wgoDataByCustomTag2.UniqueId.Id);
					}
				}
			}
			MainGame.PlayerData.RemoveFromTutorialModeExcludedList(list2);
			return true;
		});
		ctx.RegisterFunction("TutorialModeAddToExclude", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < pars.Length; i++)
			{
				string text5 = pars[i].EvaluateAsString(values);
				if (!string.IsNullOrEmpty(text5))
				{
					WgoData wgoDataByCustomTag = MainGame.Instance.GameSave.worldData.GetWgoDataByCustomTag(text5);
					if (wgoDataByCustomTag != null)
					{
						list.Add(wgoDataByCustomTag.UniqueId.Id);
					}
				}
			}
			MainGame.PlayerData.AddToTutorialModeExcludedList(list);
			return true;
		});
		ctx.RegisterFunction("TrySetOrRemoveResurrectionDebuff", delegate
		{
			int resInt = MainGame.PlayerData.GetResInt("cur_zombies_count");
			int num3 = (int)MainGame.WorldData.GetWorldZoneDataById("resurrection").GetTotalQuality();
			if (resInt <= num3)
			{
				MainGame.Instance.GameSave.perkSystemData.RemovePerk("debuff_excessive_zombie");
			}
			else
			{
				MainGame.Instance.GameSave.perkSystemData.AddPerk("debuff_excessive_zombie");
			}
			return true;
		});
		ctx.RegisterFunction("SetWeatherState", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string stateName = pars[0].EvaluateAsString(values);
			WeatherSystem.Instance.SetWeatherState(stateName, force: true);
			return true;
		});
		ctx.RegisterFunction("ResetWeatherState", delegate
		{
			WeatherSystem.Instance.ResetWeatherState();
			return true;
		});
		ctx.RegisterFunction("SetWeatherComponent", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string componentName = pars[0].EvaluateAsString(values);
			bool isActive = pars[1].EvaluateAsBoolean(values);
			WeatherSystem.Instance.SetWeatherComponent(componentName, isActive);
			return true;
		});
		ctx.RegisterFunction("SetPostProcessProfile", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (pars.Length < 1)
			{
				return false;
			}
			string postProcessProfile = pars[0].EvaluateAsString(values);
			CameraSystem.Instance.MainCamera.SetPostProcessProfile(postProcessProfile);
			return true;
		});
		ctx.RegisterFunction("ResetPostProcessProfile", delegate
		{
			CameraSystem.Instance.MainCamera.ResetPostProcessProfile();
			return true;
		});
		ctx.RegisterFunction("SetLightEnvironmentOverride", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (pars.Length < 1)
			{
				return false;
			}
			string presetName = pars[0].EvaluateAsString(values);
			EnvironmentEngine.Instance.ApplyOverridePreset(presetName);
			return true;
		});
		ctx.RegisterFunction("ResetLightEnvironmentOverride", delegate
		{
			EnvironmentEngine.Instance.ApplyOverridePreset((LightEnvironmentPreset)null, 0f);
			return true;
		});
		ctx.RegisterFunction("CloseUIWindow", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			IUIWindowCustomOperable windowByInterface2 = LazyUI.GetWindowByInterface<IUIWindowCustomOperable>(pars[0].EvaluateAsString(values));
			if (windowByInterface2 == null)
			{
				return false;
			}
			windowByInterface2.Close();
			return true;
		});
		ctx.RegisterFunction("IsShownUIWindow", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			IUIWindowCustomOperable windowByInterface = LazyUI.GetWindowByInterface<IUIWindowCustomOperable>(pars[0].EvaluateAsString(values));
			return (windowByInterface == null) ? ((object)false) : ((object)windowByInterface.IsShown);
		});
		ctx.RegisterFunction("ShowTextNotification", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string locale = pars[0].EvaluateAsString(values);
			LazySingleton<UINotificator>.Instance.ShowSimpleTextNotification(locale);
			return true;
		});
		ctx.RegisterFunction("ResetVendors", delegate
		{
			MainGame.Instance.GameSave.townSystem.ResetVendors();
			return true;
		});
		ctx.RegisterFunction("ClearTownPalettes", delegate
		{
			MainGame.Instance.GameSave.townSystem.ClearTownPalettes();
			return true;
		});
		ctx.RegisterFunction("AddOrderSlot", delegate
		{
			MainGame.Instance.GameSave.vendorSystem.currentOrders.Add(SGuid.Empty);
			return true;
		});
		ctx.RegisterFunction("RepairTownCluster", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			LazyExpressionEvaluationScope.WgoData.TownClusterRepairWgoComponent.DoRepairLogic();
			return true;
		});
		ctx.RegisterFunction("EnableGDPointById", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string gdPointId2 = pars[0].EvaluateAsString(values);
			foreach (GDPointData item30 in MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointsDataById(gdPointId2))
			{
				item30.Enabled = true;
			}
			return true;
		});
		ctx.RegisterFunction("EnableGDPointByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string customTag2 = pars[0].EvaluateAsString(values);
			foreach (GDPointData item31 in MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointsDataByCustomTag(customTag2))
			{
				item31.Enabled = true;
			}
			return true;
		});
		ctx.RegisterFunction("DisableGDPointById", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string gdPointId = pars[0].EvaluateAsString(values);
			foreach (GDPointData item32 in MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointsDataById(gdPointId))
			{
				item32.Enabled = false;
			}
			return true;
		});
		ctx.RegisterFunction("DisableGDPointByTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string customTag = pars[0].EvaluateAsString(values);
			foreach (GDPointData item33 in MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointsDataByCustomTag(customTag))
			{
				item33.Enabled = false;
			}
			return true;
		});
		ctx.RegisterFunction("GetWeapon", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return new ItemDef();
			}
			return (LazyExpressionEvaluationScope.WgoData is ZombieWgoData zombieWgoData2) ? zombieWgoData2.WorkerToolInventory.GetItemByTypes(new ItemType[3]
			{
				ItemType.Bow,
				ItemType.Pike,
				ItemType.Sword
			}) : LazyExpressionEvaluationScope.WgoData.Inventory.GetItemByTypes(new ItemType[3]
			{
				ItemType.Bow,
				ItemType.Pike,
				ItemType.Sword
			});
		});
		ctx.RegisterFunction("GetArmor", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return new ItemDef();
			}
			return (LazyExpressionEvaluationScope.WgoData is ZombieWgoData zombieWgoData) ? zombieWgoData.WorkerToolInventory.GetItemByType(ItemType.BodyArmor) : LazyExpressionEvaluationScope.WgoData.Inventory.GetItemByType(ItemType.BodyArmor);
		});
		ctx.RegisterFunction("Item_Dmg", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item item8 = pars[0].Evaluate(values) as Item;
			if (item8 == null || item8.IsEmpty)
			{
				item8 = LazyExpressionEvaluationScope.Item;
			}
			return (item8 != null && !item8.IsEmpty) ? ((object)item8.Definition.damage.EvaluateInt(LazyExpressionEvaluationScope.CombatEntity)) : ((object)0);
		});
		ctx.RegisterFunction("Item_Range", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item item7 = pars[0].Evaluate(values) as Item;
			if (item7 == null || item7.IsEmpty)
			{
				item7 = LazyExpressionEvaluationScope.Item;
			}
			return (item7 != null && !item7.IsEmpty) ? ((object)item7.Definition.atkRange) : ((object)0f);
		});
		ctx.RegisterFunction("Item_AtkPause", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item item6 = pars[0].Evaluate(values) as Item;
			if (item6 == null || item6.IsEmpty)
			{
				item6 = LazyExpressionEvaluationScope.Item;
			}
			return (item6 != null && !item6.IsEmpty) ? ((object)item6.Definition.atkPause) : ((object)1f);
		});
		ctx.RegisterFunction("Item_Armor", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item item5 = pars[0].Evaluate(values) as Item;
			if (item5 == null || item5.IsEmpty)
			{
				item5 = LazyExpressionEvaluationScope.Item;
			}
			return (item5 != null && !item5.IsEmpty) ? ((object)item5.Definition.quality) : ((object)0);
		});
		ctx.RegisterFunction("Item_KnForce", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item item4 = pars[0].Evaluate(values) as Item;
			if (item4 == null || item4.IsEmpty)
			{
				item4 = LazyExpressionEvaluationScope.Item;
			}
			return (item4 != null && !item4.IsEmpty) ? ((object)item4.Definition.knockbackForce) : ((object)0);
		});
		ctx.RegisterFunction("Item_DPTag", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			Item item3 = pars[0].Evaluate(values) as Item;
			if (item3 == null || item3.IsEmpty)
			{
				item3 = LazyExpressionEvaluationScope.Item;
			}
			return (item3 != null && !item3.IsEmpty) ? ((object)item3.Definition.dockPointTag) : ((object)DockPointTag.None);
		});
		ctx.RegisterFunction("CollectedCount", delegate
		{
			Item item2 = LazyExpressionEvaluationScope.Item;
			return (item2 == null || item2.IsEmpty) ? ((object)0) : ((object)item2.Count);
		});
		ctx.RegisterFunction("Spawn_FightingLvl", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id3 = pars[0].EvaluateAsString(values);
			string text4 = ((pars.Length > 1) ? pars[1].EvaluateAsString(values) : "RuinedTemple");
			GameSceneData gameSceneDataById3 = MainGame.WorldData.GetGameSceneDataById(text4);
			if (gameSceneDataById3 == null)
			{
				Debug.LogError("Scene with name [" + text4 + "] not found");
				return false;
			}
			gameSceneDataById3.AddFightingLevelData(id3);
			return true;
		});
		ctx.RegisterFunction("Despawn_FightingLvl", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id2 = pars[0].EvaluateAsString(values);
			string text3 = ((pars.Length > 1) ? pars[1].EvaluateAsString(values) : "RuinedTemple");
			GameSceneData gameSceneDataById2 = MainGame.WorldData.GetGameSceneDataById(text3);
			if (gameSceneDataById2 == null)
			{
				Debug.LogError("Scene with name [" + text3 + "] not found");
				return false;
			}
			gameSceneDataById2.RemoveFightingLevelData(id2);
			return true;
		});
		ctx.RegisterFunction("SetStage_FightingLvl", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string id = pars[0].EvaluateAsString(values);
			int stageId = pars[1].EvaluateAsInt(values);
			string text2 = ((pars.Length > 2) ? pars[2].EvaluateAsString(values) : "RuinedTemple");
			GameSceneData gameSceneDataById = MainGame.WorldData.GetGameSceneDataById(text2);
			if (gameSceneDataById == null)
			{
				Debug.LogError("Scene with name [" + text2 + "] not found");
				return false;
			}
			gameSceneDataById.ApplyStageForFightingLevel(id, stageId);
			return true;
		});
		ctx.RegisterFunction("Replace_To_Broken_Barricade", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			int rotationIndex = LazyExpressionEvaluationScope.WgoData.MainWgoPartData.rotationIndex;
			WgoData wgoData = new WgoData(LazyExpressionEvaluationScope.WgoData.id + "_broken", LazyExpressionEvaluationScope.WgoData.Position, LazyExpressionEvaluationScope.WgoData.WorldId);
			MainGame.Instance.GameSave.worldData.AddWgoData(wgoData);
			wgoData.ApplyWgoPartState(wgoData.MainWgoPartData.variationId, rotationIndex);
			LazySingleton<FightingGameController>.Instance.AddTemporaryWgoData(wgoData);
			return true;
		});
		ctx.RegisterFunction("SetMercenariesPaymentId", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string mercenariesPaymentId = pars[0].EvaluateAsString(values);
			MainGame.Instance.GameSave.militaryBaseData.MercenariesPaymentId = mercenariesPaymentId;
			return true;
		});
		ctx.RegisterFunction("IsFightActive", (IExpression[] pars, IDictionary<string, object> values) => LazySingleton<FightingGameController>.Instance.CurrentFightState != FightState.Disabled);
		ctx.RegisterFunction("AddWorkerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			IWorker worker7 = LazyExpressionEvaluationScope.WgoData.Worker;
			if (worker7 == null || (worker7 is UnityEngine.Object object8 && object8 == null))
			{
				return false;
			}
			string type6 = pars[0].EvaluateAsString(values);
			float value3 = pars[1].EvaluateAsFloat(values);
			worker7.AddRes(type6, value3);
			return true;
		});
		ctx.RegisterFunction("DecWorkerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			IWorker worker6 = LazyExpressionEvaluationScope.WgoData.Worker;
			if (worker6 == null || (worker6 is UnityEngine.Object object7 && object7 == null))
			{
				return false;
			}
			string type5 = pars[0].EvaluateAsString(values);
			float num2 = pars[1].EvaluateAsFloat(values);
			worker6.AddRes(type5, 0f - num2);
			return true;
		});
		ctx.RegisterFunction("MultiplyWorkerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			IWorker worker5 = LazyExpressionEvaluationScope.WgoData.Worker;
			if (worker5 == null || (worker5 is UnityEngine.Object object6 && object6 == null))
			{
				return false;
			}
			string type4 = pars[0].EvaluateAsString(values);
			float value2 = pars[1].EvaluateAsFloat(values);
			worker5.MultiplyRes(type4, value2);
			return true;
		});
		ctx.RegisterFunction("DivideWorkerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			IWorker worker4 = LazyExpressionEvaluationScope.WgoData.Worker;
			if (worker4 == null || (worker4 is UnityEngine.Object object5 && object5 == null))
			{
				return false;
			}
			string type3 = pars[0].EvaluateAsString(values);
			float num = pars[1].EvaluateAsFloat(values);
			worker4.MultiplyRes(type3, 1f / num);
			return true;
		});
		ctx.RegisterFunction("SetWorkerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return false;
			}
			IWorker worker3 = LazyExpressionEvaluationScope.WgoData.Worker;
			if (worker3 == null || (worker3 is UnityEngine.Object object4 && object4 == null))
			{
				return false;
			}
			string type2 = pars[0].EvaluateAsString(values);
			float value = pars[1].EvaluateAsFloat(values);
			worker3.SetRes(type2, value);
			return true;
		});
		ctx.RegisterFunction("WorkerPar", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			IWorker worker2 = LazyExpressionEvaluationScope.WgoData.Worker;
			if (worker2 == null || (worker2 is UnityEngine.Object object3 && object3 == null))
			{
				return 0;
			}
			string type = pars[0].EvaluateAsString(values);
			return worker2.GetRes(type);
		});
		ctx.RegisterFunction("WorkerIsPlayer", delegate
		{
			if (LazyExpressionEvaluationScope.WgoData == null)
			{
				return 0;
			}
			IWorker worker = LazyExpressionEvaluationScope.WgoData.Worker;
			return (worker == null || (worker is UnityEngine.Object object2 && object2 == null)) ? ((object)0) : ((object)((worker is PlayerController) ? 1 : 0));
		});
		ctx.RegisterFunction("NPCSim_AddWgoToGroupFromBalance", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId3 = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.AddWgoToGroupFromBalance(wgoId3);
			return true;
		});
		ctx.RegisterFunction("NPCSim_AddWgoToGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId2 = pars[0].EvaluateAsString(values);
			string groupId4 = pars[1].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.AddWgoToGroup(wgoId2, groupId4);
			return true;
		});
		ctx.RegisterFunction("NPCSim_RemoveWgoFromGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string wgoId = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.RemoveWgoFromGroup(wgoId);
			return true;
		});
		ctx.RegisterFunction("NPCSim_UnlockPointOfInterest", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string pointId2 = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.UnlockPointOfInterest(pointId2);
			return true;
		});
		ctx.RegisterFunction("NPCSim_LockPointOfInterest", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string pointId = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.LockPointOfInterest(pointId);
			return true;
		});
		ctx.RegisterFunction("NPCSim_SendGroupHome", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string groupId3 = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.SendGroupHome(groupId3);
			return true;
		});
		ctx.RegisterFunction("NPCSim_SendAllGroupsHome", delegate
		{
			MainGame.Instance.npcLifeSimulator.SendAllGroupsHome();
			return true;
		});
		ctx.RegisterFunction("NPCSim_RollActivityForGroup", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string groupId2 = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.RollActivityForGroup(groupId2);
			return true;
		});
		ctx.RegisterFunction("NPCSim_RollActivityForAllGroups", delegate
		{
			MainGame.Instance.npcLifeSimulator.RollActivityForAllGroups();
			return true;
		});
		ctx.RegisterFunction("NPCSim_ForceAllGroupsTeleportHome", delegate
		{
			MainGame.Instance.npcLifeSimulator.ForceAllGroupsTeleportHome();
			return true;
		});
		ctx.RegisterFunction("NPCSim_ForceGroupTeleportHome", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string groupId = pars[0].EvaluateAsString(values);
			MainGame.Instance.npcLifeSimulator.ForceGroupTeleportHome(groupId);
			return true;
		});
		ctx.RegisterFunction("ScanGraph", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (Enum.TryParse<LazyConsts.Navigation.Graph>(pars[0].EvaluateAsString(values), out var result2) && AstarPath.active.graphs[(int)result2] is RecastGraph recastGraph)
			{
				recastGraph.Scan();
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("Debug_ScanGraphWithDelay", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			if (Enum.TryParse<LazyConsts.Navigation.Graph>(pars[0].EvaluateAsString(values), out var result) && AstarPath.active.graphs[(int)result] is RecastGraph @object)
			{
				LazyTimer.AddTimer(1f, @object.Scan);
				return true;
			}
			return false;
		});
		ctx.RegisterFunction("SpawnFxOnGDPoint", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string text = pars[0].EvaluateAsString(values);
			string name = pars[1].EvaluateAsString(values);
			GDPointData gDPointDataById = MainGame.Instance.GameSave.WorldData.gdPointsData.GetGDPointDataById(text);
			if (gDPointDataById == null)
			{
				Debug.LogError("No gd point:[" + text + "] can't spawn fx");
				return false;
			}
			WorldFX.Spawn(gDPointDataById.Position, name);
			return true;
		});
		ctx.RegisterFunction("AchUnlock", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string achievementId = pars[0].EvaluateAsString(values);
			AchievementsSystem.Instance.Unlock(achievementId);
			return true;
		});
		ctx.RegisterFunction("AchTriggerCountable", delegate(IExpression[] pars, IDictionary<string, object> values)
		{
			string trigger = pars[0].EvaluateAsString(values);
			int countProgress = pars[1].EvaluateAsInt(values);
			AchievementsSystem.Instance.TriggerCountable(trigger, countProgress);
			return true;
		});
	}

	private static void OpenAutopsyWindow(WgoData wgoData, Action<UIAutopsyWindowData> onClose = null)
	{
		UIAutopsyWindow window = LazyUI.GetWindow<UIAutopsyWindow>();
		UIAutopsyWindowData data = new UIAutopsyWindowData(wgoData);
		window.Open(data, onClose);
	}
}
