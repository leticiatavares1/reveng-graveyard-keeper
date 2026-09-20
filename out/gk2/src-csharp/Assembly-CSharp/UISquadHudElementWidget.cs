using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UISquadHudElementWidget : LazyWidget<UISquadHudElementWidgetData>
{
	[SerializeField]
	private Image weaponIcon;

	[SerializeField]
	private Slider healthBar;

	[SerializeField]
	private Sprite bowIcon;

	[SerializeField]
	private Sprite spearIcon;

	[SerializeField]
	private Sprite swordIcon;

	private HPComponent hpComponent;

	public override void Hide()
	{
		UnsubscribeFromHp();
		base.Hide();
	}

	public override void Redraw()
	{
		base.Redraw();
		UnsubscribeFromHp();
		hpComponent = data?.Fighter?.HpComponent;
		if (hpComponent != null)
		{
			hpComponent.OnHpChanged += HandleHpChanged;
		}
		UpdateHealthBar();
		UpdateWeaponIcon();
	}

	private void UpdateHealthBar()
	{
		if (!(healthBar == null))
		{
			if (hpComponent == null)
			{
				healthBar.value = 0f;
				return;
			}
			healthBar.maxValue = hpComponent.MaxHpValue;
			healthBar.value = hpComponent.Hp;
		}
	}

	private void UpdateWeaponIcon()
	{
		if (!(weaponIcon == null))
		{
			Item currentWeapon = GetCurrentWeapon();
			Sprite sprite = GetWeaponIcon(currentWeapon);
			if (sprite == null)
			{
				weaponIcon.gameObject.SetActive(value: false);
				return;
			}
			weaponIcon.sprite = sprite;
			weaponIcon.gameObject.SetActive(value: true);
		}
	}

	private Item GetCurrentWeapon()
	{
		if (data?.Fighter is ZombieWgoData zombieWgoData)
		{
			return zombieWgoData.Hand;
		}
		return data?.Fighter?.Inventory?.GetItemByGroupId("weapon");
	}

	private Sprite GetWeaponIcon(Item weapon)
	{
		if (weapon == null || weapon.IsEmpty || weapon.Definition == null)
		{
			return null;
		}
		return weapon.Definition.type switch
		{
			ItemType.Bow => bowIcon, 
			ItemType.Pike => spearIcon, 
			ItemType.Sword => swordIcon, 
			_ => null, 
		};
	}

	private void HandleHpChanged(HPComponent component)
	{
		UpdateHealthBar();
	}

	private void UnsubscribeFromHp()
	{
		if (hpComponent != null)
		{
			hpComponent.OnHpChanged -= HandleHpChanged;
			hpComponent = null;
		}
	}

	protected override void TestDraw()
	{
	}
}
