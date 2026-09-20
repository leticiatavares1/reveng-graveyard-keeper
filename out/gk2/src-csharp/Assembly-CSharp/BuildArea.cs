using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
public class BuildArea : MonoBehaviour
{
	[SerializeField]
	private Collider collider;

	[SerializeField]
	private string id;

	[SerializeField]
	private bool hasRotationRequirement;

	[SerializeField]
	private int rotationIndexRequirement = -1;

	public bool fullCoveringMode;

	public bool ignoreForPointerPlacement;

	public bool foprceShowAsBuffAreaForPointerPlacement;

	public string Id => id;

	public Collider Collider => collider;

	public bool HasRotationRequirement => hasRotationRequirement;

	public int RotationRequirement => rotationIndexRequirement;
}
