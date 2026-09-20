using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Find GD Point Data", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_FindGdPoint : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool getList;

	[GatherPortsCallback]
	public bool getView;

	[GatherPortsCallback]
	public bool findByCustomTag;

	private ValueInput<string> gdPointId;

	private ValueInput<string> gdPointCustomTag;

	private ValueOutput<GDPointData> gdPointData;

	private ValueOutput<GDPoint> gdPoint;

	private ValueOutput<List<GDPointData>> gdPointDataList;

	private ValueOutput<List<GDPoint>> gdPointList;

	private ValueOutput<Vector3> gdPointPosition;

	private GDPointData foundGdPointData;

	private GDPoint foundGdPoint;

	private List<GDPointData> foundGdPointDataList = new List<GDPointData>();

	private List<GDPoint> foundGdPointList = new List<GDPoint>();

	public override string name => "Find GD Point" + ((!getView) ? " Data" : "") + (getList ? " List" : "") + (findByCustomTag ? " By Tag" : "");

	protected override void RegisterPorts()
	{
		if (!findByCustomTag)
		{
			gdPointId = AddValueInput<string>("id");
		}
		else
		{
			gdPointCustomTag = AddValueInput<string>("customTag");
		}
		if (!getList)
		{
			gdPointData = AddValueOutput("gdPointData", FindGdPointData);
			if (getView)
			{
				gdPoint = AddValueOutput("gdPoint", FindGdPoint);
			}
		}
		else
		{
			gdPointDataList = AddValueOutput("gdPointDataList", FindGdPointsData);
			if (getView)
			{
				gdPointList = AddValueOutput("gdPointList", FindGdPoints);
			}
		}
		if (getList)
		{
			return;
		}
		gdPointPosition = AddValueOutput("pos", delegate
		{
			if (foundGdPointData != null)
			{
				return foundGdPointData.Position;
			}
			FindGdPointData();
			return (foundGdPointData == null) ? default(Vector3) : foundGdPointData.Position;
		});
	}

	private GDPointData FindGdPointData()
	{
		foundGdPointData = ((!findByCustomTag) ? MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(gdPointId.value) : MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataByCustomTag(gdPointCustomTag.value));
		if (foundGdPointData == null)
		{
			Debug.LogError("[Flow_FindGdPoint]: not found GDPointData by " + GetIdOrCustomTagLog());
		}
		return foundGdPointData;
	}

	private GDPoint FindGdPoint()
	{
		foreach (GameScene loadedGameScene in LazySingleton<GameSceneManager>.Instance.LoadedGameScenes)
		{
			GDPoint[] sceneGdPoints = loadedGameScene.SceneGdPoints;
			foreach (GDPoint gDPoint in sceneGdPoints)
			{
				if (!findByCustomTag)
				{
					if (gDPoint.Id == gdPointId.value)
					{
						return gDPoint;
					}
					if (gDPoint.CustomTag == gdPointCustomTag.value)
					{
						return gDPoint;
					}
				}
			}
		}
		Debug.LogError("[Flow_FindGdPoint]: not found GDPoint by " + GetIdOrCustomTagLog());
		return null;
	}

	private List<GDPointData> FindGdPointsData()
	{
		foundGdPointDataList = ((!findByCustomTag) ? MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointsDataById(gdPointId.value) : MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointsDataByCustomTag(gdPointCustomTag.value));
		if (foundGdPointDataList.Count == 0)
		{
			Debug.LogError("[Flow_FindGdPoint]: not found GDPointsData by " + GetIdOrCustomTagLog());
		}
		return foundGdPointDataList;
	}

	private List<GDPoint> FindGdPoints()
	{
		List<GDPoint> list = new List<GDPoint>();
		foreach (GameScene loadedGameScene in LazySingleton<GameSceneManager>.Instance.LoadedGameScenes)
		{
			GDPoint[] sceneGdPoints = loadedGameScene.SceneGdPoints;
			foreach (GDPoint gDPoint in sceneGdPoints)
			{
				if (!findByCustomTag)
				{
					if (gDPoint.Id == gdPointId.value)
					{
						list.Add(gDPoint);
					}
				}
				else if (gDPoint.CustomTag == gdPointCustomTag.value)
				{
					list.Add(gDPoint);
				}
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("[Flow_FindGdPoint]: not found GDPoints by " + GetIdOrCustomTagLog());
		}
		return list;
	}

	private string GetIdOrCustomTagLog()
	{
		if (!findByCustomTag)
		{
			return "ID: [" + gdPointId.value + "]";
		}
		return "customTag: [" + gdPointCustomTag.value + "]";
	}
}
