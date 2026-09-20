using System.Collections.Generic;
using UnityEngine;

public class Fog : AbstractControllerComponent
{
	public static Fog me;

	private List<FogObject> _objs = new List<FogObject>();

	private float _cur_amount;

	private float _target_amount;

	private Material _mat;

	private GameObject _sub_go;

	public float speed = 0.2f;

	public static Transform SpawnNewFog()
	{
		GameObject gameObject = new GameObject("Fog");
		me = gameObject.AddComponent<Fog>();
		gameObject.transform.SetParent(MainGame.me.world_root.transform, worldPositionStays: false);
		me._sub_go = new GameObject("fog objects");
		me._sub_go.transform.SetParent(gameObject.transform, worldPositionStays: false);
		return me._sub_go.transform;
	}

	public void OnNewFogObjectCreated(FogObject fo)
	{
		if (_objs.Count == 0)
		{
			_mat = fo.GetComponent<SpriteRenderer>().sharedMaterial;
			ApplyCurrentAmount();
		}
		_objs.Add(fo);
	}

	public void Update()
	{
		_cur_amount = _target_amount;
		ApplyCurrentAmount();
	}

	private void ApplyCurrentAmount()
	{
		_sub_go.SetActive((double)_cur_amount > 0.01);
		_mat.SetFloat("_Multiply", _cur_amount);
	}

	public override void Set(float a)
	{
		_target_amount = a;
	}
}
