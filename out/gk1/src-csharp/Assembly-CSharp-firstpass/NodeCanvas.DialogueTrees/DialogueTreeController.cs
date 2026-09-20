using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

public class DialogueTreeController : GraphOwner<DialogueTree>, IDialogueActor
{
	string IDialogueActor.name => base.name;

	Texture2D IDialogueActor.portrait => null;

	Sprite IDialogueActor.portraitSprite => null;

	Color IDialogueActor.dialogueColor => Color.white;

	Vector3 IDialogueActor.dialoguePosition => Vector3.zero;

	Transform IDialogueActor.transform => base.transform;

	public void StartDialogue()
	{
		graph = GetInstance(graph);
		graph.StartGraph(this, blackboard, autoUpdate: true);
	}

	public void StartDialogue(IDialogueActor instigator)
	{
		graph = GetInstance(graph);
		graph.StartGraph((instigator is Component) ? ((Component)instigator) : instigator.transform, blackboard, autoUpdate: true);
	}

	public void StartDialogue(IDialogueActor instigator, Action<bool> callback)
	{
		graph = GetInstance(graph);
		graph.StartGraph((instigator is Component) ? ((Component)instigator) : instigator.transform, blackboard, autoUpdate: true, callback);
	}

	public void StartDialogue(Action<bool> callback)
	{
		graph = GetInstance(graph);
		graph.StartGraph(this, blackboard, autoUpdate: true, callback);
	}

	public void SetActorReference(string paramName, IDialogueActor actor)
	{
		if (base.behaviour != null)
		{
			base.behaviour.SetActorReference(paramName, actor);
		}
	}

	public void SetActorReferences(Dictionary<string, IDialogueActor> actors)
	{
		if (base.behaviour != null)
		{
			base.behaviour.SetActorReferences(actors);
		}
	}

	public IDialogueActor GetActorReferenceByName(string paramName)
	{
		if (!(base.behaviour != null))
		{
			return null;
		}
		return base.behaviour.GetActorReferenceByName(paramName);
	}
}
