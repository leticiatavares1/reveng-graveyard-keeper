using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LazyBearTechnology;

public class RuntimeAudioEditor : MonoBehaviour
{
	private static RuntimeAudioEditor instance;

	private AudioSource previewAudioSource;

	private AudioConfig config;

	private AudioConfigData originalConfig;

	private AudioConfigData loadedConfig;

	private string loadedConfigName;

	private bool isVisible;

	private int currentTab;

	private string searchText = "";

	private string configName = "AudioConfigData";

	private int selectedConfigIndex;

	private string statusMessage = "";

	private bool statusIsError;

	private float statusTime;

	private Vector2 soundsScroll;

	private Vector2 playlistsScroll;

	private HashSet<string> expandedSounds = new HashSet<string>();

	private HashSet<string> expandedPlaylists = new HashSet<string>();

	private List<string> configFiles = new List<string>();

	private Rect windowRect;

	private bool isDragging;

	private Vector2 dragOffset;

	private Sound playingSound;

	private Sample playingSample;

	private Playlist playingPlaylist;

	private Track playingTrack;

	private const string WINDOW_TITLE = "Audio Editor";

	private const float WINDOW_WIDTH = 900f;

	private const float WINDOW_HEIGHT = 650f;

	private const string CONFIGS_FOLDER = "AudioConfigs";

	private const string CONFIG_EXTENSION = ".json";

	private GUIStyle windowStyle;

	private GUIStyle headerStyle;

	private GUIStyle tabActiveStyle;

	private GUIStyle tabInactiveStyle;

	private GUIStyle itemStyle;

	private GUIStyle itemStylePlaying;

	private GUIStyle sampleStyle;

	private GUIStyle sampleStylePlaying;

	private GUIStyle labelStyle;

	private GUIStyle smallButtonStyle;

	private GUIStyle foldoutStyle;

	private GUIStyle statusStyle;

	private GUIStyle searchStyle;

	private GUIStyle valueFieldStyle;

	private bool stylesInitialized;

	private string ConfigsPath => Path.Combine(Application.persistentDataPath, "AudioConfigs");

	public static void ToggleWindow()
	{
		if (instance == null)
		{
			CreateInstance();
		}
		else
		{
			instance.Toggle();
		}
	}

	private static void CreateInstance()
	{
		GameObject obj = new GameObject("RuntimeAudioEditor");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		instance = obj.AddComponent<RuntimeAudioEditor>();
	}

	private void Awake()
	{
		config = LazySingletonSO<AudioConfig>.Instance;
		originalConfig = config.CreateConfigData();
		EnsureConfigsDirectoryExists();
		previewAudioSource = base.gameObject.AddComponent<AudioSource>();
		previewAudioSource.playOnAwake = false;
		windowRect = new Rect(((float)Screen.width - 900f) / 2f, ((float)Screen.height - 650f) / 2f, 900f, 650f);
		RefreshConfigList();
		isVisible = true;
		Debug.Log($"[RuntimeAudioEditor] Initialized with {config.sounds?.Count ?? 0} sounds and {config.playlists?.Count ?? 0} playlists");
	}

	private void EnsureConfigsDirectoryExists()
	{
		if (!Directory.Exists(ConfigsPath))
		{
			Directory.CreateDirectory(ConfigsPath);
			Debug.Log("[RuntimeAudioEditor] Created configs directory: " + ConfigsPath);
		}
	}

	private void OnDestroy()
	{
		StopPreview();
		if (instance == this)
		{
			instance = null;
		}
	}

	private void Update()
	{
		if (!string.IsNullOrEmpty(statusMessage) && Time.time - statusTime > 3f)
		{
			statusMessage = "";
		}
	}

	public void Toggle()
	{
		isVisible = !isVisible;
	}

	private void OnGUI()
	{
		if (isVisible)
		{
			InitStyles();
			windowRect.x = Mathf.Clamp(windowRect.x, 0f, (float)Screen.width - windowRect.width);
			windowRect.y = Mathf.Clamp(windowRect.y, 0f, (float)Screen.height - windowRect.height);
			GUI.Box(windowRect, "", windowStyle);
			GUILayout.BeginArea(windowRect);
			DrawHeader();
			DrawTabs();
			DrawSearch();
			DrawContent();
			DrawFooter();
			GUILayout.EndArea();
			HandleDragging();
			UpdateLiveAudio();
		}
	}

