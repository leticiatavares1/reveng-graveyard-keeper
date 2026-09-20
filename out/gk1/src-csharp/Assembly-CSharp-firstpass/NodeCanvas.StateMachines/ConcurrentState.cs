using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Name("Concurrent", 0)]
[Color("ff64cb")]
[Description("Execute a number of Actions with optional conditional requirement and in parallel to any other state, as soon as the FSM is started.\nAll actions will prematurely be stoped as soon as the FSM stops as well.\nThis is not a state per-se and thus can have neither incomming, nor outgoing transitions.")]
public class ConcurrentState : FSMState, IUpdatable, ISubTasksContainer
{
	[SerializeField]
	private ConditionList _conditionList;

	[SerializeField]
	private ActionList _actionList;

	[SerializeField]
	private bool _repeatStateActions;

	private bool accessed;

	public ConditionList conditionList
	{
		get
		{
			return _conditionList;
		}
		set
		{
			_conditionList = value;
		}
	}

	public ActionList actionList
	{
		get
		{
			return _actionList;
		}
		set
		{
			_actionList = value;
		}
	}

	public bool repeatStateActions
	{
		get
		{
			return _repeatStateActions;
		}
		set
		{
			_repeatStateActions = value;
		}
	}

	public override string name => base.name.ToUpper();

	public override int maxInConnections => 0;

	public override int maxOutConnections => 0;

	public override bool allowAsPrime => false;

	public Task[] GetSubTasks()
	{
		return new Task[2] { _conditionList, _actionList };
	}

	public override void OnValidate(Graph assignedGraph)
	{
		if (conditionList == null)
		{
			conditionList = (ConditionList)Task.Create(typeof(ConditionList), assignedGraph);
			conditionList.checkMode = ConditionList.ConditionsCheckMode.AllTrueRequired;
		}
		if (actionList == null)
		{
			actionList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
			actionList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
		}
	}

	protected override void OnEnter()
	{
		accessed = false;
		Update();
	}

	public new void Update()
	{
		if (base.status != Status.Resting && base.status != Status.Running)
		{
			return;
		}
		if (conditionList.CheckCondition(base.graphAgent, base.graphBlackboard))
		{
			accessed = true;
		}
		if (accessed && actionList.ExecuteAction(base.graphAgent, base.graphBlackboard) != Status.Running)
		{
			accessed = false;
			if (!repeatStateActions)
			{
				Finish();
			}
		}
	}

	protected override void OnExit()
	{
		actionList.EndAction(null);
	}

	protected override void OnPause()
	{
		actionList.PauseAction();
	}
}
