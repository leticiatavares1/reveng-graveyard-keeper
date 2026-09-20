using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.StateMachines;

[Name("Any State", 0)]
[Description("The Transitions of this node will constantly be checked. If any becomes true, the target connected State will Enter regardless of the current State. This node can have no incomming transitions.")]
[Color("b3ff7f")]
public class AnyState : FSMState, IUpdatable
{
	public bool dontRetriggerStates;

	public override string name => "FROM ANY STATE";

	public override int maxInConnections => 0;

	public override int maxOutConnections => -1;

	public override bool allowAsPrime => false;

	public new void Update()
	{
		if (base.outConnections.Count == 0)
		{
			return;
		}
		base.status = Status.Running;
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			FSMConnection fSMConnection = (FSMConnection)base.outConnections[i];
			ConditionTask condition = fSMConnection.condition;
			if (fSMConnection.isActive && condition != null && (!dontRetriggerStates || base.FSM.currentState != (FSMState)fSMConnection.targetNode || base.FSM.currentState.status != Status.Running))
			{
				if (condition.CheckCondition(base.graphAgent, base.graphBlackboard))
				{
					base.FSM.EnterState((FSMState)fSMConnection.targetNode);
					fSMConnection.status = Status.Success;
					break;
				}
				fSMConnection.status = Status.Failure;
			}
		}
	}
}
