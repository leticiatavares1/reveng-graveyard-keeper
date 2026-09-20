using System.Collections.Generic;
using System.Linq;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Functions")]
[Name("Find Nearest Dock Point From WGO", 0)]
[Color("eed9a7")]
public class Flow_NearestDockPoint : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> target_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("Dock Point", delegate
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(target_wgo);
			if (worldGameObject == null)
			{
				Debug.Log(" WGO is null!");
				return (Transform)null;
			}
			List<DockPoint> list = new List<DockPoint>();
			list = worldGameObject.GetComponentsInChildren<DockPoint>().ToList();
			if (list.Count == 0)
			{
				Debug.Log("No Dock Points in WGO!");
				return (Transform)null;
			}
			DockPoint dockPoint = null;
			float num = float.PositiveInfinity;
			foreach (DockPoint item in list)
			{
				float num2 = Vector2.Distance(item.transform.position, MainGame.me.player.transform.position);
				if (num2 < num)
				{
					num = num2;
					dockPoint = item;
				}
			}
			return dockPoint.transform;
		});
	}
}
