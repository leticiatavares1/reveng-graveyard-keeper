using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("Player")]
[Name("Set Item", 0)]
public class Action_SetItem : WGOBehaviourAction
{
	public BBParameter<ItemDefinition.ItemType> item = new BBParameter<ItemDefinition.ItemType>(ItemDefinition.ItemType.None);

	protected override void OnExecute()
	{
		base.self_wgo.SetCurrentItem(item.value);
		EndAction(success: true);
	}
}
