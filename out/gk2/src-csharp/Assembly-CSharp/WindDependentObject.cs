using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class WindDependentObject : MonoBehaviour
{
	[SerializeField]
	private Material windClothMaterial;

	[SerializeField]
	private float lastWindValue;

	[SerializeField]
	public float WindModificator = 1f;

	private static readonly int WindStrengthProperty = Shader.PropertyToID("_WindIntensity");

	private WeatherSystem weatherSystem;

	private MeshRenderer meshRenderer;

	private void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer != null)
		{
			windClothMaterial = meshRenderer.material;
		}
		if (windClothMaterial == null)
		{
			Debug.LogError("[WindDependentObject]: Material is null", this);
		}
	}

	private void Start()
	{
		weatherSystem = WeatherSystem.Instance;
		lastWindValue = weatherSystem.WindValue;
	}

	private void Update()
	{
		if (!lastWindValue.EqualsTo(weatherSystem.WindValue))
		{
			lastWindValue = weatherSystem.WindValue;
			if (windClothMaterial != null)
			{
				windClothMaterial.SetFloat(WindStrengthProperty, lastWindValue * WindModificator);
				meshRenderer.material = windClothMaterial;
			}
		}
	}
}
