using FlowCanvas;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Day Number", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_GetDayNumber : GKCustomFlowNode
{
	public enum AllDays
	{
		day_gluttony,
		day_sloth,
		day_lust,
		day_envy,
		day_pride,
		day_wrath
	}

	private ValueInput<AllDays> selectedDay;

	private ValueOutput<int> dayNumberOut;

	private ValueOutput<bool> isSelectedDayToday;

	protected override void RegisterPorts()
	{
		selectedDay = AddValueInput<AllDays>("selectedDay");
		dayNumberOut = AddValueOutput("Number", () => MainGame.Instance.GameSave.environmentData.CurrentDayNumber);
		isSelectedDayToday = AddValueOutput("isSelectedDayToday", () => MainGame.Instance.GameSave.environmentData.CurrentDayNumber == ConstDef.Get(selectedDay.value.ToString()).IntValue);
	}
}
