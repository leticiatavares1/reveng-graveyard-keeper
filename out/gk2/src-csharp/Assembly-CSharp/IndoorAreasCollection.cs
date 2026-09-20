using System.Collections.Generic;
using UnityEngine;

public class IndoorAreasCollection : MonoBehaviour
{
	[SerializeField]
	private List<IndoorArea> indoorAreas = new List<IndoorArea>();

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}
}
