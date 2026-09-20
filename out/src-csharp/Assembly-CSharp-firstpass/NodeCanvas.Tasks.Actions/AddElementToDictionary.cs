using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Blackboard/Dictionaries")]
public class AddElementToDictionary<T> : ActionTask
{
	[RequiredField]
	[BlackboardOnly]
	public BBParameter<Dictionary<string, T>> dictionary;

	public BBParameter<string> key;

	public BBParameter<T> value;

	protected override string info => $"{dictionary}[{key}] = {value}";

	protected override void OnExecute()
	{
		if (dictionary.value == null)
		{
			EndAction(success: false);
			return;
		}
		dictionary.value[key.value] = value.value;
		EndAction();
	}
}
