using UnityEngine;

namespace LazyBearTechnology;

public interface ILazyUIElementWithId
{
	string LazyUIElementId { get; set; }

	MonoBehaviour MonoBehaviour { get; }
}
