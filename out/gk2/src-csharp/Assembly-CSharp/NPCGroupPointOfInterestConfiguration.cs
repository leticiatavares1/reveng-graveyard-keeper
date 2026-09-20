using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCGroupPointOfInterestConfiguration
{
	[SerializeField]
	private string id;

	[SerializeField]
	private List<string> pointsOfInterest;

	public List<string> AllPonts => pointsOfInterest;

	public string Id => id;
}
