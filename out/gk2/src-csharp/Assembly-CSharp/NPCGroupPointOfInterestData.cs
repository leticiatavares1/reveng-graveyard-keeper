using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class NPCGroupPointOfInterestData
{
	[SerializeField]
	private string groupId;

	[SerializeField]
	private List<SGuid> wgos = new List<SGuid>();

	public List<SGuid> Wgos => wgos;

	public string Id => groupId;

	public NPCGroupPointOfInterestConfiguration Configuration => LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.GetGroupById(groupId);

	public NPCGroupPointOfInterestData(NPCGroupPointOfInterestConfiguration configuration)
	{
		groupId = configuration.Id;
	}

	public void AddWgoToGroup(WgoData wgoData)
	{
		wgos.Add(wgoData.UniqueId);
	}

	public void RemoveWgoFromGroup(WgoData wgoData)
	{
		wgos.Remove(wgoData.UniqueId);
	}
}
