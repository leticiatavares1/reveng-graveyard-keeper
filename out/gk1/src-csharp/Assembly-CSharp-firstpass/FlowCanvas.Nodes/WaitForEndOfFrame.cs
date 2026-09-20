using System.Collections;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Wait until the end of current frame")]
[Category("Time")]
public class WaitForEndOfFrame : LatentActionNode
{
	public override bool exposeRoutineControls => false;

	public override IEnumerator Invoke()
	{
		yield return new UnityEngine.WaitForEndOfFrame();
	}
}
