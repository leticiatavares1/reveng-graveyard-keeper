using System;
using UnityEngine;

[Serializable]
public class Object3DSpriteData
{
	public GameObject obj;

	public Sprite sprite;

	public Object3DSpriteData(GameObject obj, Sprite sprite)
	{
		this.obj = obj;
		this.sprite = sprite;
	}
}
