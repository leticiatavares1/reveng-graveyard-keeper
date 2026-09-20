using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class ApplicationSettings
{
	private enum SupportedPlatforms
	{
		PC,
		Switch,
		Xbox,
		PS4,
		PS5,
		Switch2
	}

	[Serializable]
	public class NintendoApplicationData
	{
		public string applicationId;

		public string saveFolderPath;
	}

	[Serializable]
	public class PS5PackagePair
	{
		public string sourcePath;

		public string contentLabel;
	}

	[Serializable]
	public class PS5TransferringData
	{
		public string fingerprint;

		public string titleId;
	}

	[Serializable]
	public class PS4TransferringData
	{
		public string fingerprint;

		public string titleId;
	}

	public const string XBOX_ONE_DEFAULT_CONFIGURATION_PATH = "ProjectSettings/XboxOneGame.config";

	public const string XBOX_SCARLETT_DEFAULT_CONFIGURATION_PATH = "ProjectSettings/ScarlettGame.config";

	[Header("General")]
	public LazyBuildType buildType;

	[Tooltip("Defines which should be included only for this build type")]
	public List<string> exclusiveDefines;

	public string applicationName;

	public bool showUnityLogo;

	[Header("Standalone")]
	public string pathToSteamPreorderFile;

	[Header("Nintendo Switch")]
	[Tooltip("Uses to access saves from other application\nPlease use id without \"0x\" prefix")]
	public List<NintendoApplicationData> switchOtherApplicationsData;

	[Tooltip("Path to NMETAOverride file")]
	public string NMETAOverride;

	[Tooltip("Save folder path")]
	public string switchSaveFolderPath = "saveData";

	[Header("Nintendo Switch")]
	[Tooltip("Uses to access saves from other application\nPlease use id without \"0x\" prefix")]
	public List<NintendoApplicationData> switch2OtherApplicationsData;

	[Tooltip("Path to NMETAOverride file")]
	public string NMETAOverrideSwitch2;

	[Tooltip("Save folder path")]
	public string switch2SaveFolderPath = "saveData2";

	[Header("Xbox")]
	public string xboxSCID;

	public bool advancedUserModel;

	[Tooltip("Path to XboxOneGame.config\nIt must be different with XBOX_DEFAULT_CONFIGURATION_PATH")]
	public string xboxConfigPath;

	public string xboxOverrodeResourcesPath;

	public string xboxOverrodeLocalizationPath;

	[Tooltip("Uses to access saves from other applications")]
	public List<string> xboxOtherApplicationIds;

	[Header("PS4")]
	[Header("PS4")]
	public string pathToPS4ParamFile;

	public int parentalLevelPS4;

	public int ageRatingPS4;

	public string pathToPS4BackgroundImage;

	public string pathToPS4StartupImagePath;

	public string npTitleSecretPS4;

	public string npTitleDatPathPS4;

	public int appTypePS4;

	public int categoryPS4;

	public string npTrophyPackPathPS4;

	public SonyNPAgeRestriction[] sonyNPAgeRestrictions;

	[Tooltip("For PlayerPrefs")]
	public string pathToPS4SaveDataImage;

	[Tooltip("For savedata2 plugin")]
	public string pathToPS4SaveDataImageStreaming = "/app0/Media/StreamingAssets/SaveIcon.png";

	public PS4TransferringData ps4TransferringData = new PS4TransferringData();

	public string ps4PatchChangeinfoPath;

	[Header("PS5")]
	[Header("PS5")]
	public List<PS5PackagePair> ps5PackagePairs;

	public PS5TransferringData ps5TransferringData;

	[Header("PS5")]
	public string ps5ParamFilePath;

	[Header("PS5")]
	public string[] sharedBinarySystemFolders;
}
