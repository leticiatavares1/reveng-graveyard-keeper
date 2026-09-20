using System.Collections;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Wait until the physics update frame")]
[Category("Time")]
public class WaitForPhysicsFrame : LatentActionNode
{
	public override bool exposeRoutineControls => false;

	public override IEnumerator Invoke()
	{
		yield return new WaitForFixedUpdate();
	}
}
