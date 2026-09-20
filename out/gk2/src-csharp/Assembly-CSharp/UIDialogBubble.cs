using System.Collections.Generic;
using Cinemachine;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogBubble : UISpeechBubble
{
	[SerializeField]
	private Image portraitImage;

	private PhraseData phraseData;

	private List<UIDialogBubble> dialogBubbles;

	private bool hasVoiceOver;

	private SpeechBubbleCornerFadeMask cornerFadeMask;

	public AnimationComponent animationComponent;

	public override void Init()
	{
		base.Init();
		UISpeechBubble.instance = this;
		Canvas component = GetComponent<Canvas>();
		component.overrideSorting = true;
		component.sortingOrder = 350;
	}

	public void OnDestroy()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePos);
	}

	public static UIDialogBubble ShowMessage(PhraseData data)
	{
		int num = (data.isPlayer ? MainGame.PlayerController.GetHashCode() : data.npcWgoData.GetHashCode());
		Vector3 targetPos = data.GetTargetPos();
		targetPos = CameraSystem.WorldToScreenPoint(targetPos);
		UIDialogBubble uIDialogBubble = UISpeechBubble.ShowMessage(voiceId: data.isPlayer ? VoiceID.Feather : (((object)data.npcWgoData.Definition.voiceId != null && !(data.npcWgoData.Definition.voiceId == VoiceID.None)) ? data.npcWgoData.Definition.voiceId : VoiceID.Feather), speakerId: num, localKey: data.text, uiPosition: targetPos, bubblePreset: data.preset, onDisappeared: data.onFinished, onTextAnimationEnd: null, onUpdatePosition: null, onForceHideCondition: null, forceCornerPosition: data.cornerPosition, fixedShowTime: data.fixedShowTimeValue) as UIDialogBubble;
		if (uIDialogBubble == null)
		{
			return null;
		}
		uIDialogBubble.phraseData = data;
		uIDialogBubble.hasVoiceOver = LazyAudio.VoiceOverPlayer.HasVoiceOver && VoiceOverSettings.IsEnabled;
		CinemachineCore.CameraUpdatedEvent.AddListener(uIDialogBubble.UpdatePos);
		if (!uIDialogBubble.phraseData.isPlayer)
		{
			uIDialogBubble.DrawNPCRelatedStuff();
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(data.npcWgoData.UniqueId);
			if ((bool)wgoViewGlobal)
			{
				uIDialogBubble.animationComponent = wgoViewGlobal.MainWgoPart?.AnimationComponent as AnimationComponent;
			}
		}
		else
		{
			uIDialogBubble.animationComponent = MainGame.PlayerController.PhysicalBody.PlayerView.PlayerAnimation;
		}
		Canvas component = uIDialogBubble.GetComponent<Canvas>();
		component.overrideSorting = true;
		component.sortingOrder = (data.isOverBlackout ? 900 : 350);
		uIDialogBubble.EnsureTextFits();
		LayoutRebuilder.ForceRebuildLayoutImmediate(uIDialogBubble.RootTransform);
		uIDialogBubble.SyncCornerFadeMask();
		return uIDialogBubble;
	}

	private void UpdatePos(CinemachineBrain brain)
	{
		if (base.gameObject.activeSelf)
		{
			Vector3 targetPos = phraseData.GetTargetPos();
			UpdatePositionAndCorner(CameraSystem.WorldToScreenPoint(targetPos), phraseData.cornerPosition);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!animationComponent)
		{
			return;
		}
		bool flag = (hasVoiceOver ? LazyAudio.VoiceOverPlayer.IsPlayingByLoud(base.VoiceId, base.VoiceOverLocalKey) : LazySpeechEngine.Instance.IsSpeechPlaying(base.VoiceId));
		if (animationComponent.UseTalkingAnimation)
		{
			float num = (flag ? 1f : 0f);
			if (!animationComponent.GetLayerWeight(AnimationComponent.Layers.Talking).EqualsTo(num))
			{
				animationComponent.SetLayerWeight(AnimationComponent.Layers.Talking, num);
			}
		}
		else if (animationComponent.HasTalkingHeadFrames)
		{
			if (flag)
			{
				animationComponent.StartTalkingHead();
			}
			else
			{
				animationComponent.PauseTalkingHead();
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)animationComponent)
		{
			if (animationComponent.UseTalkingAnimation)
			{
				animationComponent.SetLayerWeight(AnimationComponent.Layers.Talking, 0f);
			}
			animationComponent.StopTalkingHead();
			animationComponent = null;
		}
	}

	protected override void OnCornerChanged()
	{
		base.OnCornerChanged();
		SyncCornerFadeMask();
	}

	private void OnEnable()
	{
		EnsureCornerFadeMask();
		SyncCornerFadeMask();
	}

	private void EnsureCornerFadeMask()
	{
		if (cornerFadeMask == null)
		{
			cornerFadeMask = SpeechBubbleCornerFadeMask.Ensure(background, corners, base.gameObject);
		}
	}

	private void SyncCornerFadeMask()
	{
		EnsureCornerFadeMask();
		cornerFadeMask.Sync(currentCorner);
	}

	private void DrawNPCRelatedStuff()
	{
		if (currentCorner != null && currentCorner.customText != null)
		{
			currentCorner.customText.text = phraseData.npcWgoData.id;
		}
		if (phraseData.npcWgoData.Definition.usePortraitInDialogues)
		{
			portraitImage.sprite = phraseData.npcWgoData.Definition.Portrait;
			portraitImage.transform.parent.gameObject.SetActive(value: true);
		}
		else
		{
			portraitImage.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
