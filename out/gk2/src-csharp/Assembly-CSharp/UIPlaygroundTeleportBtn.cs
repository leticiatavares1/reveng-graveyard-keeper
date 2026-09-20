using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPlaygroundTeleportBtn : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Button button;

	public string TaskId { get; private set; }

	public void Init(string name, UnityAction callback)
	{
		base.gameObject.SetActive(value: true);
		button.onClick.AddListener(callback);
		Match match = new Regex("GK2?-\\d+").Match(name);
		if (match.Success)
		{
			TaskId = match.Value;
			string text = "<color=#808080>" + name.Replace(TaskId, "") + "</color>";
			label.text = TaskId + text;
		}
		else
		{
			label.text = name;
		}
	}
}
