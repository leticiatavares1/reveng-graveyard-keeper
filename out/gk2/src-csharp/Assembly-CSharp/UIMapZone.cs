using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIMapZone : MonoBehaviour
{
	[SerializeField]
	private string id;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private GameObject cloudObject;

	[SerializeField]
	private bool alwaysVisible;

	private List<UIMapWorldZone> worldZones;

	private List<UISortComponent> sortComponents;

	private bool isHidden;

	public string Id => id;

	public List<UISortComponent> SortComponents => sortComponents;

	public void Init()
	{
		sortComponents = GetComponentsInChildren<UISortComponent>(includeInactive: true).ToList();
		worldZones = GetComponentsInChildren<UIMapWorldZone>(includeInactive: true).ToList();
		base.gameObject.SetActive(value: false);
		if ((bool)cloudObject)
		{
			cloudObject.SetActive(value: true);
		}
	}

	public void Draw(bool isHidden)
	{
		this.isHidden = isHidden;
		if (alwaysVisible)
		{
			isHidden = false;
		}
		base.gameObject.SetActive(value: true);
		if (label != null)
		{
			label.text = LLBase.L("wz_" + id);
			label.gameObject.SetActive(!isHidden);
		}
		foreach (UISortComponent sortComponent in SortComponents)
		{
			sortComponent.gameObject.SetActive(isHidden);
		}
		foreach (UIMapWorldZone worldZone in worldZones)
		{
			bool flag = MainGame.Instance.GameSave.knowledgeSystem.visitedWorldZones.Contains(worldZone.WorldZoneId);
			worldZone.gameObject.SetActive(!isHidden && flag);
			worldZone.Label.text = LLBase.L("wz_" + worldZone.WorldZoneId);
		}
	}
}
