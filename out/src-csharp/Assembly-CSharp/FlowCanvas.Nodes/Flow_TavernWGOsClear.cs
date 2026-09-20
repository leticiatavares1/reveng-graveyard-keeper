using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Clear interfering WGOs under Tavern", 0)]
[Category("Game Actions")]
public class Flow_TavernWGOsClear : MyFlowNode
{
	private struct WGOCoords
	{
		public string[] ids;

		public Vector2 coords;

		public WGOCoords(string[] ids, Vector2 coords)
		{
			this.ids = ids;
			this.coords = coords;
		}
	}

	private WGOCoords[] wgos_to_remove = new WGOCoords[8]
	{
		new WGOCoords(new string[4] { "tree_3_1", "tree_3_1_stump", "tree_spawner", "tree_micro" }, new Vector2(21024f, -1056f)),
		new WGOCoords(new string[1] { "bush_2" }, new Vector2(21216f, -1152f)),
		new WGOCoords(new string[1] { "tree_tiny_2" }, new Vector2(21348f, -1056f)),
		new WGOCoords(new string[4] { "tree_1_4", "tree_1_4_stump", "tree_spawner", "tree_micro" }, new Vector2(21600f, -1056f)),
		new WGOCoords(new string[1] { "tree_tiny_1" }, new Vector2(21816f, -936f)),
		new WGOCoords(new string[4] { "flower_small_1", "flower_small_4", "flower_small_7", "flower_spawner" }, new Vector2(21072f, -1296f)),
		new WGOCoords(new string[4] { "flower_small_1", "flower_small_4", "flower_small_7", "flower_spawner" }, new Vector2(21636f, -1164f)),
		new WGOCoords(new string[4] { "flower_small_2", "flower_small_5", "flower_small_8", "flower_spawner" }, new Vector2(21600f, -1308f))
	};

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WGOCoords[] array = wgos_to_remove;
			for (int i = 0; i < array.Length; i++)
			{
				WGOCoords wGOCoords = array[i];
				if (!WorldMap.DeleteWGO(wGOCoords.ids, wGOCoords.coords, string.Empty))
				{
					string[] obj = new string[5]
					{
						"FATAL ERROR: Can not delete WGO \"",
						wGOCoords.ids[0],
						"\" on coords (",
						null,
						null
					};
					Vector2 coords = wGOCoords.coords;
					obj[3] = coords.ToString();
					obj[4] = ")";
					Debug.LogError(string.Concat(obj));
				}
			}
			flow_out.Call(f);
		});
	}
}
