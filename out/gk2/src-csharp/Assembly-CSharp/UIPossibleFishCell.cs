using UnityEngine;

public class UIPossibleFishCell : MonoBehaviour
{
	public UIItemCell itemCell;

	public GameObject locked;

	public GameObject inactive;

	public void Draw(Item item)
	{
		Debug.Log("UIPossibleFishCell Draw: " + item.id, base.gameObject);
		locked.SetActive(value: false);
		inactive.SetActive(value: false);
		itemCell.Draw(item);
	}

	public void DrawInactive(Item item)
	{
		Debug.Log("UIPossibleFishCell DrawInactive: " + item.id, base.gameObject);
		locked.SetActive(value: false);
		inactive.SetActive(value: true);
		itemCell.Draw(item);
	}

	public void DrawLocked()
	{
		Debug.Log("UIPossibleFishCell DrawLocked", base.gameObject);
		locked.SetActive(value: true);
		inactive.SetActive(value: false);
		itemCell.DrawEmpty(drawAsNonInteractable: true);
	}

	public void DrawLockedInactive()
	{
		Debug.Log("UIPossibleFishCell DrawLockedInactive", base.gameObject);
		locked.SetActive(value: true);
		inactive.SetActive(value: true);
		itemCell.DrawEmpty(drawAsNonInteractable: true);
	}
}
