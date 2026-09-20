using UnityEngine;

[ExecuteInEditMode]
public class RopeRenderer : MonoBehaviour
{
	[Header("Rope Settings")]
	[SerializeField]
	[Range(0f, 1f)]
	public float looseness = 0.5f;

	[SerializeField]
	[Range(0.001f, 0.1f)]
	private float ropeThickness = 0.02f;

	[SerializeField]
	public Color ropeColor = Color.white;

	[SerializeField]
	[Range(0f, 5f)]
	private float emissionIntensity = 0.3f;

	[SerializeField]
	[Range(1f, 10f)]
	private float gravityStrength = 2f;

	[Header("Rope Points")]
	[SerializeField]
	private RopePoint startPoint;

	[SerializeField]
	private RopePoint endPoint;

	[Header("Quality Settings")]
	[SerializeField]
	[Range(64f, 512f)]
	private int textureResolution = 256;

	[SerializeField]
	private bool antiAliasing = true;

	[Header("Mesh View")]
	[SerializeField]
	private RopeMeshView meshView;

	private float ropeLength;

	private Vector3 lastStartPos;

	private Vector3 lastEndPos;

	private bool needsUpdate = true;

	public RopePoint StartPoint
	{
		get
		{
			return startPoint;
		}
		set
		{
			startPoint = value;
		}
	}

	public RopePoint EndPoint
	{
		get
		{
			return endPoint;
		}
		set
		{
			endPoint = value;
		}
	}

	public float Looseness
	{
		get
		{
			return looseness;
		}
		set
		{
			looseness = value;
		}
	}

	private void Awake()
	{
		InitializeComponents();
	}

	private void OnEnable()
	{
		ValidateRopePoints();
		UpdateRopeParameters();
	}

	private void LateUpdate()
	{
		CheckForPositionChanges();
		if (needsUpdate)
		{
			UpdateRopeParameters();
			needsUpdate = false;
		}
	}

	private void InitializeComponents()
	{
		if (meshView == null)
		{
			meshView = GetComponentInChildren<RopeMeshView>(includeInactive: true);
		}
		if (meshView != null)
		{
			meshView.EnsureInitialized();
		}
	}

	private void ValidateRopePoints()
	{
		if (startPoint == null)
		{
			startPoint = GetComponentInChildren<RopePoint>(includeInactive: true);
			if (startPoint != null)
			{
				Debug.LogWarning("Start point was null, automatically assigned first found RopePoint child.");
			}
		}
		if (endPoint == null)
		{
			RopePoint[] componentsInChildren = GetComponentsInChildren<RopePoint>(includeInactive: true);
			if (componentsInChildren.Length > 1)
			{
				endPoint = componentsInChildren[1];
				Debug.LogWarning("End point was null, automatically assigned second found RopePoint child.");
			}
		}
		if (startPoint == null || endPoint == null)
		{
			Debug.LogError("RopeRenderer requires two RopePoint child objects!");
		}
	}

	private void CheckForPositionChanges()
	{
		if (!(startPoint == null) && !(endPoint == null))
		{
			Vector3 localPosition = startPoint.transform.localPosition;
			Vector3 localPosition2 = endPoint.transform.localPosition;
			if (Vector3.Distance(localPosition, lastStartPos) > 0.001f || Vector3.Distance(localPosition2, lastEndPos) > 0.001f)
			{
				lastStartPos = localPosition;
				lastEndPos = localPosition2;
				needsUpdate = true;
			}
		}
	}

	private void UpdateRopeParameters()
	{
		if (!(meshView == null) && !(startPoint == null) && !(endPoint == null))
		{
			Vector3 localPosition = startPoint.transform.localPosition;
			Vector3 vector = endPoint.transform.localPosition - localPosition;
			Vector3 localEndPoint = vector;
			meshView.AlignToEndPoint(localEndPoint);
			float magnitude = new Vector2(vector.x, vector.z).magnitude;
			ropeLength = new Vector2(magnitude, vector.y).magnitude;
			Vector3 meshScale = meshView.MeshScale;
			Vector2 zero = Vector2.zero;
			Vector2 endPos2D = new Vector2((0f - magnitude) / meshScale.x, vector.y / meshScale.y) * 2f;
			meshView.UpdateRopeVisuals(zero, endPos2D, looseness, ropeThickness, ropeColor, emissionIntensity, gravityStrength, textureResolution, antiAliasing);
		}
	}

	public void SetLooseness(float value)
	{
		looseness = Mathf.Clamp01(value);
		needsUpdate = true;
	}

	public void SetThickness(float value)
	{
		ropeThickness = Mathf.Clamp(value, 0.001f, 0.1f);
		needsUpdate = true;
	}

	public void SetColor(Color color)
	{
		ropeColor = color;
		needsUpdate = true;
	}

	public void SetEmission(float intensity)
	{
		emissionIntensity = Mathf.Clamp(intensity, 0f, 5f);
		needsUpdate = true;
	}

	public void SetGravityStrength(float value)
	{
		gravityStrength = Mathf.Clamp(value, 1f, 10f);
		needsUpdate = true;
	}

	private void OnValidate()
	{
		needsUpdate = true;
	}

	private void OnRopeParameterChanged()
	{
		if (meshView != null)
		{
			meshView.EnsureInitialized();
			meshView.UpdateRopeParameters(looseness, ropeThickness, ropeColor, emissionIntensity, gravityStrength);
			needsUpdate = true;
		}
	}

	private void OnRopePointChanged()
	{
		needsUpdate = true;
		if (Application.isPlaying)
		{
			ValidateRopePoints();
		}
	}

	private void OnQualityParameterChanged()
	{
		if (meshView != null)
		{
			meshView.EnsureInitialized();
			meshView.UpdateQualitySettings(textureResolution, antiAliasing);
		}
	}
}
