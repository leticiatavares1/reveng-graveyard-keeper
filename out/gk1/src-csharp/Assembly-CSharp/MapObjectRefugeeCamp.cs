using UnityEngine;

public class MapObjectRefugeeCamp : MonoBehaviour
{
	[SerializeField]
	private GameObject _content;

	private void Awake()
	{
		base.gameObject.SetActive(value: true);
		_content.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		if (CheckIsTavernBuild())
		{
			_content.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		_content.SetActive(value: false);
	}

	private bool CheckIsTavernBuild()
	{
		return MainGame.me.player.GetParam("camp_is_live") != 0f;
	}
}
