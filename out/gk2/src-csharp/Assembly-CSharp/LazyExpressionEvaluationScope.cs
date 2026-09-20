using System;

public static class LazyExpressionEvaluationScope
{
	public static LazyExpressionContext Context;

	public static Action CustomCallback;

	public static Item OverheadCandidate;

	public static WgoData WgoData
	{
		get
		{
			return Context.WgoData;
		}
		set
		{
			Context.WgoData = value;
		}
	}

	public static Item Item
	{
		get
		{
			return Context.Item;
		}
		set
		{
			Context.Item = value;
		}
	}

	public static WorldZoneData WorldZoneData
	{
		get
		{
			return Context.WorldZoneData;
		}
		set
		{
			Context.WorldZoneData = value;
		}
	}

	public static ICombatEntity CombatEntity
	{
		get
		{
			return Context.CombatEntity;
		}
		set
		{
			Context.CombatEntity = value;
		}
	}

	public static ICombatEntity CombatEntityAttacksMe
	{
		get
		{
			return Context.CombatEntityAttacksMe;
		}
		set
		{
			Context.CombatEntityAttacksMe = value;
		}
	}

	public static int DeltaValue
	{
		get
		{
			return Context.DeltaValue;
		}
		set
		{
			Context.DeltaValue = value;
		}
	}

	public static void Begin(LazyExpressionContext context, Action customCallback = null)
	{
		Context = context;
		CustomCallback = customCallback;
	}

	public static void End()
	{
		Context = default(LazyExpressionContext);
		CustomCallback = null;
	}

	public static bool AnyOverheadItem(Predicate<Item> match)
	{
		if (match == null)
		{
			return false;
		}
		if (OverheadCandidate != null && !OverheadCandidate.IsEmpty)
		{
			return match(OverheadCandidate);
		}
		Item item;
		return MainGame.PlayerData?.TryGetOverheadItem(match, out item) ?? false;
	}

	public static Item GetOverheadItemForMutation()
	{
		if (OverheadCandidate != null && !OverheadCandidate.IsEmpty)
		{
			return OverheadCandidate;
		}
		return MainGame.PlayerData?.overheadItem;
	}
}
