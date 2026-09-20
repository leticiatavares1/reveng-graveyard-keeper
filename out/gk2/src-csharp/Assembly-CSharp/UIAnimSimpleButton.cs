using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIAnimSimpleButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Button button;

	public void Activate(string name, UnityAction callback)
	{
		base.gameObject.SetActive(value: true);
		base.transform.SetAsFirstSibling();
		button.onClick.AddListener(callback);
		label.text = name;
	}

	public void Select()
	{
		button.image.color = Color.green;
	}

	public void Deselect()
	{
		button.image.color = Color.white;
	}

	public void Deactivate()
	{
		Deselect();
		button.onClick.RemoveAllListeners();
		base.gameObject.SetActive(value: false);
	}
}
