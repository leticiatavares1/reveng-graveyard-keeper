using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Playables;

public class RiverTrailerHelper : MonoBehaviour
{
	public PlayableDirector director;

	public GameObject cameraFollowGo;

	public AreaBodiesSpawner bodiesSpawner;

	public BoxCollider chunkIgnoreCollider;

	public GameObject singleFlowingBody;

	[Range(0f, 1f)]
	public float timeOfDayForAnim = 0.3f;

	public string timeOfDayPresetName = "outdoor";

	private bool areChunksIgnored;

	private HashSet<IChunkableObject> AllStaticObjectsInZone { get; set; }

	public void SetCustomCamaraFollowTarget()
	{
		CameraSystem.Instance.ActiveCameraController.SetTarget(cameraFollowGo.transform);
	}

	public void ResetCustomCamaraFollowTarget()
	{
		CameraSystem.Instance.ActiveCameraController.SetTarget(CameraSystem.Instance.GroundPointTransform);
	}

	public void MakePreparationsOnAnimStart()
	{
		EnvironmentEngine environmentEngine = MainGame.Instance.GameSave.environmentData.EnvironmentEngine;
		environmentEngine.SetTimeOfDay(Mathf.Clamp(timeOfDayForAnim, 0f, 1f));
		environmentEngine.IsPaused = true;
		GUIElements.Instance.SetVisibilityState(isVisible: false);
	}

	public void SpawnBodies()
	{
		if (!areChunksIgnored)
		{
			areChunksIgnored = true;
			SetNotIgnoredStateForChunkableObjects();
		}
		bodiesSpawner.SpawnBodies();
	}

	public void ClearBodies()
	{
		bodiesSpawner.ClearBodies();
	}

	public void Play()
	{
		director.Play();
	}

	public void ResetBodiesPos()
	{
		if ((bool)bodiesSpawner)
		{
			bodiesSpawner.ResetBodiesPos(true);
		}
	}

	public void StartRiverFlowTransform()
	{
		if ((bool)bodiesSpawner)
		{
			bodiesSpawner.StartFlowTransform();
		}
	}

	public void StartRiverFlowPhysics()
	{
		if ((bool)bodiesSpawner)
		{
			bodiesSpawner.StartFlow();
		}
	}

	public void AddSingleBodyToOthersForFlow()
	{
		bodiesSpawner.AddBody(singleFlowingBody);
	}

	private void SetNotIgnoredStateForChunkableObjects()
	{
		AllStaticObjectsInZone = new HashSet<IChunkableObject>();
		foreach (IChunkableObject allChunkableObjectsInBoundsForSelectedLayer in LazySingleton<ChunkManager>.Instance.GetAllChunkableObjectsInBoundsForSelectedLayers(new List<ChunkManagerLayerType> { ChunkManagerLayerType.StaticObjects }, chunkIgnoreCollider.bounds))
		{
			AllStaticObjectsInZone.Add(allChunkableObjectsInBoundsForSelectedLayer);
			allChunkableObjectsInBoundsForSelectedLayer.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
		}
	}

	private void Start()
	{
		EnvironmentEngine.Instance.SetTimeOfDayPreset(timeOfDayPresetName);
	}
}
