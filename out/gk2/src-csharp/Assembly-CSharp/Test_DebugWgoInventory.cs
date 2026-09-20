using UnityEngine;

public class Test_DebugWgoInventory : MonoBehaviour
{
	public string gameResId;

	public string itemId;

	private Wgo wgo;

	private void Start()
	{
		wgo = GetComponentInParent<Wgo>();
	}

	private void AddTestItem()
	{
		wgo.Data.Inventory.AddItemToInventory(new Item(itemId));
	}

	private void AddTestGameRes()
	{
		wgo.Data.AddGameRes(gameResId, 1);
	}
}
