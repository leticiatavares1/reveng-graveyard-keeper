using System.Collections.Generic;
using JetBrains.Annotations;

public class BuildData
{
	private const string REMOVE_BUILD_DATA_POINTER_ICON_ID = "i_b_remove";

	private BuildingDef.BuildingMode buildingMode;

	[CanBeNull]
	private BuildingDef definition;

	public BuildingDef.BuildingMode BuildingMode => buildingMode;

	[CanBeNull]
	public BuildingDef Definition => definition;

	public string WgoId { get; private set; }

	public string IconId
	{
		get
		{
			if (definition == null)
			{
				return "i_b_remove";
			}
			return definition.BuildResultIcon;
		}
	}

	public List<NeedItemData> NeedItems => definition?.needItems;

	public static BuildData GetDataForBuild(BuildingDef buildingDef)
	{
		return new BuildData
		{
			buildingMode = buildingDef.buildingMode,
			definition = buildingDef,
			WgoId = buildingDef.wgoId
		};
	}

	public static BuildData GetDataForRemove()
	{
		return new BuildData
		{
			buildingMode = BuildingDef.BuildingMode.Remove
		};
	}
}
