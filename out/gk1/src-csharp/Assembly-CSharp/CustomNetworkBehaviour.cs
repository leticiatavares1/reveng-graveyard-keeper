using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkBehaviour : MonoBehaviour
{
	public delegate void DelegateVoid();

	public delegate void DelegateFloat(float f);

	public delegate void DelegateWGOFloat(WorldGameObject wgo, float f);

	private readonly List<DelegateVoid> _dlg_void = new List<DelegateVoid>();

	private readonly List<DelegateFloat> _dlg_float = new List<DelegateFloat>();

	private readonly List<DelegateWGOFloat> _dlg_wgo_float = new List<DelegateWGOFloat>();

	protected void NetRegisterDelegate(DelegateVoid dlg)
	{
		_dlg_void.Add(dlg);
	}

	protected void NetRegisterDelegate(DelegateFloat dlg)
	{
		_dlg_float.Add(dlg);
	}

	protected void NetRegisterDelegate(DelegateWGOFloat dlg)
	{
		_dlg_wgo_float.Add(dlg);
	}
}
