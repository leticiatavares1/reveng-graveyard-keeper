using System.Globalization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Collider))]
public class TownSoundZone : MonoBehaviour
{
	[SerializeField]
	private AssetReferenceT<SoundEnvironmentConfig> configRef;

	[SerializeField]
	private string soundPostfixPlayerGameRes;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float switchTime = 5f;

	[SerializeField]
	[Range(0f, 2f)]
	private float exitGraceTime = 0.4f;

	private readonly AmbientSoundMixer mixer = new AmbientSoundMixer();

	private AsyncOperationHandle<SoundEnvironmentConfig> configHandle;

	private SoundEnvironmentConfig config;

	private int insideCount;

	private bool isActive;

	private float exitTimer = -1f;

	private float currentTimeOfDay;

	private int currentGameResValue;

	private string cachedBaseId1;

	private string cachedBaseId2;

	private string cachedVariantId1;

	private string cachedVariantId2;

	private void Awake()
	{
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			component.isTrigger = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<ISoundZoneRecognizable>() != null)
		{
			insideCount++;
			if (exitTimer >= 0f)
			{
				exitTimer = -1f;
			}
			else if (insideCount == 1)
			{
				EnterZone();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<ISoundZoneRecognizable>() != null)
		{
			insideCount = Mathf.Max(0, insideCount - 1);
			if (insideCount == 0 && isActive)
			{
				exitTimer = exitGraceTime;
			}
		}
	}

	private void Update()
	{
		if (!isActive)
		{
			return;
		}
		if (exitTimer >= 0f)
		{
			exitTimer -= Time.deltaTime;
			if (exitTimer < 0f)
			{
				exitTimer = -1f;
				ExitZone();
				return;
			}
		}
		mixer.Update(Time.deltaTime);
	}

	private void OnDestroy()
	{
		if (isActive)
		{
			ExitZone();
		}
	}

	private void EnterZone()
	{
		config = AddressableUtils.LoadAssetReferenceSync(configRef, ref configHandle);
		if (config == null)
		{
			Debug.LogWarning("TownSoundZone [" + base.name + "]: failed to load SoundEnvironmentConfig");
			return;
		}
		isActive = true;
		exitTimer = -1f;
		mixer.SwitchDuration = switchTime;
		EnvironmentEngine.OnTimeOfDayChangedEvent += OnTimeOfDayChanged;
		if (MainGame.PlayerData != null)
		{
			MainGame.PlayerData.OnGameResChanged += OnGameResChanged;
		}
		currentGameResValue = ComputeGameResValue();
		float timeOfDay = ((EnvironmentEngine.Instance != null) ? EnvironmentEngine.Instance.timeOfDay : 0f);
		ApplyPair(timeOfDay);
	}

	private void ExitZone()
	{
		if (isActive)
		{
			isActive = false;
			EnvironmentEngine.OnTimeOfDayChangedEvent -= OnTimeOfDayChanged;
			if (MainGame.PlayerData != null)
			{
				MainGame.PlayerData.OnGameResChanged -= OnGameResChanged;
			}
			mixer.StopAll();
			currentGameResValue = 0;
			cachedBaseId1 = (cachedBaseId2 = null);
			cachedVariantId1 = (cachedVariantId2 = null);
			config = null;
			AddressableUtils.ReleaseAssetReference(ref configHandle);
		}
	}

	private void OnTimeOfDayChanged(float timeOfDay, bool isFake)
	{
		ApplyPair(timeOfDay);
	}

	private void OnGameResChanged()
	{
		int num = ComputeGameResValue();
		if (num != currentGameResValue)
		{
			currentGameResValue = num;
			cachedBaseId1 = (cachedBaseId2 = null);
			ApplyPair(currentTimeOfDay);
		}
	}

	private int ComputeGameResValue()
	{
		if (MainGame.PlayerData == null || string.IsNullOrEmpty(soundPostfixPlayerGameRes))
		{
			return 0;
		}
		return (int)MainGame.PlayerData.GetRes(soundPostfixPlayerGameRes);
	}

	private void ApplyPair(float timeOfDay)
	{
		currentTimeOfDay = timeOfDay;
		if (!(config == null))
		{
			float crossfadeDuration = ((EnvironmentEngine.Instance != null) ? config.GetCrossfadeDuration01(EnvironmentEngine.Instance.gameplayDayInMinutes * 60f) : 0f);
			SoundEnvironmentConfig.FindPair(config.sounds, timeOfDay, crossfadeDuration, out var id, out var id2, out var lerp);
			string id3 = WithVariant(id, ref cachedBaseId1, ref cachedVariantId1);
			string id4 = WithVariant(id2, ref cachedBaseId2, ref cachedVariantId2);
			mixer.SetAmbientPair(id3, id4, lerp);
		}
	}

	private string WithVariant(string baseId, ref string cachedBase, ref string cachedVariant)
	{
		if (string.IsNullOrEmpty(baseId) || currentGameResValue <= 0)
		{
			return baseId;
		}
		if (baseId == cachedBase && cachedVariant != null)
		{
			return cachedVariant;
		}
		cachedBase = baseId;
		cachedVariant = baseId + "_" + currentGameResValue.ToString(CultureInfo.InvariantCulture);
		return cachedVariant;
	}
}