	private void InitStyles()
	{
		if (!stylesInitialized)
		{
			stylesInitialized = true;
			windowStyle = new GUIStyle(GUI.skin.box);
			windowStyle.normal.background = MakeTexture(2, 2, new Color(0.15f, 0.15f, 0.18f, 0.98f));
			headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.fontSize = 18;
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.normal.textColor = Color.white;
			headerStyle.alignment = TextAnchor.MiddleLeft;
			tabActiveStyle = new GUIStyle(GUI.skin.button);
			tabActiveStyle.normal.background = MakeTexture(2, 2, new Color(0.3f, 0.5f, 0.7f));
			tabActiveStyle.normal.textColor = Color.white;
			tabActiveStyle.fontStyle = FontStyle.Bold;
			tabActiveStyle.fontSize = 13;
			tabInactiveStyle = new GUIStyle(GUI.skin.button);
			tabInactiveStyle.normal.background = MakeTexture(2, 2, new Color(0.25f, 0.25f, 0.28f));
			tabInactiveStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
			tabInactiveStyle.fontSize = 13;
			itemStyle = new GUIStyle(GUI.skin.box);
			itemStyle.normal.background = MakeTexture(2, 2, new Color(0.22f, 0.22f, 0.25f));
			itemStyle.margin = new RectOffset(0, 0, 2, 2);
			itemStyle.padding = new RectOffset(8, 8, 6, 6);
			sampleStyle = new GUIStyle(GUI.skin.box);
			sampleStyle.normal.background = MakeTexture(2, 2, new Color(0.18f, 0.18f, 0.21f));
			sampleStyle.margin = new RectOffset(20, 0, 1, 1);
			sampleStyle.padding = new RectOffset(8, 8, 4, 4);
			itemStylePlaying = new GUIStyle(GUI.skin.box);
			itemStylePlaying.normal.background = MakeTexture(2, 2, new Color(0.15f, 0.35f, 0.2f));
			itemStylePlaying.margin = new RectOffset(0, 0, 2, 2);
			itemStylePlaying.padding = new RectOffset(8, 8, 6, 6);
			sampleStylePlaying = new GUIStyle(GUI.skin.box);
			sampleStylePlaying.normal.background = MakeTexture(2, 2, new Color(0.12f, 0.3f, 0.18f));
			sampleStylePlaying.margin = new RectOffset(20, 0, 1, 1);
			sampleStylePlaying.padding = new RectOffset(8, 8, 4, 4);
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
			labelStyle.fontSize = 12;
			smallButtonStyle = new GUIStyle(GUI.skin.button);
			smallButtonStyle.fontSize = 14;
			smallButtonStyle.fixedWidth = 28f;
			smallButtonStyle.fixedHeight = 22f;
			foldoutStyle = new GUIStyle(GUI.skin.label);
			foldoutStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
			foldoutStyle.fontStyle = FontStyle.Bold;
			foldoutStyle.fontSize = 13;
			statusStyle = new GUIStyle(GUI.skin.label);
			statusStyle.fontSize = 11;
			statusStyle.alignment = TextAnchor.MiddleLeft;
			searchStyle = new GUIStyle(GUI.skin.textField);
			searchStyle.fontSize = 12;
			valueFieldStyle = new GUIStyle(GUI.skin.textField);
			valueFieldStyle.fontSize = 11;
			valueFieldStyle.alignment = TextAnchor.MiddleCenter;
			valueFieldStyle.fixedHeight = 18f;
		}
	}

	private float FloatSliderWithInput(float value, float min, float max, float sliderWidth, float fieldWidth)
	{
		float num = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(sliderWidth));
		string text = value.ToString("F3");
		string text2 = GUILayout.TextField(text, valueFieldStyle, GUILayout.Width(fieldWidth));
		if (text2 != text)
		{
			text2 = text2.Replace(',', '.');
			if (float.TryParse(text2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				return Mathf.Clamp(result, min, max);
			}
		}
		if (Mathf.Abs(num - value) > 0.0001f)
		{
			return num;
		}
		return value;
	}

