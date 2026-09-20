using UnityEngine;

public class LiftCraneDeliverSMB : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Wgo componentInParent = animator.gameObject.GetComponentInParent<Wgo>();
		if (componentInParent == null)
		{
			Debug.LogError("[LiftCraneDeliverSMB] No WGO found for crane " + animator.gameObject.name);
			return;
		}
		string text = componentInParent.Data.GameResStr.Get("target_storage_wgo");
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogWarning("[LiftCraneDeliverSMB] No target storage ID found for crane " + text);
			return;
		}
		WgoData wgoData = MainGame.WorldData.GetWgoData(text);
		if (wgoData == null)
		{
			Debug.LogError("[LiftCraneDeliverSMB] Target storage WGO " + text + " not found!");
			return;
		}
		Inventory inventory = componentInParent.Data.Inventory;
		foreach (Item item in inventory.Data.Inventory)
		{
			wgoData.Inventory.Data.AddItemToInventory(item);
		}
		inventory.Clear();
	}
}
