using System;
using System.Diagnostics;
using UnityEngine;

public static class GitTools
{
	public static string RunGitCommand(string gitCommand)
	{
		string text = "no-git";
		string text2 = "no-git";
		ProcessStartInfo startInfo = new ProcessStartInfo("git", gitCommand)
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		Process process = new Process
		{
			StartInfo = startInfo
		};
		try
		{
			process.Start();
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Git is not set-up correctly, required to be on PATH, and to be a git project.");
			throw ex;
		}
		text = process.StandardOutput.ReadToEnd();
		text2 = process.StandardError.ReadToEnd();
		process.WaitForExit();
		process.Close();
		if (text.Contains("fatal") || text == "no-git" || text == "")
		{
			throw new Exception("Command: git " + gitCommand + " Failed\n" + text + text2);
		}
		if (text2 != "")
		{
			UnityEngine.Debug.LogError("Git Error: " + text2);
		}
		return text;
	}

	public static string GetCurrentCommitShorthash()
	{
		string text = RunGitCommand("rev-parse --short --verify HEAD");
		return string.Join("", text.Split((string[])null, StringSplitOptions.RemoveEmptyEntries));
	}
}
