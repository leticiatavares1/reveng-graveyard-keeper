using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedRtlAlign : MonoBehaviour
{
	[SerializeField]
	[InspectorName("Don't change align on RTL")]
	[Tooltip("If enabled, right-to-left still applies but the horizontal alignment is not flipped.")]
	private bool dontChangeAlignOnRtl = true;

	public bool DontChangeAlignOnRtl => dontChangeAlignOnRtl;
}
