using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Category("Movement/Pathfinding")]
[Description("Makes the agent wander randomly within the navigation map")]
public class Wander : ActionTask<NavMeshAgent>
{
	public BBParameter<float> speed = 4f;

	public BBParameter<float> keepDistance = 0.1f;

	public BBParameter<float> minWanderDistance = 5f;

	public BBParameter<float> maxWanderDistance = 20f;

	public bool repeat = true;

	protected override void OnExecute()
	{
		base.agent.speed = speed.value;
		DoWander();
	}

	protected override void OnUpdate()
	{
		if (!base.agent.pathPending && base.agent.remainingDistance <= base.agent.stoppingDistance + keepDistance.value)
		{
			if (repeat)
			{
				DoWander();
			}
			else
			{
				EndAction();
			}
		}
	}

	private void DoWander()
	{
		float value = minWanderDistance.value;
		float value2 = maxWanderDistance.value;
		value = Mathf.Clamp(value, 0.01f, value2);
		value2 = Mathf.Clamp(value2, value, value2);
		Vector3 vector = base.agent.transform.position;
		while ((vector - base.agent.transform.position).sqrMagnitude < value)
		{
			vector = Random.insideUnitSphere * value2 + base.agent.transform.position;
		}
		if (!base.agent.SetDestination(vector))
		{
			DoWander();
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
