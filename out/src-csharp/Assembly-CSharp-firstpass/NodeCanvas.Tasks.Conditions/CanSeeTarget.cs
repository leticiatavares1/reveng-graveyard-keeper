using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Description("A combination of line of sight and view angle check")]
[Category("GameObject")]
public class CanSeeTarget : ConditionTask<Transform>
{
	[RequiredField]
	public BBParameter<GameObject> target;

	public BBParameter<float> maxDistance = 50f;

	public BBParameter<float> awarnessDistance = 0f;

	[SliderField(1, 180)]
	public BBParameter<float> viewAngle = 70f;

	public Vector3 offset;

	private RaycastHit hit;

	protected override string info => "Can See " + target;

	protected override bool OnCheck()
	{
		Transform transform = target.value.transform;
		if (Vector3.Distance(base.agent.position, transform.position) > maxDistance.value)
		{
			return false;
		}
		if (Physics.Linecast(base.agent.position + offset, transform.position + offset, out hit) && hit.collider != transform.GetComponent<Collider>())
		{
			return false;
		}
		if (Vector3.Angle(transform.position - base.agent.position, base.agent.forward) < viewAngle.value)
		{
			return true;
		}
		if (Vector3.Distance(base.agent.position, transform.position) < awarnessDistance.value)
		{
			return true;
		}
		return false;
	}

	public override void OnDrawGizmosSelected()
	{
		if (base.agent != null)
		{
			Gizmos.DrawLine(base.agent.position, base.agent.position + offset);
			Gizmos.DrawLine(base.agent.position + offset, base.agent.position + offset + base.agent.forward * maxDistance.value);
			Gizmos.DrawWireSphere(base.agent.position + offset + base.agent.forward * maxDistance.value, 0.1f);
			Gizmos.DrawWireSphere(base.agent.position, awarnessDistance.value);
			Gizmos.matrix = Matrix4x4.TRS(base.agent.position + offset, base.agent.rotation, Vector3.one);
			Gizmos.DrawFrustum(Vector3.zero, viewAngle.value, 5f, 0f, 1f);
		}
	}
}
