using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Category("Movement/Pathfinding")]
[Description("Flees away from the target")]
public class Flee : ActionTask<NavMeshAgent>
{
	[RequiredField]
	public BBParameter<GameObject> target;

	public BBParameter<float> speed = 4f;

	public BBParameter<float> fledDistance = 10f;

	public BBParameter<float> lookAhead = 2f;

	protected override string info => $"Flee from {target}";

	protected override void OnExecute()
	{
		base.agent.speed = speed.value;
		if ((base.agent.transform.position - target.value.transform.position).magnitude >= fledDistance.value)
		{
			EndAction(success: true);
		}
	}

	protected override void OnUpdate()
	{
		Vector3 position = target.value.transform.position;
		if ((base.agent.transform.position - position).magnitude >= fledDistance.value)
		{
			EndAction(success: true);
			return;
		}
		Vector3 destination = position + (base.agent.transform.position - position).normalized * (fledDistance.value + lookAhead.value + base.agent.stoppingDistance);
		if (!base.agent.SetDestination(destination))
		{
			EndAction(success: false);
		}
	}

	protected override void OnPause()
	{
		OnStop();
	}

	protected override void OnStop()
	{
		if (base.agent.gameObject.activeSelf)
		{
			base.agent.ResetPath();
		}
	}
}
