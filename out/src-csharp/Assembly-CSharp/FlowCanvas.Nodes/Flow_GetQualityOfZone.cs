using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Icon("CubeArrow", false, "")]
[Name("Get Quality Of Zone", 0)]
[FlowNode.ContextDefinedOutputs(new Type[] { typeof(float) })]
[Category("Game Functions")]
public class Flow_GetQualityOfZone : PureFunctionNode<float, string>
{
	public override float Invoke(string zone_id)
	{
		WorldZone zoneByID = WorldZone.GetZoneByID(zone_id);
		if (zoneByID == null)
		{
			return 0f;
		}
		return zoneByID.GetTotalQuality();
	}
}
