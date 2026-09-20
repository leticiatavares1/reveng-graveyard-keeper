using TMPro;
using UnityEngine;

public class UIMapWorldZone : MonoBehaviour
{
	[SerializeField]
	private string worldZoneId;

	[SerializeField]
	private TextMeshProUGUI label;

	public string WorldZoneId => worldZoneId;

	public TextMeshProUGUI Label => label;
}