	private Texture2D MakeTexture(int width, int height, Color color)
	{
		Color[] array = new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = color;
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	private void HandleDragging()
	{
		Rect rect = new Rect(windowRect.x, windowRect.y, windowRect.width - 40f, 35f);
		Event current = Event.current;
		if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
		{
			isDragging = true;
			dragOffset = current.mousePosition - new Vector2(windowRect.x, windowRect.y);
			current.Use();
		}
		else if (current.type == EventType.MouseUp)
		{
			isDragging = false;
		}
		else if (current.type == EventType.MouseDrag && isDragging)
		{
			windowRect.x = current.mousePosition.x - dragOffset.x;
			windowRect.y = current.mousePosition.y - dragOffset.y;
			current.Use();
		}
	}

	private void DrawHeader()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(15f);
		GUILayout.Label("Audio Editor", headerStyle, GUILayout.Height(35f));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("✕", GUILayout.Width(30f), GUILayout.Height(25f)))
		{
			isVisible = false;
		}
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();
	}

	private void DrawTabs()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(15f);
		if (GUILayout.Button("Sounds", (currentTab == 0) ? tabActiveStyle : tabInactiveStyle, GUILayout.Height(28f), GUILayout.Width(150f)))
		{
			currentTab = 0;
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Playlists", (currentTab == 1) ? tabActiveStyle : tabInactiveStyle, GUILayout.Height(28f), GUILayout.Width(150f)))
		{
			currentTab = 1;
		}
		GUILayout.FlexibleSpace();
		GUILayout.Space(15f);
		GUILayout.EndHorizontal();
		GUILayout.Space(8f);
	}

	private void DrawSearch()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(15f);
		GUILayout.Label("Search:", labelStyle, GUILayout.Width(50f));
		searchText = GUILayout.TextField(searchText, searchStyle, GUILayout.Height(22f));
		GUILayout.Space(15f);
		GUILayout.EndHorizontal();
		GUILayout.Space(8f);
	}

	private void DrawContent()
	{
		GUILayout.BeginArea(new Rect(15f, 110f, windowRect.width - 30f, windowRect.height - 230f));
		if (currentTab == 0)
		{
			DrawSoundsTab();
		}
		else
		{
			DrawPlaylistsTab();
		}
		GUILayout.EndArea();
	}

	private void DrawSoundsTab()
	{
		soundsScroll = GUILayout.BeginScrollView(soundsScroll);
		string value = searchText.ToLower();
		foreach (Sound sound in config.sounds)
		{
			if (string.IsNullOrEmpty(value) || (!string.IsNullOrEmpty(sound.id) && sound.id.ToLower().Contains(value)))
			{
				DrawSoundItem(sound);
			}
		}
		GUILayout.EndScrollView();
	}

	private void DrawSoundItem(Sound sound)
	{
		GUILayout.BeginVertical((previewAudioSource != null && previewAudioSource.isPlaying && playingSound == sound) ? itemStylePlaying : itemStyle);
		GUILayout.BeginHorizontal();
		bool flag = expandedSounds.Contains(sound.id);
		if (GUILayout.Button(flag ? "▼" : "►", smallButtonStyle))
		{
			if (flag)
			{
				expandedSounds.Remove(sound.id);
			}
			else
			{
				expandedSounds.Add(sound.id);
			}
		}
		if (GUILayout.Button("▶", smallButtonStyle))
		{
			PlaySound(sound);
		}
		if (GUILayout.Button("■", smallButtonStyle))
		{
			StopPreview();
		}
		GUILayout.Label(sound.id, foldoutStyle, GUILayout.Width(180f));
		GUILayout.Label("Vol:", labelStyle, GUILayout.Width(30f));
		sound.volume = FloatSliderWithInput(sound.volume, 0f, 1f, 100f, 40f);
		GUILayout.Label("Pan:", labelStyle, GUILayout.Width(30f));
		sound.panning = FloatSliderWithInput(sound.panning, -1f, 1f, 80f, 45f);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		if (expandedSounds.Contains(sound.id) && sound.samples != null)
		{
			GUILayout.Space(4f);
			foreach (Sample sample in sound.samples)
			{
				DrawSampleItem(sample, sound);
			}
		}
		GUILayout.EndVertical();
	}

	private void DrawSampleItem(Sample sample, Sound parentSound)
	{
		GUILayout.BeginHorizontal((previewAudioSource != null && previewAudioSource.isPlaying && playingSample == sample) ? sampleStylePlaying : sampleStyle);
		if (GUILayout.Button("▶", smallButtonStyle))
		{
			PlaySample(sample, parentSound);
		}
		GUILayout.Label((sample.clip != null) ? sample.clip.name : "(no clip)", labelStyle, GUILayout.Width(150f));
		GUILayout.Label("Vol:", labelStyle, GUILayout.Width(28f));
		sample.volume = FloatSliderWithInput(sample.volume, 0f, 1f, 60f, 38f);
		GUILayout.Label("Pitch:", labelStyle, GUILayout.Width(35f));
		sample.pitch = FloatSliderWithInput(sample.pitch, -3f, 3f, 60f, 38f);
		GUILayout.Label("Pan:", labelStyle, GUILayout.Width(28f));
		sample.panning = FloatSliderWithInput(sample.panning, -1f, 1f, 50f, 38f);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void DrawPlaylistsTab()
	{
		playlistsScroll = GUILayout.BeginScrollView(playlistsScroll);
		string value = searchText.ToLower();
		foreach (Playlist playlist in config.playlists)
		{
			if (string.IsNullOrEmpty(value) || (!string.IsNullOrEmpty(playlist.id) && playlist.id.ToLower().Contains(value)))
			{
				DrawPlaylistItem(playlist);
			}
		}
		GUILayout.EndScrollView();
	}

	private void DrawPlaylistItem(Playlist playlist)
	{
		GUILayout.BeginVertical((previewAudioSource != null && previewAudioSource.isPlaying && playingPlaylist == playlist) ? itemStylePlaying : itemStyle);
		GUILayout.BeginHorizontal();
		bool flag = expandedPlaylists.Contains(playlist.id);
		if (GUILayout.Button(flag ? "▼" : "►", smallButtonStyle))
		{
			if (flag)
			{
				expandedPlaylists.Remove(playlist.id);
			}
			else
			{
				expandedPlaylists.Add(playlist.id);
			}
		}
		if (GUILayout.Button("▶", smallButtonStyle))
		{
			PlayPlaylist(playlist);
		}
		if (GUILayout.Button("■", smallButtonStyle))
		{
			StopPreview();
		}
		GUILayout.Label(playlist.id, foldoutStyle, GUILayout.Width(200f));
		GUILayout.Label("Vol:", labelStyle, GUILayout.Width(30f));
		playlist.volume = FloatSliderWithInput(playlist.volume, 0f, 1f, 120f, 45f);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		if (expandedPlaylists.Contains(playlist.id) && playlist.tracks != null)
		{
			GUILayout.Space(4f);
			foreach (Track track in playlist.tracks)
			{
				DrawTrackItem(track, playlist);
			}
		}
		GUILayout.EndVertical();
	}

	private void DrawTrackItem(Track track, Playlist parentPlaylist)
	{
		GUILayout.BeginHorizontal((previewAudioSource != null && previewAudioSource.isPlaying && playingTrack == track) ? sampleStylePlaying : sampleStyle);
		if (GUILayout.Button("▶", smallButtonStyle))
		{
			PlayTrack(track, parentPlaylist);
		}
		GUILayout.Label((!string.IsNullOrEmpty(track.id)) ? track.id : ((track.clip != null) ? track.clip.name : "(no clip)"), labelStyle, GUILayout.Width(150f));
		GUILayout.Label("Vol:", labelStyle, GUILayout.Width(28f));
		track.volume = FloatSliderWithInput(track.volume, 0f, 1f, 60f, 38f);
		GUILayout.Label("Pitch:", labelStyle, GUILayout.Width(35f));
		track.pitch = FloatSliderWithInput(track.pitch, -3f, 3f, 60f, 38f);
		GUILayout.Label("Pan:", labelStyle, GUILayout.Width(28f));
		track.panning = FloatSliderWithInput(track.panning, -1f, 1f, 50f, 38f);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void DrawFooter()
	{
		GUILayout.BeginArea(new Rect(15f, windowRect.height - 120f, windowRect.width - 30f, 115f));
		if (!string.IsNullOrEmpty(statusMessage))
		{
			statusStyle.normal.textColor = (statusIsError ? new Color(1f, 0.5f, 0.5f) : new Color(0.5f, 1f, 0.5f));
			GUILayout.Label(statusMessage, statusStyle);
		}
		else
		{
			GUILayout.Label("", GUILayout.Height(16f));
		}
		GUILayout.Space(3f);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Save as:", labelStyle, GUILayout.Width(55f));
		configName = GUILayout.TextField(configName, GUILayout.Width(200f), GUILayout.Height(22f));
		GUILayout.Space(5f);
		if (GUILayout.Button("Save", GUILayout.Width(70f), GUILayout.Height(24f)))
		{
			SaveConfig();
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Apply Loaded", GUILayout.Width(100f), GUILayout.Height(24f)))
		{
			ApplyLoadedConfig();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Reset to Original", GUILayout.Width(115f), GUILayout.Height(24f)))
		{
			ResetToOriginal();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Load:", labelStyle, GUILayout.Width(55f));
		if (configFiles.Count > 0)
		{
			selectedConfigIndex = Mathf.Clamp(selectedConfigIndex, 0, configFiles.Count - 1);
			selectedConfigIndex = EditorPopup(selectedConfigIndex, configFiles.ToArray(), GUILayout.Width(200f), GUILayout.Height(22f));
		}
		else
		{
			GUILayout.Label("(no configs)", labelStyle, GUILayout.Width(200f));
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Load", GUILayout.Width(70f), GUILayout.Height(24f)))
		{
			LoadConfig();
		}
		if (GUILayout.Button("↻", GUILayout.Width(28f), GUILayout.Height(24f)))
		{
			RefreshConfigList();
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Open Folder", GUILayout.Width(85f), GUILayout.Height(24f)))
		{
			OpenConfigsFolder();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private void OpenConfigsFolder()
	{
		EnsureConfigsDirectoryExists();
		Application.OpenURL("file://" + ConfigsPath);
		SetStatus("Opened: " + ConfigsPath);
	}

	private int EditorPopup(int selectedIndex, string[] options, params GUILayoutOption[] layoutOptions)
	{
		if (GUILayout.Button(((options.Length != 0 && selectedIndex >= 0 && selectedIndex < options.Length) ? options[selectedIndex] : "(none)") + " ▾", layoutOptions))
		{
			selectedIndex = (selectedIndex + 1) % options.Length;
		}
		return selectedIndex;
	}

	private void PlaySound(Sound sound)
	{
		ClearPlayingReferences();
		if (sound.samples == null || sound.samples.Count == 0)
		{
			playingSound = sound;
			PlayClip(config.defaultClip, sound.volume, sound.panning, 1f, sound.loop);
			return;
		}
		Sample randomSample = sound.RandomSample;
		if (randomSample == null || randomSample.clip == null)
		{
			playingSound = sound;
			PlayClip(config.defaultClip, sound.volume, sound.panning, 1f, sound.loop);
			return;
		}
		playingSound = sound;
		playingSample = randomSample;
		float panning = Mathf.Clamp(randomSample.panning + sound.panning, -1f, 1f);
		PlayClip(randomSample.clip, randomSample.volume * sound.volume, panning, randomSample.Pitch, sound.loop);
	}

	private void PlaySample(Sample sample, Sound parentSound)
	{
		ClearPlayingReferences();
		playingSound = parentSound;
		playingSample = sample;
		if (sample.clip == null)
		{
			PlayClip(config.defaultClip, sample.volume * parentSound.volume, sample.panning, 1f, loop: false);
			return;
		}
		float panning = Mathf.Clamp(sample.panning + parentSound.panning, -1f, 1f);
		PlayClip(sample.clip, sample.volume * parentSound.volume, panning, sample.Pitch, loop: false);
	}

	private void PlayPlaylist(Playlist playlist)
	{
		ClearPlayingReferences();
		if (playlist.tracks != null && playlist.tracks.Count != 0)
		{
			Track track = playlist.tracks[0];
			if (!(track.clip == null))
			{
				playingPlaylist = playlist;
				playingTrack = track;
				PlayClip(track.clip, track.volume * playlist.volume, track.panning, track.pitch, loop: false);
			}
		}
	}

	private void PlayTrack(Track track, Playlist parentPlaylist)
	{
		ClearPlayingReferences();
		if (!(track.clip == null))
		{
			playingPlaylist = parentPlaylist;
			playingTrack = track;
			PlayClip(track.clip, track.volume * parentPlaylist.volume, track.panning, track.pitch, loop: false);
		}
	}

	private void PlayClip(AudioClip clip, float volume, float panning, float pitch, bool loop)
	{
		if (clip == null)
		{
			Debug.LogWarning("[RuntimeAudioEditor] Cannot play null clip");
			return;
		}
		previewAudioSource.clip = clip;
		previewAudioSource.volume = volume;
		previewAudioSource.panStereo = panning;
		previewAudioSource.pitch = pitch;
		previewAudioSource.loop = loop;
		previewAudioSource.Play();
	}

	private void StopPreview()
	{
		ClearPlayingReferences();
		if (previewAudioSource != null && previewAudioSource.isPlaying)
		{
			previewAudioSource.Stop();
			previewAudioSource.clip = null;
		}
	}

	private void ClearPlayingReferences()
	{
		playingSound = null;
		playingSample = null;
		playingPlaylist = null;
		playingTrack = null;
	}

	private void UpdateLiveAudio()
	{
		if (!(previewAudioSource == null) && previewAudioSource.isPlaying)
		{
			if (playingSample != null && playingSound != null)
			{
				float panStereo = Mathf.Clamp(playingSample.panning + playingSound.panning, -1f, 1f);
				previewAudioSource.volume = playingSample.volume * playingSound.volume;
				previewAudioSource.panStereo = panStereo;
				previewAudioSource.pitch = playingSample.Pitch;
			}
			else if (playingSound != null)
			{
				previewAudioSource.volume = playingSound.volume;
				previewAudioSource.panStereo = playingSound.panning;
			}
			else if (playingTrack != null && playingPlaylist != null)
			{
				previewAudioSource.volume = playingTrack.volume * playingPlaylist.volume;
				previewAudioSource.panStereo = playingTrack.panning;
				previewAudioSource.pitch = playingTrack.pitch;
			}
			else if (playingPlaylist != null)
			{
				previewAudioSource.volume = playingPlaylist.volume;
			}
		}
	}

	private void RefreshConfigList()
	{
		EnsureConfigsDirectoryExists();
		configFiles = (from f in Directory.GetFiles(ConfigsPath, "*.json")
			select Path.GetFileNameWithoutExtension(f) into f
			orderby f
			select f).ToList();
		if (configFiles.Count > 0 && selectedConfigIndex >= configFiles.Count)
		{
			selectedConfigIndex = 0;
		}
	}

	private void SaveConfig()
	{
		string text = configName?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			SetStatus("Enter a config name", isError: true);
			return;
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char oldChar in invalidFileNameChars)
		{
			text = text.Replace(oldChar, '_');
		}
		string text2 = Path.Combine(ConfigsPath, text + ".json");
		try
		{
			string contents = JsonUtility.ToJson(config.CreateConfigData(), prettyPrint: true);
			File.WriteAllText(text2, contents);
			SetStatus("Saved: " + text);
			Debug.Log("[RuntimeAudioEditor] Saved config to: " + text2);
			RefreshConfigList();
			int num = configFiles.IndexOf(text);
			if (num >= 0)
			{
				selectedConfigIndex = num;
			}
		}
		catch (Exception ex)
		{
			SetStatus("Save failed: " + ex.Message, isError: true);
			Debug.LogError("[RuntimeAudioEditor] Save failed: " + ex.Message);
		}
	}

	private void LoadConfig()
	{
		if (configFiles.Count == 0 || selectedConfigIndex < 0 || selectedConfigIndex >= configFiles.Count)
		{
			SetStatus("No config selected", isError: true);
			return;
		}
		string text = configFiles[selectedConfigIndex];
		string text2 = Path.Combine(ConfigsPath, text + ".json");
		if (!File.Exists(text2))
		{
			SetStatus("Config not found: " + text, isError: true);
			RefreshConfigList();
			return;
		}
		try
		{
			string json = File.ReadAllText(text2);
			loadedConfig = JsonUtility.FromJson<AudioConfigData>(json);
			loadedConfigName = text;
			SetStatus("Loaded: " + text + " (click Apply to use)");
			Debug.Log("[RuntimeAudioEditor] Loaded config from: " + text2);
		}
		catch (Exception ex)
		{
			SetStatus("Load failed: " + ex.Message, isError: true);
			Debug.LogError("[RuntimeAudioEditor] Load failed: " + ex.Message);
			loadedConfig = null;
			loadedConfigName = null;
		}
	}

	private void ApplyLoadedConfig()
	{
		if (loadedConfig == null)
		{
			SetStatus("Load a config first", isError: true);
			return;
		}
		config.ApplyConfigData(loadedConfig);
		SetStatus("Applied: " + loadedConfigName);
		Debug.Log("[RuntimeAudioEditor] Applied config: " + loadedConfigName);
	}

	private void ResetToOriginal()
	{
		if (originalConfig == null)
		{
			SetStatus("No original config cached", isError: true);
			return;
		}
		config.ApplyConfigData(originalConfig);
		SetStatus("Reset to original values");
		Debug.Log("[RuntimeAudioEditor] Reset to original values.");
	}

	private void SetStatus(string message, bool isError = false)
	{
		statusMessage = message;
		statusIsError = isError;
		statusTime = Time.time;
	}
}
