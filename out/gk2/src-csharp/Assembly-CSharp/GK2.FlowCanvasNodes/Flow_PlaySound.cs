using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Play Sound", 0)]
[Category("Game/Sounds")]
[Color("f5da42")]
public class Flow_PlaySound : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool stop;

	[GatherPortsCallback]
	[ShowIf("stop", 0)]
	public bool atPosition;

	[GatherPortsCallback]
	[ShowIf("stop", 0)]
	public bool getSelfWgoForPosition;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> soundId;

	private new ValueInput<Transform> position;

	public override string name => (stop ? "Stop" : "Play") + " Sound";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), PlaySound);
		@out = AddFlowOutput("out".CapitalizeFirst());
		soundId = AddValueInput<string>("soundId".CapitalizeFirst());
		if (atPosition && !getSelfWgoForPosition)
		{
			position = AddValueInput<Transform>("position".CapitalizeFirst());
		}
	}

	private void PlaySound(Flow flow)
	{
		if (stop)
		{
			LazyAudio.Stop(soundId.value);
		}
		else
		{
			Transform transform = null;
			if (atPosition && getSelfWgoForPosition && base.SelfWgoData != null)
			{
				transform = GameScene.GetWgoViewGlobal(base.SelfWgoData.UniqueId).transform;
			}
			if (atPosition && position != null && transform == null)
			{
				transform = position.value;
			}
			if (atPosition && transform != null)
			{
				LazyAudio.PlayAtGameObject(soundId.value, transform, SpatialType.sound3D);
			}
			else
			{
				LazyAudio.Play(soundId.value);
			}
		}
		@out.Call(flow);
	}
}
