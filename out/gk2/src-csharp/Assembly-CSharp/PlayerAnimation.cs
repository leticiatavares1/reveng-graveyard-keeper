using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PlayerAnimation : AnimationComponent
{
	public enum EyesBlinkingReason
	{
		ControlValue,
		ArmorEquippedEquippedState,
		EnabledState
	}

	[SerializeField]
	private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

	[SerializeField]
	private bool isCustomizationCharacter;

	private bool isEyesLayerActive;

	private Coroutine eyesBlinkCoroutine;

	private int eyesBlinkTriggerId = Animator.StringToHash("do_eyes_blink");

	private MultiFlagAND<EyesBlinkingReason> eyesBlinkingMultiflag = new MultiFlagAND<EyesBlinkingReason>();

	private float lastClimbStepNormalizedTime = -1f;

	private Action customDeathAnimationFinishedCallback;

	public int PlayerDeathExitTriggerId => Animator.StringToHash("player_death_exit");

	public MultiFlagAND<EyesBlinkingReason> EyesBlinkingMultiflag => eyesBlinkingMultiflag;

	public override void Init(SkinPresetGK2 skinPreset)
	{
		if (!isInitialized)
		{
			skinChanger = new SkinChangerGK2(base.gameObject, spriteRenderers, isCustomizationCharacter);
			ApplyCustomLayers();
			isInitialized = true;
			dropView.gameObject.SetActive(value: false);
		}
	}

	public override void ChangeSkinPreset(SkinPresetGK2 preset)
	{
		if (!isInitialized || skinChanger == null)
		{
			skinChanger = new SkinChangerGK2(base.gameObject, spriteRenderers, isCustomizationCharacter);
			isInitialized = true;
		}
		skinPreset = preset;
		skinChanger.ApplySkin(preset);
	}

	public void ApplyPlayerColors(Texture2D palette, List<CustomizablePartType> affectedPartTypes, SkinPresetGK2 customSkinPreset = null)
	{
		SkinPresetGK2 skinPresetGK = ((customSkinPreset != null) ? customSkinPreset : PlayerSkinHelper.CurrentPreset);
		for (int i = 0; i < affectedPartTypes.Count; i++)
		{
			switch (affectedPartTypes[i])
			{
			case CustomizablePartType.Head:
				skinPresetGK.head.palette = palette;
				break;
			case CustomizablePartType.Hair:
				skinPresetGK.hairstyle.palette = palette;
				break;
			case CustomizablePartType.Beard:
				skinPresetGK.beard.palette = palette;
				break;
			case CustomizablePartType.Body:
				skinPresetGK.body.palette = palette;
				break;
			case CustomizablePartType.Arms:
				skinPresetGK.arms.palette = palette;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		skinChanger.ApplyShaderParameters();
	}

	public void SetCustomDeathAnimationFinishedCallback(Action callback)
	{
		customDeathAnimationFinishedCallback = callback;
	}

	public void OnSwordAttack()
	{
		MainGame.PlayerData.staminaSystem.ConsumeStamina();
		LazyAudio.PlayAtGameObject("sword_attack", base.transform, SpatialType.sound3D);
	}

	public override void OnBowAimStart()
	{
		MainGame.PlayerData.staminaSystem.ConsumeStamina();
		base.OnBowAimStart();
	}

	public override void OnSpearAttack()
	{
		MainGame.PlayerData.staminaSystem.ConsumeStamina();
		base.OnSpearAttack();
	}

	public override void OnDeathAnimationFinished()
	{
		if (customDeathAnimationFinishedCallback != null)
		{
			customDeathAnimationFinishedCallback();
			return;
		}
		UIDialogWindowData data = new UIDialogWindowData("ui_death", "ui_youre_dead", new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UIDialogWindow>().Close, "btn_ok", null, replaceForGamepad: true, GameKey.Select));
		LazyUI.GetWindow<UIDialogWindow>().Open(data, delegate
		{
			MainGame.PlayerController.SetControlTakenType(TakenControlType.ByDeath, isEnabled: true);
			SetTrigger(PlayerDeathExitTriggerId);
			animator.Update(0f);
		});
	}

	public void OnClimbStep()
	{
		float num = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
		if (!Mathf.Abs(num - lastClimbStepNormalizedTime).EqualsTo(0f))
		{
			lastClimbStepNormalizedTime = num;
			LazyAudio.PlayAndForget("ladder_climb");
		}
	}

	private void Start()
	{
		eyesBlinkingMultiflag.Init(HandleEyesBlinkingMultiflagChanged, initialFlag: true);
		HandleEyesBlinkingMultiflagChanged(eyesBlinkingMultiflag.ResultFlag);
	}

	private IEnumerator EyesBlinkCoroutine()
	{
		while (isEyesLayerActive)
		{
			float seconds = UnityEngine.Random.Range(3f, 7f);
			yield return new WaitForSeconds(seconds);
			if (!isEyesLayerActive)
			{
				break;
			}
			int blinkCount = ((UnityEngine.Random.value < 0.7f) ? 1 : 2);
			float blinkBetweenCountPause = UnityEngine.Random.Range(0.1f, 0.25f);
			SetLayerWeight(Layers.Eyes, 1f);
			while (blinkCount > 0 && isEyesLayerActive)
			{
				float waitTimeout = 10f;
				yield return new WaitUntil(delegate
				{
					waitTimeout -= Time.deltaTime;
					return waitTimeout <= 0f || !animator.IsInTransition(17);
				});
				if (!isEyesLayerActive)
				{
					break;
				}
				animator.SetTrigger(eyesBlinkTriggerId);
				waitTimeout = 10f;
				yield return new WaitUntil(delegate
				{
					waitTimeout -= Time.deltaTime;
					return waitTimeout <= 0f || animator.GetCurrentAnimatorStateInfo(17).normalizedTime >= 1f;
				});
				blinkCount--;
				if (blinkCount > 0 && isEyesLayerActive)
				{
					yield return new WaitForSeconds(blinkBetweenCountPause);
				}
			}
			SetLayerWeight(Layers.Eyes, 0f);
		}
		eyesBlinkCoroutine = null;
	}

	private void HandleEyesBlinkingMultiflagChanged(bool canBlink)
	{
		isEyesLayerActive = canBlink;
		if (canBlink)
		{
			if (eyesBlinkCoroutine == null)
			{
				eyesBlinkCoroutine = StartCoroutine(EyesBlinkCoroutine());
			}
		}
		else if (eyesBlinkCoroutine != null)
		{
			StopCoroutine(eyesBlinkCoroutine);
			eyesBlinkCoroutine = null;
			SetLayerWeight(Layers.Eyes, 0f);
		}
	}
}
