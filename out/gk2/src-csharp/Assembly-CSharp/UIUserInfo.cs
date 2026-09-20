using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUserInfo : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI userId;

	[SerializeField]
	private TextMeshProUGUI userIp;

	[SerializeField]
	private Image bgImage;

	public void Init(string id, string ip)
	{
		userId.text = "User Id: " + id;
		userIp.text = ip;
	}

	public void Activate()
	{
		bgImage.color = Color.white;
	}

	public void Deactivate()
	{
		bgImage.color = Color.grey;
	}
}
