using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneWgoContentPart : MonoBehaviour
{
	public static bool IS_BAKING_IN_PROGRESS;

	private const string WORLD_ZONE_DATA_FOLDER = "Assets/AddressableAssets/WorldZones/Data";

	[SerializeField]
	private bool autoLoadContent = true;

	[SerializeField]
	private bool explicitSetWorldId;

	[SerializeField]
	private string explicitWorldId;

	[SerializeField]
	private List<WgoData> wgos = new List<WgoData>();

	[SerializeField]
	private List<SGuid> wgoUniqueIds = new List<SGuid>();

	[SerializeField]
	private List<SGuid> wsoUniqueIds = new List<SGuid>();

	[SerializeField]
	private List<WsoData> wsos = new List<WsoData>();

	[SerializeField]
	private List<WorldZoneBakedData> worldZones = new List<WorldZoneBakedData>();

	public bool AutoLoadContent => autoLoadContent;

	public IReadOnlyList<WgoData> Wgos => wgos;

	public IReadOnlyList<WsoData> Wsos => wsos;

	public IReadOnlyList<SGuid> WgoUniqueIds => wgoUniqueIds;

	public IReadOnlyList<SGuid> WsoUniqueIds => wsoUniqueIds;

	public IReadOnlyList<WorldZoneBakedData> WorldZones => worldZones;

	public string ExplicitWorldId => explicitWorldId;

	public string WorldId
	{
		get
		{
			if (explicitSetWorldId)
			{
				return explicitWorldId;
			}
			int num = base.name.IndexOf("_ContentPart", StringComparison.Ordinal);
			if (num < 0)
			{
				return base.name;
			}
			return base.name.Substring(0, num);
		}
	}
}
