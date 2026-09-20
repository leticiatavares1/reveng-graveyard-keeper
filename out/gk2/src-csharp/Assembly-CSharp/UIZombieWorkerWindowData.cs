using System;
using LazyBearTechnology;

public class UIZombieWorkerWindowData : LazyWidgetDataBase
{
	public enum ZombieState
	{
		Body,
		Worker
	}

	public ZombieWgoData ZombieWgoData { get; private set; }

	public BodyOrgansInventoryWidgetData BodyOrgansInventoryWidgetData { get; private set; }

	public BodyPocketInventoryWidgetData BodyPocketInventoryWidgetData { get; private set; }

	public ZombieEquipmentInventoryWidgetData ZombieEquipmentInventoryWidgetData { get; private set; }

	public ZombieProgressionWidgetData ZombieProgressionWidgetData { get; private set; }

	public ZombieState State { get; private set; }

	public static event Action OnBodyItemAddOrRemove;

	public UIZombieWorkerWindowData(ZombieWgoData zombieWgoData)
	{
		ZombieWgoData = zombieWgoData;
		State = ZombieState.Worker;
		CommonFill();
	}

	public UIZombieWorkerWindowData(DropData dropData)
	{
		ZombieWgoData = MainGame.ZombieSystemData.GetZombie(dropData.Item.UniqueId);
		State = ZombieState.Body;
		CommonFill();
	}

	private void CommonFill()
	{
		Inventory inventory = new Inventory(ZombieWgoData.ZombieItem);
		BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData(isActive: true, ZombieWgoData, ZombieWgoData, inventory, null, null);
		BodyPocketInventoryWidgetData = new BodyPocketInventoryWidgetData(isActive: true, ZombieWgoData, ZombieWgoData, inventory, null, null);
		inventory.OnItemsAdd += delegate
		{
			UIZombieWorkerWindowData.OnBodyItemAddOrRemove?.Invoke();
		};
		inventory.OnItemsRemove += delegate
		{
			UIZombieWorkerWindowData.OnBodyItemAddOrRemove?.Invoke();
		};
		ZombieEquipmentInventoryWidgetData = new ZombieEquipmentInventoryWidgetData(ZombieWgoData, inventory, null, null);
		ZombieProgressionWidgetData = new ZombieProgressionWidgetData(ZombieWgoData);
	}
}
