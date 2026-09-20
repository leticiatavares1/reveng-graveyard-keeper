using System;
using System.Collections.Generic;
using UnityEngine;

internal class DLCDependentElement : MonoBehaviour
{
	[Serializable]
	private class ConditionAtom
	{
		public DLCEngine.DLCVersion DLCVersion;

		public bool is_enable;
	}

	[Header("Enable if")]
	[SerializeField]
	private List<ConditionAtom> conditions;

	public void Start()
	{
		bool flag = false;
		bool flag2 = true;
		for (int i = 0; i < conditions.Count; i++)
		{
			flag = true;
			flag2 &= DLCEngine.IsDLCAvailable(conditions[i].DLCVersion) == conditions[i].is_enable;
		}
		base.gameObject.SetActive(flag && flag2);
	}
}
