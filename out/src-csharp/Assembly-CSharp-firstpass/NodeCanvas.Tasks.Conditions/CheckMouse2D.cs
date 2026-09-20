using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("System Events")]
[Name("Check Mouse 2D", 0)]
[EventReceiver(new string[] { "OnMouseEnter", "OnMouseExit", "OnMouseOver" })]
public class CheckMouse2D : ConditionTask<Collider2D>
{
	public MouseInteractionTypes checkType;

	protected override string info => checkType.ToString();

	protected override bool OnCheck()
	{
		return false;
	}

	public void OnMouseEnter()
	{
		if (checkType == MouseInteractionTypes.MouseEnter)
		{
			YieldReturn(value: true);
		}
	}

	public void OnMouseExit()
	{
		if (checkType == MouseInteractionTypes.MouseExit)
		{
			YieldReturn(value: true);
		}
	}

	public void OnMouseOver()
	{
		if (checkType == MouseInteractionTypes.MouseOver)
		{
			YieldReturn(value: true);
		}
	}
}
