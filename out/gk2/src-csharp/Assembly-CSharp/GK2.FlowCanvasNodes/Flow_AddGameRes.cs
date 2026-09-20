using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Add Gameres", 0)]
[Category("Game/GameRes")]
[Color("FFFFFF")]
public class Flow_AddGameRes : GKCustomFlowNodeWithWgoData
{
	public enum GameResGiveType
	{
		Set,
		Add
	}

	[GatherPortsCallback]
	public bool addToPlayer;

	[GatherPortsCallback]
	public GameResGiveType giveType;

	[GatherPortsCallback]
	[ShowIf("addToPlayer", 0)]
	public bool updateView;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> wgo;

	private ValueInput<string> gameresName;

	private ValueInput<float> gameresValue;

	private ValueOutput<string> gameResNameOut;

	public override string name => giveType.ToString() + " GameRes To " + (addToPlayer ? "Player" : "WgoData");

	protected override void RegisterPorts()
	{
		if (!addToPlayer)
		{
			base.RegisterPorts();
		}
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeGameRes);
		@out = AddFlowOutput("out".CapitalizeFirst());
		gameresName = AddValueInput<string>("gameresName".CapitalizeFirst());
		gameresValue = AddValueInput<float>("gameresValue".CapitalizeFirst());
		gameResNameOut = AddValueOutput("paramName", () => gameresName.value);
	}

	private void ChangeGameRes(Flow flow)
	{
		if (addToPlayer)
		{
			PlayerData playerData = MainGame.PlayerData;
			switch (giveType)
			{
			case GameResGiveType.Add:
				playerData.AddRes(gameresName.value, gameresValue.value);
				break;
			case GameResGiveType.Set:
				playerData.SetRes(gameresName.value, gameresValue.value);
				break;
			}
			if (gameresName.value == "cur_bodies_count" && playerData.CurrentWorldZoneData != null && playerData.CurrentWorldZoneData.Definition.id == "morgue")
			{
				GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
			}
		}
		else
		{
			WgoData wgoData = GetWgoData();
			if (wgoData != null)
			{
				switch (giveType)
				{
				case GameResGiveType.Add:
					wgoData.AddGameRes(gameresName.value, (int)gameresValue.value);
					break;
				case GameResGiveType.Set:
					wgoData.SetGameRes(gameresName.value, (int)gameresValue.value);
					break;
				}
				if (updateView)
				{
					GameScene.GetWgoViewGlobal(wgoData.UniqueId).UpdateViewAsset();
				}
			}
			else
			{
				Debug.LogError(string.Format("{0}: Tried to {1} GameRes to null WGO", "Flow_AddGameRes", giveType));
			}
		}
		@out.Call(flow);
	}
}
