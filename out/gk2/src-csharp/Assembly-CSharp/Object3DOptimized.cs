using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Object3DOptimized : MonoBehaviour
{
	[SerializeField]
	public List<GameObject> optimizedChildren = new List<GameObject>();

	[SerializeField]
	public List<GameObject> disabledSourceObjects = new List<GameObject>();

	[SerializeField]
	public List<Component> disabledComponents = new List<Component>();
}
