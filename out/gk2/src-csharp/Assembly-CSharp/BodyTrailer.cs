using System.Collections.Generic;
using UnityEngine;

public class BodyTrailer : MonoBehaviour
{
	public List<Texture2D> textures = new List<Texture2D>();

	public Object3D bodyObject3D;

	public void RollAndSetTexture()
	{
		bodyObject3D.modelsData[0].texturesData[0].texture = textures.GetRandom();
	}
}
