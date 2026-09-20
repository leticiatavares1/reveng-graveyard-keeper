using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/SaveFixer/Save Fix", fileName = "1.0")]
public class SaveFixData : ScriptableObject
{
	public const string AddressablesFolder = "Assets/AddressableAssets/SaveFixerAssets";

	public const string AddressablesLabel = "SaveFixerAssets";

	public const string AddressablesGroup = "SaveFixerAssets";

	[SerializeField]
	private string version;

	[SerializeReference]
	private List<SaveFixOperation> operations = new List<SaveFixOperation>();

	public string Version => version;

	public IReadOnlyList<SaveFixOperation> Operations => operations;

	public bool TryGetVersion(out GameSaveVersion parsed)
	{
		if (GameSaveVersion.TryParse(version, out parsed))
		{
			return true;
		}
		return GameSaveVersion.TryParse(base.name, out parsed);
	}

	public void AddOperation(SaveFixOperation operation)
	{
		if (operations == null)
		{
			operations = new List<SaveFixOperation>();
		}
		operations.Add(operation);
	}

	public bool ContainsWgoOperationFor(SGuid uniqueId)
	{
		if (SGuid.IsNullOrEmpty(uniqueId) || operations == null)
		{
			return false;
		}
		foreach (SaveFixOperation operation in operations)
		{
			if (operation is SaveFixWgoOperation { OccupiesUniqueId: not false } saveFixWgoOperation && saveFixWgoOperation.ContainsUniqueId(uniqueId))
			{
				return true;
			}
		}
		return false;
	}

	public T FindOperation<T>() where T : SaveFixOperation
	{
		if (operations == null)
		{
			return null;
		}
		foreach (SaveFixOperation operation in operations)
		{
			if (operation is T result)
			{
				return result;
			}
		}
		return null;
	}
}
