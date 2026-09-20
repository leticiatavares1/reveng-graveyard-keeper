using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LazyBearTechnology;

public static class ModsPaths
{
	public const string ModsFolderName = "Mods";

	public const string LanguagesFolderName = "Languages";

	public const string VoiceOversFolderName = "VoiceOvers";

	public const string ExampleFolderName = "~Example";

	public const string ReadmeFileName = "README.txt";

	public const string LanguageJsonFileName = "language.json";

	public const string StringsCsvFileName = "strings.csv";

	public const string MissingLinesCsvFileName = "_missing_lines.csv";

	public const string VoiceOverManifestFileName = "mod.json";

	public const string WorkshopConfigFileName = "workshop.json";

	public static string Root => Path.Combine(Application.persistentDataPath, "Mods");

	public static string LanguagesRoot => Path.Combine(Root, "Languages");

	public static string VoiceOversRoot => Path.Combine(Root, "VoiceOvers");

	public static string ExampleLanguageFolder => Path.Combine(LanguagesRoot, "~Example");

	public static string WorkshopConfigPath => Path.Combine(Root, "workshop.json");

	public static string GetGameExecutableDirectory()
	{
		return Path.GetDirectoryName(Application.dataPath);
	}

	public static IReadOnlyList<string> GetModSearchRoots()
	{
		List<string> list = new List<string>();
		AddRoot(list, Root);
		AddRoot(list, GetGameExecutableDirectory());
		IReadOnlyList<string> installedFolders = SteamWorkshopInstalledItems.GetInstalledFolders();
		for (int i = 0; i < installedFolders.Count; i++)
		{
			AddRoot(list, installedFolders[i]);
		}
		return list;
	}

	public static bool IsDisabledFolderName(string folderName)
	{
		if (!string.IsNullOrEmpty(folderName))
		{
			return folderName[0] == '~';
		}
		return false;
	}

	public static bool IsEditableModPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		if (!IsUnder(path, Root))
		{
			return IsUnder(path, GetGameExecutableDirectory());
		}
		return true;
	}

	private static void AddRoot(List<string> roots, string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}
		try
		{
			path = Path.GetFullPath(path);
		}
		catch
		{
			return;
		}
		if (!Directory.Exists(path))
		{
			return;
		}
		for (int i = 0; i < roots.Count; i++)
		{
			if (string.Equals(roots[i], path, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		roots.Add(path);
	}

	private static bool IsUnder(string path, string root)
	{
		if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root))
		{
			return false;
		}
		try
		{
			string text = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string text2 = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (string.Equals(text, text2, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string value = text2 + directorySeparatorChar;
			return text.StartsWith(value, StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}
}
