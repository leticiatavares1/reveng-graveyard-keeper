using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Saves the blackboard variables in the provided key and to be loaded later on")]
[Category("✫ Blackboard")]
public class SaveBlackboard : ActionTask<Blackboard>
{
	[RequiredField]
	public BBParameter<string> saveKey;

	protected override string info => $"Save Blackboard [{saveKey.ToString()}]";

	protected override void OnExecute()
	{
		base.agent.Save(saveKey.value);
		EndAction();
	}
}
