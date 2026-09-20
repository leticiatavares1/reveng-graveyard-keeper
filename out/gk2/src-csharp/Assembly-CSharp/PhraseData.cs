using System;
using LazyBearTechnology;
using UnityEngine;

public struct PhraseData
{
	public bool isPlayer;

	public WgoData npcWgoData;

	public string text;

	public Action onFinished;

	public UIBasicBubble.ForceCornerPosition cornerPosition;

	public SpeechBubblePreset preset;

	public SpeechBubbleType speechType;

	public float fixedShowTimeValue;

	public bool isOverBlackout;

	public PhraseData(bool isPlayer, WgoData npcWgoData, string text, Action onFinished, SpeechBubblePreset preset, SpeechBubbleType speechType = SpeechBubbleType.Talk, UIBasicBubble.ForceCornerPosition cornerPosition = UIBasicBubble.ForceCornerPosition.Auto, float fixedShowTimeValue = 0f, bool isOverBlackout = false)
	{
		this.isPlayer = isPlayer;
		this.npcWgoData = npcWgoData;
		this.text = text;
		this.onFinished = onFinished;
		this.preset = preset;
		this.speechType = speechType;
		this.cornerPosition = cornerPosition;
		this.fixedShowTimeValue = fixedShowTimeValue;
		this.isOverBlackout = isOverBlackout;
	}

	public Vector3 GetTargetPos()
	{
		if (isPlayer || npcWgoData.Definition.usePortraitInDialogues)
		{
			return MainGame.PlayerController.BubblePoint.position;
		}
		if (npcWgoData.id == "player_wisp")
		{
			return MainGame.PlayerController.WispController.BubblePoint.position;
		}
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(npcWgoData.UniqueId);
		if (wgoViewGlobal != null)
		{
			return wgoViewGlobal.BubbleDrawablePosition;
		}
		return npcWgoData.BubblePos;
	}

	public Direction GetDirection()
	{
		if (isPlayer || npcWgoData.Definition.usePortraitInDialogues || npcWgoData.id == "player_wisp")
		{
			return MainGame.PlayerController.Direction;
		}
		return npcWgoData.Direction;
	}
}
