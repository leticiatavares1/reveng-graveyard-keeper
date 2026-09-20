using System.Collections.Generic;
using UnityEngine;

public class MultiObject3DAnimatable : MonoBehaviour
{
	[SerializeField]
	private List<Object3D> object3Ds = new List<Object3D>();

	public List<Object3D> Object3Ds => object3Ds;
}
