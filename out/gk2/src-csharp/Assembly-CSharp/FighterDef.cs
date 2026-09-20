using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class FighterDef : BalanceBaseObject
{
	[AutoParse("hp")]
	public LazyExpression hp;

	[AutoParse("atk_damage")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression atkDamage = new LazyExpression();

	[AutoParse("atk_range")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression atkRange = new LazyExpression();

	[AutoParse("atk_pause")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression atkPause = new LazyExpression();

	[AutoParse("armor")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression armor = new LazyExpression();

	[AutoParse("ret_dmg")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression retDamage = new LazyExpression();

	[AutoParse("knockback_force")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression knockbackForce = new LazyExpression();

	[AutoParse("movement_speed")]
	public float mvtSpeed;

	[AutoParse("movement_acc")]
	public float mvtAcceleration;

	[AutoParse("kill_xp")]
	public int killXp;

	[AutoParse("target_filter_dock_point_tag")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression targetFilterDockPointTag = new LazyExpression();

	[AutoParse("expressions_on_combat_death")]
	public List<LazyExpression> expressionsOnCombatDeath = new List<LazyExpression>();

	public bool HasKnockback => knockbackForce.HasExpression;

	public DockPointTag TargetFilterDockPointTag(WgoData wgoData)
	{
		Item item = null;
		if (wgoData is ZombieWgoData { Hand: var hand } && !hand.IsEmpty && (hand.Definition.type == ItemType.Bow || hand.Definition.type == ItemType.Pike))
		{
			return (DockPointTag)targetFilterDockPointTag.EvaluateInt(hand);
		}
		item = wgoData.Inventory.GetItemByType(ItemType.Bow);
		if (item == null || item.IsEmpty)
		{
			item = wgoData.Inventory.GetItemByType(ItemType.Pike);
		}
		if (item != null && !item.IsEmpty)
		{
			return (DockPointTag)targetFilterDockPointTag.EvaluateInt(item);
		}
		return DockPointTag.None;
	}
}
