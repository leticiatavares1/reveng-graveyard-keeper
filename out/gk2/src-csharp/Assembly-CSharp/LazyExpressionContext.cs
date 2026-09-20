using UnityEngine;

public struct LazyExpressionContext
{
	public WgoData WgoData;

	public Item Item;

	public WorldZoneData WorldZoneData;

	public ICombatEntity CombatEntity;

	public ICombatEntity CombatEntityAttacksMe;

	public int DeltaValue;

	public static LazyExpressionContext Empty => default(LazyExpressionContext);

	public static LazyExpressionContext From(WgoData wgoData)
	{
		LazyExpressionContext result = default(LazyExpressionContext);
		result.WgoData = wgoData;
		return result;
	}

	public static LazyExpressionContext From(Item item)
	{
		LazyExpressionContext result = default(LazyExpressionContext);
		result.Item = item;
		return result;
	}

	public static LazyExpressionContext From(WorldZoneData worldZoneData)
	{
		LazyExpressionContext result = default(LazyExpressionContext);
		result.WorldZoneData = worldZoneData;
		return result;
	}

	public static LazyExpressionContext From(ICombatEntity combatEntity)
	{
		LazyExpressionContext lazyExpressionContext = default(LazyExpressionContext);
		lazyExpressionContext.CombatEntity = combatEntity;
		LazyExpressionContext result = lazyExpressionContext;
		if (combatEntity is Object @object && @object is Wgo wgo && (bool)wgo)
		{
			result.WgoData = wgo.Data;
		}
		return result;
	}

	public static LazyExpressionContext From(ICraftable craftable)
	{
		LazyExpressionContext result = default(LazyExpressionContext);
		result.WgoData = craftable as WgoData;
		return result;
	}

	public static LazyExpressionContext WithAttackerAttacksMe(ICombatEntity attacker)
	{
		LazyExpressionContext result = default(LazyExpressionContext);
		result.CombatEntityAttacksMe = attacker;
		return result;
	}

	public static LazyExpressionContext ValueDelta(int valueDelta)
	{
		LazyExpressionContext result = default(LazyExpressionContext);
		result.DeltaValue = valueDelta;
		return result;
	}
}
