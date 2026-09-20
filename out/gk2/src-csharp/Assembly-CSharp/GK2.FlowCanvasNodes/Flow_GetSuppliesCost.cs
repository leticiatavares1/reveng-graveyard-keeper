using System;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Supplies Cost", 0)]
[Category("Game")]
public class Flow_GetSuppliesCost : GKCustomFlowNode
{
	public enum SuppliesType
	{
		SpecialWood,
		Steel
	}

	public enum SuppliesCount
	{
		X1,
		X10,
		X50,
		X100
	}

	[GatherPortsCallback]
	public SuppliesType suppliesType;

	[GatherPortsCallback]
	public SuppliesCount suppliesCount;

	private ValueOutput<SmartRes> smartRes;

	public override string name => "Get " + suppliesType.ToString() + " " + suppliesCount.ToString() + " Supplies Cost";

	protected override void RegisterPorts()
	{
		smartRes = AddValueOutput("smartRes".CapitalizeFirst(), delegate
		{
			SmartRes smartRes = new SmartRes
			{
				gameRes = new GameRes()
			};
			int value = 0;
			if (suppliesType == SuppliesType.SpecialWood)
			{
				value = suppliesCount switch
				{
					SuppliesCount.X1 => ConstDef.Get("special_wood_cost_x1").IntValue, 
					SuppliesCount.X10 => ConstDef.Get("special_wood_cost_x10").IntValue, 
					SuppliesCount.X50 => ConstDef.Get("special_wood_cost_x50").IntValue, 
					SuppliesCount.X100 => ConstDef.Get("special_wood_cost_x100").IntValue, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
			else if (suppliesType == SuppliesType.Steel)
			{
				value = suppliesCount switch
				{
					SuppliesCount.X1 => ConstDef.Get("steel_cost_x1").IntValue, 
					SuppliesCount.X10 => ConstDef.Get("steel_cost_x10").IntValue, 
					SuppliesCount.X50 => ConstDef.Get("steel_cost_x50").IntValue, 
					SuppliesCount.X100 => ConstDef.Get("steel_cost_x100").IntValue, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
			smartRes.gameRes.Add(new GameResAtom("money", value));
			return smartRes;
		});
	}
}
