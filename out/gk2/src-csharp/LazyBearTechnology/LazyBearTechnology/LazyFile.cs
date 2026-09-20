using System.IO;

namespace LazyBearTechnology;

public class LazyFile
{
	private bool isSupportingBackupSaves;

	public bool IsSupportingBackupSaves
	{
		get
		{
			return isSupportingBackupSaves;
		}
		private set
		{
			isSupportingBackupSaves = value;
		}
	}

	public LazyFile()
	{
		IsSupportingBackupSaves = true;
	}

	public void DisableBackupSaving()
	{
		isSupportingBackupSaves = false;
	}

	public bool WriteAllBytes(string path, byte[] bytes)
	{
		File.WriteAllBytes(path, bytes);
		return true;
	}

	public bool WriteAllText(string path, string contents)
	{
		File.WriteAllText(path, contents);
		return true;
	}

	public bool Exists(string path)
	{
		return File.Exists(path);
	}

	public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
	{
		return Directory.GetFiles(path, "*" + searchPattern, searchOption);
	}

	public byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public bool Delete(string path)
	{
		File.Delete(path);
		return true;
	}
}
