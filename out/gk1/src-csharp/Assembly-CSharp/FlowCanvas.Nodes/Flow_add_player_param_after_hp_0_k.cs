using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Affect add_player_param_after_hp_0_k", 0)]
public class Flow_add_player_param_after_hp_0_k : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(wgo);
			GameRes gameRes = new GameRes(worldGameObject.obj_def.add_player_param_after_hp_0);
			if (worldGameObject.obj_def.add_player_param_after_hp_0_k.has_expression)
			{
				float num = worldGameObject.obj_def.add_player_param_after_hp_0_k.EvaluateFloat(worldGameObject);
				foreach (GameResAtom item in gameRes.ToAtomList())
				{
					gameRes.Set(item.type, item.value * num);
				}
			}
			MainGame.me.player.AddToParams(gameRes);
			if (gameRes.Get("hp") < 0f)
			{
				EffectBubblesManager.ShowStackedHP(MainGame.me.player, gameRes.Get("hp"));
			}
			flow_out.Call(f);
		});
	}
}
