using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFightingControlsWidget : LazyWidget<UIFightingControlsWidgetData>
{
	[SerializeField]
	private GameObject changeWeaponObj;

	[SerializeField]
	private Image poseImage;

	[SerializeField]
	private Sprite poseLocked;

	[SerializeField]
	private Sprite poseFree;

	[SerializeField]
	private GameObject arrowDown;

	[SerializeField]
	private GameObject arrowUp;

	[SerializeField]
	private GameObject arrowLeft;

	[SerializeField]
	private GameObject arrowRight;

	[SerializeField]
	private GameObject lockedPosGlow;

	[SerializeField]
	private GameObject lockedPosSelection;

	[SerializeField]
	private Color activeCol;

	[SerializeField]
	private Color inactiveCol;

	[SerializeField]
	private Image activeWeaponIcon;

	[SerializeField]
	private Image inactiveWeaponIcon;

	[SerializeField]
	private TextMeshProUGUI poseGamepadTip;

	[SerializeField]
	private TextMeshProUGUI attackGamepadTip;

	[SerializeField]
	private TextMeshProUGUI changeWeaponGamepadTip;

	private bool canChangeWeapon;

	private bool isPlayerInFocus;

	private bool isAttackAnimPlaying;

	private Direction focusedDirection;

	private ItemType activeWeaponType;

	public override void Redraw()
	{
		base.Redraw();
		RedrawWeapons();
		RedrawPose();
	}

	private void RedrawWeapons()
	{
		bool flag = MainGame.PlayerController.Sword.id != "empty";
		bool flag2 = MainGame.PlayerController.Bow.id != "empty";
		canChangeWeapon = flag && flag2;
		changeWeaponObj.SetActive(canChangeWeapon);
		AttackComponent attackComponent = MainGame.PlayerController.AttackComponent;
		if (attackComponent.HasEquippedWeapon)
		{
			ItemDef itemDef = attackComponent.weapon.ItemDef;
			activeWeaponType = itemDef.type;
			activeWeaponIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(itemDef.iconId);
			activeWeaponIcon.BlueColorReplace(activeCol);
			if (canChangeWeapon)
			{
				ItemDef itemDef2 = ((activeWeaponType == ItemType.Sword) ? MainGame.PlayerController.Bow.Definition : MainGame.PlayerController.Sword.Definition);
				inactiveWeaponIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(itemDef2.iconId);
				inactiveWeaponIcon.BlueColorReplace(inactiveCol);
			}
		}
	}

	private void UpdateCombatFlags()
	{
		isPlayerInFocus = IsAttackFocusInputActive();
		focusedDirection = Direction.None;
		SSMState curState = MainGame.PlayerController.Ssm.CurState;
		if (curState is AttackSwordFocusedPlayerState { IsActive: not false } attackSwordFocusedPlayerState)
		{
			isPlayerInFocus = true;
			focusedDirection = attackSwordFocusedPlayerState.FocusedDirection;
		}
		else if (curState is AttackBowFocusedPlayerState { IsActive: not false } attackBowFocusedPlayerState)
		{
			isPlayerInFocus = true;
			focusedDirection = attackBowFocusedPlayerState.FocusedDirection;
		}
		else if (isPlayerInFocus)
		{
			focusedDirection = MainGame.PlayerController.PlayerData.Direction.ConvertFromVector2();
		}
		bool flag;
		if (curState is AttackSwordPlayerState attackSwordPlayerState)
		{
			if (!attackSwordPlayerState.IsActive)
			{
				goto IL_011a;
			}
			flag = true;
		}
		else if (curState is AttackSwordDefaultPlayerState attackSwordDefaultPlayerState)
		{
			if (!attackSwordDefaultPlayerState.IsActive)
			{
				goto IL_011a;
			}
			flag = true;
		}
		else if (curState is AttackSwordContinuousPlayerState attackSwordContinuousPlayerState)
		{
			if (!attackSwordContinuousPlayerState.IsActive)
			{
				goto IL_011a;
			}
			flag = true;
		}
		else if (curState is AttackBowDefaultPlayerState attackBowDefaultPlayerState)
		{
			if (!attackBowDefaultPlayerState.IsActive)
			{
				goto IL_011a;
			}
			flag = true;
		}
		else
		{
			if (!(curState is AttackBowAutoPlayerState { IsActive: not false }))
			{
				goto IL_011a;
			}
			flag = true;
		}
		goto IL_0137;
		IL_011a:
		flag = MainGame.PlayerController.View.PlayerAnimation.GetLayerWeight(AnimationComponent.Layers.WeaponHitBox) > 0f;
		goto IL_0137;
		IL_0137:
		isAttackAnimPlaying = flag;
	}

	private bool IsAttackFocusInputActive()
	{
		PlayerController playerController = MainGame.PlayerController;
		if (!playerController.IsControlsEnabled || !LazyInput.GetKey(GameKey.AttackFocus))
		{
			return false;
		}
		AttackComponent attackComponent = playerController.AttackComponent;
		if (!attackComponent.HasEquippedWeapon)
		{
			return false;
		}
		PlayerInputHandler playerInputHandler = playerController.PlayerInputHandler;
		if (!attackComponent.IsRangedWeapon)
		{
			return playerInputHandler.meleeMode == MeleeMode.WithFocus;
		}
		if (playerInputHandler.rangedMode == RangedMode.WithFocus)
		{
			return MainGame.PlayerData.staminaSystem.CanPerformAttack();
		}
		return false;
	}

	private void RedrawPose()
	{
		UpdateCombatFlags();
		arrowUp.SetActive(value: false);
		arrowDown.SetActive(value: false);
		arrowLeft.SetActive(value: false);
		arrowRight.SetActive(value: false);
		if (isPlayerInFocus)
		{
			poseImage.sprite = poseLocked;
			if (focusedDirection == Direction.Left)
			{
				poseImage.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
			else
			{
				poseImage.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			switch (focusedDirection)
			{
			case Direction.Right:
				arrowRight.SetActive(value: true);
				break;
			case Direction.Up:
				arrowUp.SetActive(value: true);
				break;
			case Direction.Left:
				arrowLeft.SetActive(value: true);
				break;
			case Direction.Down:
				arrowDown.SetActive(value: true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case Direction.None:
				break;
			}
			lockedPosGlow.SetActive(value: true);
			lockedPosSelection.SetActive(value: true);
		}
		else
		{
			lockedPosGlow.SetActive(value: false);
			lockedPosSelection.SetActive(value: false);
			poseImage.sprite = poseFree;
			poseImage.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void DrawGamepadTips()
	{
		poseGamepadTip.text = ControllerIconLibrary.GetIconId(GameKey.AttackFocus);
		attackGamepadTip.text = ControllerIconLibrary.GetIconId(GameKey.Attack);
		changeWeaponGamepadTip.text = ControllerIconLibrary.GetIconId(GameKey.ChangeWeapon);
	}

	public override void CustomUpdate()
	{
		base.CustomUpdate();
		UpdateCombatFlags();
		if (LazyInput.GetKeyDown(GameKey.ChangeWeapon))
		{
			RedrawWeapons();
			if (canChangeWeapon && !isAttackAnimPlaying && !isPlayerInFocus)
			{
				if (activeWeaponType == ItemType.Sword)
				{
					MainGame.PlayerController.AttackComponent.EquipWeapon(MainGame.PlayerController.Bow.Definition);
				}
				else
				{
					MainGame.PlayerController.AttackComponent.EquipWeapon(MainGame.PlayerController.Sword.Definition);
				}
			}
		}
		RedrawWeapons();
		RedrawPose();
	}

	protected override void TestDraw()
	{
	}
}
