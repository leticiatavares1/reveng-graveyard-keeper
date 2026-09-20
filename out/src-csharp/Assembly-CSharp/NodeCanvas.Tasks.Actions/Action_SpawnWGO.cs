using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Name("Spawn WGO", 0)]
[Category("Player")]
public class Action_SpawnWGO : WGOBehaviourAction
{
	public BBParameter<string> obj_id = new BBParameter<string>();

	public BBParameter<string> custom_tag = new BBParameter<string>();

	public BBParameter<bool> set_yourself_as_anchor = new BBParameter<bool>(value: true);

	public BBParameter<bool> return_true = new BBParameter<bool>(value: true);

	protected override string info => "Spawn WGO " + obj_id;

	protected override void OnExecute()
	{
		if (string.IsNullOrEmpty(obj_id.value))
		{
			return;
		}
		WorldGameObject worldGameObject = GS.Spawn(obj_id.value, base.self_wgo.tf, custom_tag.value);
		if (set_yourself_as_anchor.value && worldGameObject != null && worldGameObject.components != null && worldGameObject.components.character.enabled && base.self_wgo != null && base.self_wgo.gameObject != null)
		{
			if (base.self_wgo.components != null && base.self_wgo.components.character.enabled && base.self_wgo.components.character.anchor_obj != null)
			{
				worldGameObject.components.character.SetAnchor(base.self_wgo.components.character.anchor_obj);
			}
			else
			{
				worldGameObject.components.character.SetAnchor(base.self_wgo.gameObject);
			}
		}
		EndAction(return_true.value);
	}
}
