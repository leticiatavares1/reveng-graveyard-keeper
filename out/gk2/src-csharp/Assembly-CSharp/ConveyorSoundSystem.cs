using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ConveyorSoundSystem : LazySingleton<ConveyorSoundSystem>
{
	[Serializable]
	private class ClusterDebugInfo
	{
		public string soundId;

		public int memberCount;

		public float mass;

		public float volume;

		public bool isPlaying;

		public Vector3 proxyPosition;

		public Vector3 targetPosition;
	}

	private class ClusterVoice
	{
		public string soundId;

		public Transform proxy;

		public SoundHandler handler;

		public Vector3 targetPosition;

		public float mass;

		public List<ConveyorPlaySoundOnEnable> members;
	}

	private class ComputedCluster
	{
		public string soundId;

		public Vector3 centroid;

		public float mass;

		public List<ConveyorPlaySoundOnEnable> members;
	}

	[SerializeField]
	private float clusterRadius = 3f;

	[SerializeField]
	private float recomputeInterval = 0.15f;

	[SerializeField]
	private int maxActiveClusters = 8;

	[SerializeField]
	private float centroidFollowSpeed = 10f;

	[SerializeField]
	private float massDistanceFloor = 1f;

	[SerializeField]
	private float evictionMassMargin = 1.5f;

	private readonly Dictionary<string, List<ConveyorPlaySoundOnEnable>> registeredSources = new Dictionary<string, List<ConveyorPlaySoundOnEnable>>();

	private readonly List<ClusterVoice> activeVoices = new List<ClusterVoice>();

	private readonly Stack<Transform> freeProxies = new Stack<Transform>();

	private float recomputeTimer;

	private bool phaseSoundsUnpaused;

	[SerializeField]
	private bool showDebugGizmos = true;

	[SerializeField]
	private int registeredSourceCount;

	[SerializeField]
	private int activeVoiceCount;

	[SerializeField]
	private int freeProxyCount;

	[SerializeField]
	private List<ClusterDebugInfo> activeClusterDebug = new List<ClusterDebugInfo>();

	public void Register(ConveyorPlaySoundOnEnable source)
	{
		if (source == null)
		{
			return;
		}
		string soundId = source.SoundId;
		if (!string.IsNullOrEmpty(soundId))
		{
			if (!registeredSources.TryGetValue(soundId, out var value))
			{
				value = new List<ConveyorPlaySoundOnEnable>();
				registeredSources[soundId] = value;
			}
			if (!value.Contains(source))
			{
				value.Add(source);
			}
		}
	}

	public void Unregister(ConveyorPlaySoundOnEnable source)
	{
		if (source == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<ConveyorPlaySoundOnEnable>> registeredSource in registeredSources)
		{
			registeredSource.Value.Remove(source);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InitializeProxyPool();
	}

	private void InitializeProxyPool()
	{
		for (int i = 0; i < maxActiveClusters; i++)
		{
			GameObject gameObject = new GameObject($"ConveyorSoundProxy_{i}");
			gameObject.transform.SetParent(base.transform);
			freeProxies.Push(gameObject.transform);
		}
	}

	private void Update()
	{
		float maxDistanceDelta = centroidFollowSpeed * Time.deltaTime;
		foreach (ClusterVoice activeVoice in activeVoices)
		{
			if (!(activeVoice.proxy == null))
			{
				activeVoice.proxy.position = Vector3.MoveTowards(activeVoice.proxy.position, activeVoice.targetPosition, maxDistanceDelta);
			}
		}
		recomputeTimer += Time.deltaTime;
		if (recomputeTimer >= recomputeInterval)
		{
			recomputeTimer = 0f;
			RecomputeClusters();
		}
		UpdateDebugInfo();
	}

	public void PlaySounds()
	{
		RecomputeClusters();
		phaseSoundsUnpaused = true;
		foreach (ClusterVoice activeVoice in activeVoices)
		{
			activeVoice.handler?.UnPauseIfActive();
		}
		recomputeTimer = 0f;
	}

	public void PauseSounds()
	{
		phaseSoundsUnpaused = false;
		RecomputeClusters();
		foreach (ClusterVoice activeVoice in activeVoices)
		{
			activeVoice.handler?.PauseIfActive();
		}
		recomputeTimer = 0f;
	}

	private void RecomputeClusters()
	{
		Vector3 position = LazyAudio.Microphone.position;
		List<ComputedCluster> list = new List<ComputedCluster>();
		foreach (KeyValuePair<string, List<ConveyorPlaySoundOnEnable>> registeredSource in registeredSources)
		{
			string key = registeredSource.Key;
			List<ConveyorPlaySoundOnEnable> value = registeredSource.Value;
			value.RemoveAll((ConveyorPlaySoundOnEnable source) => source == null || !source.isActiveAndEnabled);
			if (value.Count == 0)
			{
				continue;
			}
			float baseVolume = LazySingletonSO<AudioConfig>.Instance.Get(key)?.volume ?? 1f;
			foreach (List<ConveyorPlaySoundOnEnable> item in ClusterByProximity(value, clusterRadius))
			{
				list.Add(ComputeCluster(key, item, position, baseVolume));
			}
		}
		list.Sort((ComputedCluster a, ComputedCluster b) => b.mass.CompareTo(a.mass));
		HashSet<ClusterVoice> hashSet = new HashSet<ClusterVoice>();
		foreach (ComputedCluster item2 in list)
		{
			ClusterVoice clusterVoice = null;
			float num = float.MaxValue;
			foreach (ClusterVoice activeVoice in activeVoices)
			{
				if (!hashSet.Contains(activeVoice) && !(activeVoice.soundId != item2.soundId))
				{
					float sqrMagnitude = (activeVoice.targetPosition - item2.centroid).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						clusterVoice = activeVoice;
					}
				}
			}
			if (clusterVoice != null)
			{
				hashSet.Add(clusterVoice);
				UpdateVoice(clusterVoice, item2);
			}
			else
			{
				TryAssignVoice(item2);
			}
		}
		for (int num2 = activeVoices.Count - 1; num2 >= 0; num2--)
		{
			if (!hashSet.Contains(activeVoices[num2]))
			{
				ReleaseVoice(activeVoices[num2]);
				activeVoices.RemoveAt(num2);
			}
		}
	}

	private static List<List<ConveyorPlaySoundOnEnable>> ClusterByProximity(List<ConveyorPlaySoundOnEnable> sources, float radius)
	{
		HashSet<ConveyorPlaySoundOnEnable> hashSet = new HashSet<ConveyorPlaySoundOnEnable>(sources);
		List<List<ConveyorPlaySoundOnEnable>> list = new List<List<ConveyorPlaySoundOnEnable>>();
		float num = radius * radius;
		while (hashSet.Count > 0)
		{
			List<ConveyorPlaySoundOnEnable> list2 = new List<ConveyorPlaySoundOnEnable>();
			Queue<ConveyorPlaySoundOnEnable> queue = new Queue<ConveyorPlaySoundOnEnable>();
			ConveyorPlaySoundOnEnable item = null;
			using (HashSet<ConveyorPlaySoundOnEnable>.Enumerator enumerator = hashSet.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					item = enumerator.Current;
				}
			}
			hashSet.Remove(item);
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				ConveyorPlaySoundOnEnable conveyorPlaySoundOnEnable = queue.Dequeue();
				list2.Add(conveyorPlaySoundOnEnable);
				List<ConveyorPlaySoundOnEnable> list3 = null;
				foreach (ConveyorPlaySoundOnEnable item2 in hashSet)
				{
					if (!((conveyorPlaySoundOnEnable.transform.position - item2.transform.position).sqrMagnitude > num))
					{
						if (list3 == null)
						{
							list3 = new List<ConveyorPlaySoundOnEnable>();
						}
						list3.Add(item2);
					}
				}
				if (list3 == null)
				{
					continue;
				}
				foreach (ConveyorPlaySoundOnEnable item3 in list3)
				{
					hashSet.Remove(item3);
					queue.Enqueue(item3);
				}
			}
			list.Add(list2);
		}
		return list;
	}

	private ComputedCluster ComputeCluster(string soundId, List<ConveyorPlaySoundOnEnable> members, Vector3 listenerPosition, float baseVolume)
	{
		Vector3 zero = Vector3.zero;
		float num = 0f;
		foreach (ConveyorPlaySoundOnEnable member in members)
		{
			float a = Vector3.Distance(listenerPosition, member.transform.position);
			float num2 = baseVolume * member.MassMultiplier / Mathf.Max(a, massDistanceFloor);
			zero += member.transform.position * num2;
			num += num2;
		}
		Vector3 centroid = ((num > 0f) ? (zero / num) : members[0].transform.position);
		return new ComputedCluster
		{
			soundId = soundId,
			centroid = centroid,
			mass = num,
			members = members
		};
	}

	private void TryAssignVoice(ComputedCluster cluster)
	{
		ClusterVoice clusterVoice = null;
		if (freeProxies.Count > 0)
		{
			clusterVoice = CreateVoiceFromPool(cluster.soundId);
		}
		else if (activeVoices.Count >= maxActiveClusters)
		{
			ClusterVoice clusterVoice2 = null;
			float num = float.MaxValue;
			foreach (ClusterVoice activeVoice in activeVoices)
			{
				if (!(activeVoice.mass >= num))
				{
					num = activeVoice.mass;
					clusterVoice2 = activeVoice;
				}
			}
			if (clusterVoice2 != null && cluster.mass > num * evictionMassMargin)
			{
				ReleaseVoice(clusterVoice2);
				activeVoices.Remove(clusterVoice2);
				clusterVoice = CreateVoiceFromPool(cluster.soundId);
			}
		}
		if (clusterVoice != null)
		{
			activeVoices.Add(clusterVoice);
			StartVoice(clusterVoice, cluster);
		}
	}

	private ClusterVoice CreateVoiceFromPool(string soundId)
	{
		return new ClusterVoice
		{
			soundId = soundId,
			proxy = freeProxies.Pop(),
			members = new List<ConveyorPlaySoundOnEnable>()
		};
	}

	private void StartVoice(ClusterVoice voice, ComputedCluster cluster)
	{
		voice.targetPosition = cluster.centroid;
		voice.mass = cluster.mass;
		voice.members = cluster.members;
		voice.proxy.position = cluster.centroid;
		voice.handler = LazyAudio.PlayAtGameObject(cluster.soundId, voice.proxy, SpatialType.sound3D, checkDelay: false);
		voice.handler?.SetVolume(Mathf.Clamp01(cluster.mass));
		voice.handler?.PauseIfActive();
	}

	private void UpdateVoice(ClusterVoice voice, ComputedCluster cluster)
	{
		voice.targetPosition = cluster.centroid;
		voice.mass = cluster.mass;
		voice.members = cluster.members;
		bool num = !phaseSoundsUnpaused;
		if (num)
		{
			if (voice.handler == null || !voice.handler.IsActive || !voice.handler.IsPaused)
			{
				voice.handler?.Stop();
				voice.handler = LazyAudio.PlayAtGameObject(cluster.soundId, voice.proxy, SpatialType.sound3D, checkDelay: false);
			}
		}
		else if (IsLoopingSound(cluster.soundId) && (voice.handler == null || !voice.handler.IsActive))
		{
			voice.handler = LazyAudio.PlayAtGameObject(cluster.soundId, voice.proxy, SpatialType.sound3D, checkDelay: false);
		}
		voice.handler?.SetVolume(Mathf.Clamp01(cluster.mass));
		if (num)
		{
			voice.handler?.PauseIfActive();
		}
	}

	private static bool IsLoopingSound(string soundId)
	{
		return LazySingletonSO<AudioConfig>.Instance.Get(soundId)?.loop ?? false;
	}

	private void ReleaseVoice(ClusterVoice voice)
	{
		voice.handler?.Stop();
		voice.handler = null;
		voice.members?.Clear();
		voice.mass = 0f;
		if (voice.proxy != null)
		{
			freeProxies.Push(voice.proxy);
		}
	}

	private void UpdateDebugInfo()
	{
		registeredSourceCount = 0;
		foreach (KeyValuePair<string, List<ConveyorPlaySoundOnEnable>> registeredSource in registeredSources)
		{
			registeredSource.Value.RemoveAll((ConveyorPlaySoundOnEnable source) => source == null);
			registeredSourceCount += registeredSource.Value.Count;
		}
		activeVoiceCount = activeVoices.Count;
		freeProxyCount = freeProxies.Count;
		activeClusterDebug.Clear();
		foreach (ClusterVoice activeVoice in activeVoices)
		{
			activeClusterDebug.Add(new ClusterDebugInfo
			{
				soundId = activeVoice.soundId,
				memberCount = (activeVoice.members?.Count ?? 0),
				mass = activeVoice.mass,
				volume = Mathf.Clamp01(activeVoice.mass),
				isPlaying = (activeVoice.handler != null && activeVoice.handler.IsActive),
				proxyPosition = ((activeVoice.proxy != null) ? activeVoice.proxy.position : activeVoice.targetPosition),
				targetPosition = activeVoice.targetPosition
			});
		}
	}

	private void OnDestroy()
	{
		foreach (ClusterVoice activeVoice in activeVoices)
		{
			activeVoice.handler?.Stop();
		}
		activeVoices.Clear();
		while (freeProxies.Count > 0)
		{
			Transform transform = freeProxies.Pop();
			if (transform != null)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}
}
