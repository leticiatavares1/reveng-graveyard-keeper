using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Starts the Dialogue Tree assigned on a Dialogue Tree Controller object with specified agent used for 'Instigator'.")]
[Icon("Dialogue", false, "")]
[Category("Dialogue")]
public class StartDialogueTree : ActionTask<IDialogueActor>
{
	[RequiredField]
	public BBParameter<DialogueTreeController> dialogueTreeController;

	public bool waitActionFinish = true;

	protected override string info => $"Start Dialogue {dialogueTreeController}";

	protected override void OnExecute()
	{
		if (waitActionFinish)
		{
			dialogueTreeController.value.StartDialogue(base.agent, delegate(bool success)
			{
				EndAction(success);
			});
		}
		else
		{
			dialogueTreeController.value.StartDialogue(base.agent);
			EndAction();
		}
	}
}
