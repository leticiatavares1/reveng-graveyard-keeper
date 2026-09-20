using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Talk", 0)]
[Category("Game Actions")]
[Icon("Dialogue", false, "")]
public class Flow_Talk : MyFlowNode
{
	public enum AnimPlayOrder
	{
		Before,
		Together,
		After
	}

	public enum SpeechBubbleForceOrientation
	{
		Auto,
		Left,
		Right
	}

	public bool say_as_player;

	public bool override_pos;

	public SmartSpeechEngine.VoiceID force_voice_id;

	public override string name
	{
		get
		{
			SpeechBubbleGUI.SpeechBubbleType value = GetInputValuePort<SpeechBubbleGUI.SpeechBubbleType>("Type").value;
			if (GetInputValuePort<bool>("Player talk").value)
			{
				return $"<color=#FFFF50>Player {value}</color>";
			}
			if (!GetInputValuePort("WGO").isConnected)
			{
				return $"<color=#30FF30>Self {value}</color>";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		WorldGameObject out_wgo = null;
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_txt = AddValueInput<string>("Text");
		ValueInput<string> par_anim = AddValueInput<string>("Anim id", "Anim");
		ValueInput<AnimPlayOrder> par_anim_t = AddValueInput<AnimPlayOrder>("Anim play");
		ValueInput<SpeechBubbleGUI.SpeechBubbleType> par_type = AddValueInput<SpeechBubbleGUI.SpeechBubbleType>("Bubble", "Type");
		ValueInput<bool> par_player = AddValueInput<bool>("Player?", "Player talk");
		ValueInput<SpeechBubbleForceOrientation> par_orientation = AddValueInput<SpeechBubbleForceOrientation>("Orientation", "OrientationType");
		FlowOutput flow_on_finished = AddFlowOutput("On Finished");
		FlowOutput flow_out = AddFlowOutput("Immediate", "Out");
		AddValueOutput("WGO", () => out_wgo);
		ValueInput<Transform> par_pos = AddValueInput<Transform>("OverrodePosition");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject obj_wgo = (par_player.value ? MainGame.me.player : WGOParamOrSelf(par_wgo));
			if (obj_wgo == null)
			{
				Debug.LogError("Trying to talk with a null WGO: " + par_txt.value);
				flow_out.Call(f);
			}
			else
			{
				SpeechBubbleForceOrientation value = par_orientation.value;
				bool? to_left = null;
				if (value != 0)
				{
					to_left = value == SpeechBubbleForceOrientation.Left;
				}
				MainGame.me.save.known_npcs.GetOrCreateNPC(obj_wgo.obj_id);
				out_wgo = obj_wgo;
				Action on_said = delegate
				{
					flow_on_finished.Call(f);
				};
				Action action = delegate
				{
					obj_wgo.Say(par_txt.value, delegate
					{
						out_wgo = obj_wgo;
						on_said();
					}, type: par_type.value, to_left: to_left, force_voice: force_voice_id, say_as_player: say_as_player, overrode_pos: override_pos ? par_pos.value : null);
					flow_out.Call(f);
				};
				if (string.IsNullOrEmpty(par_anim.value))
				{
					action();
				}
				else
				{
					Debug.Log("Triggering animation " + par_anim.value + " and waiting. WGO = " + obj_wgo.name + ", " + par_anim_t.value);
					switch (par_anim_t.value)
					{
					case AnimPlayOrder.Before:
						obj_wgo.TriggerSmartAnimation(par_anim.value, action);
						break;
					case AnimPlayOrder.Together:
						obj_wgo.TriggerSmartAnimation(par_anim.value);
						action();
						break;
					case AnimPlayOrder.After:
						on_said = delegate
						{
							obj_wgo.TriggerSmartAnimation(par_anim.value, delegate
							{
								flow_on_finished.Call(f);
							});
						};
						action();
						break;
					}
				}
			}
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		if (GUILayout.Button("Refresh"))
		{
			GatherPorts();
		}
	}
}
