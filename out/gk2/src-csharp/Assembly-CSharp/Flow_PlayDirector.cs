using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Playables;

[Name("Play Director", 0)]
[Category("Game/Animation")]
public class Flow_PlayDirector : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private Flow flow;

	private Wgo wgo;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			wgo = GameScene.GetWgoViewGlobal(GetWgoData()?.UniqueId);
			if (!wgo)
			{
				@out.Call(flow);
				onFinished.Call(flow);
			}
			else
			{
				wgo.UpdateFlag(ChunkingIgnoreType.Animation, newValue: true);
				PlayableDirector componentInChildren = wgo.GetComponentInChildren<PlayableDirector>();
				if (!componentInChildren)
				{
					Debug.LogError("Flow_PlayDirector: PlayableDirector not found on wgo");
					@out.Call(flow);
					onFinished.Call(flow);
					wgo.UpdateFlag(ChunkingIgnoreType.Animation, newValue: false);
				}
				else
				{
					this.flow = flow;
					componentInChildren.Play();
					componentInChildren.stopped += OnFinished;
					@out.Call(flow);
				}
			}
		});
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
	}

	private void OnFinished(PlayableDirector pb)
	{
		if ((bool)wgo)
		{
			wgo.UpdateFlag(ChunkingIgnoreType.Animation, newValue: false);
		}
		pb.stopped -= OnFinished;
		onFinished.Call(flow);
	}
}
