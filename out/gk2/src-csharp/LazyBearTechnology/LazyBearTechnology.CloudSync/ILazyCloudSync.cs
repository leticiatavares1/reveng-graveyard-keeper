using System;

namespace LazyBearTechnology.CloudSync;

public interface ILazyCloudSync
{
	string GetApplicationVersion();

	LazyCloudSyncSaveData GetSavegameData();

	void SetSavegameData(LazyCloudSyncSaveData data);

	void ShowCloudSyncDialogWithEnterCodeButton();

	void ShowCloudSyncDialogWithBreakSyncButton();

	void ShowCloudSyncDialogWithCode(string code);

	void OnCloudSyncCodeExpired();

	void ShowCloudSyncErrorMessage(LazyCloudSync.CloudResult result);

	void ShowCloudSyncSaveChooserDialog(LazyCloudSyncSaveData local_data, Action on_local_chosen, LazyCloudSyncSaveData remote_data, Action on_remote_chosen);
}
