using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class FightEffectsManager : MonoBehaviour
{
	[Header("Decals")]
	public GroundDecalsCollection bloodDecalsCollection;

	public GroundDecalsCollection bonesDecalsCollection;

	public GroundDecalsCollection gutsDecalsCollection;

	[Header("Paddles")]
	public BloodPaddle bloodPaddlePrefab;

	public Transform bloodPaddlesParent;

	private Pool bloodPaddlePool;

	private List<BloodPaddle> activePaddles = new List<BloodPaddle>();

	private RaycastHit[] hits = new RaycastHit[10];

	private SortedDecals sortedDecals = new SortedDecals();

	public void SpawnBloodPaddle(Vector3 position, Direction orientation)
	{
		int num = Physics.RaycastNonAlloc(new Ray(position + Vector3.up, Vector3.down), hits, 2f, 9, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hits[i];
			Collider collider = raycastHit.collider;
			if (((object)collider == null || collider.gameObject.layer != 8) && (bool)raycastHit.collider?.GetComponentInParent<BloodPaddle>())
			{
				return;
			}
		}
		BloodPaddle orCreateObject = bloodPaddlePool.GetOrCreateObject<BloodPaddle>();
		orCreateObject.SpawnDecal(position, orientation);
		activePaddles.Add(orCreateObject);
	}

	private void Awake()
	{
		bloodPaddlePool = new Pool(bloodPaddlePrefab, bloodPaddlesParent, 5);
		bloodPaddlePrefab.gameObject.SetActive(value: false);
		bloodDecalsCollection.sortedDecals = sortedDecals;
		bonesDecalsCollection.sortedDecals = sortedDecals;
		gutsDecalsCollection.sortedDecals = sortedDecals;
		bloodDecalsCollection.SetLayerBand(DecalLayerBand.Blood);
		bonesDecalsCollection.SetLayerBand(DecalLayerBand.Gore);
		gutsDecalsCollection.SetLayerBand(DecalLayerBand.Guts);
	}

	private void Update()
	{
		if (MainGame.IsGamePaused)
		{
			return;
		}
		bloodDecalsCollection.UpdateLifeTime(Time.deltaTime);
		bonesDecalsCollection.UpdateLifeTime(Time.deltaTime);
		gutsDecalsCollection.UpdateLifeTime(Time.deltaTime);
		for (int i = 0; i < activePaddles.Count; i++)
		{
			BloodPaddle bloodPaddle = activePaddles[i];
			bloodPaddle.Lifetime += Time.deltaTime;
			if (bloodPaddle.Lifetime > bloodPaddle.TimeToLive)
			{
				bloodPaddlePool.ReleaseObject(bloodPaddle);
				activePaddles.RemoveAt(i);
				i--;
			}
		}
	}
}
