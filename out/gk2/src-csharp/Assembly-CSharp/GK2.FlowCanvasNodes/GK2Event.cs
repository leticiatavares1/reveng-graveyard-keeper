using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("GK2 Event", 0)]
[Category("Events/Custom")]
[Description("Add event from DialogData through FlowDialog tool")]
public class GK2Event : CustomEvent
{
	private string buttonText = "Build event";

	[SerializeField]
	private bool addControlNodes = true;
}
