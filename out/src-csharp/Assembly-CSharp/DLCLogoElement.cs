using UnityEngine;

public class DLCLogoElement : MonoBehaviour
{
	[SerializeField]
	private DLCEngine.DLCVersion _dlc_version;

	[SerializeField]
	private GameObject _ampersand_go;

	public DLCEngine.DLCVersion dlc_version => _dlc_version;

	public void Show(bool is_ampersand_to_show)
	{
		_ampersand_go.SetActive(is_ampersand_to_show);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
