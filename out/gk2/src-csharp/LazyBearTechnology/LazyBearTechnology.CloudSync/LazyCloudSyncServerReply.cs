using UnityEngine;

namespace LazyBearTechnology.CloudSync;

internal class LazyCloudSyncServerReply
{
	[SerializeField]
	private string debugRequest;

	public string result;

	[SerializeField]
	private int auth;

	public int overwrite;

	public string query;

	public string error;

	public string serverId;

	public string password;

	public string code;

	public string file;

	public int lifetime;

	public bool IsError => !string.IsNullOrEmpty(error);

	public bool IsAuthenicated
	{
		get
		{
			if (!(query == "syncinit") || !IsResultOK)
			{
				return auth == 1;
			}
			return true;
		}
	}

	public bool IsResultOK => result == "ok";

	public bool DoOverwrite => overwrite == 1;

	public LazyCloudSync.CloudResult CloudResult => result switch
	{
		"ok" => LazyCloudSync.CloudResult.OK, 
		"not_found" => LazyCloudSync.CloudResult.WrongSyncCode, 
		"same_id" => LazyCloudSync.CloudResult.CantLinkToSameDevice, 
		"auth_error" => LazyCloudSync.CloudResult.AuthenticationError, 
		_ => LazyCloudSync.CloudResult.Unknown, 
	};
}
