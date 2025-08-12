using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// SECTION 1: THE FLOATING EDITOR WINDOW
public class PlayModeSwitchWindow : EditorWindow
{
    [MenuItem("Tools/Play Mode/Play Once with Full Reload & Reset", false, 1)]
    public static void PlayOnceWithFullReload()
    {
        PlayModeSwitchSettingsProvider.ArmOneShotReloadAndPlay();
    }
    
    [MenuItem("Window/Play Mode Switch")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayModeSwitchWindow>("Play Mode Switch");
        window.minSize = new Vector2(380, 360);
    }
    
    private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

    private void OnPlayModeStateChanged(PlayModeStateChange state) => Repaint();

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(10, 10, 10, 10) });
        PlayModeSwitchSettingsProvider.DrawSharedGui();
        EditorGUILayout.EndVertical();
    }
}


// SECTION 2: THE SETTINGS PROVIDER AND SHARED LOGIC
// This class is decorated with [InitializeOnLoad] to handle state restoration after a domain reload.
[InitializeOnLoad]
class PlayModeSwitchSettingsProvider : SettingsProvider
{
    private enum PlayModeOption { ReloadDomainAndScene, ReloadSceneOnly, ReloadDomainOnly, DoNotReload, Custom }

    // --- GUI Styles ---
    private static readonly Color ActiveColor = new Color(0.1f, 0.6f, 1f, 1f);
    private static readonly Color InactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    private static readonly Color WarningColor = new Color(1f, 0.6f, 0.2f, 1f);
    private static readonly Color OneShotColor = new Color(0.2f, 0.8f, 0.4f, 1f);

    private static GUIStyle _activeButtonStyle, _inactiveButtonStyle, _warningLabelStyle, _oneShotButtonStyle;
    private static readonly Dictionary<PlayModeOption, GUIContent> OptionContent = new Dictionary<PlayModeOption, GUIContent>();

    // --- State Management using SessionState (to survive Domain Reloads) ---
    private const string OneShotActiveKey = "PlayModeSwitch.OneShotReload.IsActive";
    private const string OneShotOriginalEnabledKey = "PlayModeSwitch.OneShotReload.OriginalEnabled";
    private const string OneShotOriginalOptionsKey = "PlayModeSwitch.OneShotReload.OriginalOptions";
    
    private static bool IsOneShotReloadActive
    {
        get => SessionState.GetBool(OneShotActiveKey, false);
        set => SessionState.SetBool(OneShotActiveKey, value);
    }

    // --- Constructor and Registration ---
    
    static PlayModeSwitchSettingsProvider()
    {
        if (IsOneShotReloadActive)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChange_HandleOneShotReload;
            EditorApplication.playModeStateChanged += OnPlayModeChange_HandleOneShotReload;
        }
    }

    public PlayModeSwitchSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        var provider = new PlayModeSwitchSettingsProvider("Project/Play Mode Switch", SettingsScope.Project)
        {
            keywords = new HashSet<string>(new[] { "Play", "Mode", "Reload", "Domain", "Scene", "Fast", "Reset" })
        };
        return provider;
    }

    // --- GUI Drawing ---
    public override void OnGUI(string searchContext) => DrawSharedGui();

    public static void DrawSharedGui()
    {
        InitializeStylesAndContent();

        EditorGUI.BeginDisabledGroup(IsOneShotReloadActive);
        
        EditorGUILayout.LabelField("Play Mode Settings", EditorStyles.largeLabel);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Change Mode:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        DrawOptions();
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(15);
        DrawOneShotReloadSection();
        EditorGUILayout.Space(15);
        DrawInfoBox();
    }
    
    // --- GUI Drawing Helpers ---
    private static void InitializeStylesAndContent()
    {
        if (_activeButtonStyle != null) return;
        _activeButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(10, 5, 5, 5), fixedHeight = 40, fontStyle = FontStyle.Bold, richText = true, normal = { textColor = Color.white } };
        _inactiveButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(10, 5, 5, 5), fixedHeight = 40, richText = true, normal = { textColor = new Color(0.8f, 0.8f, 0.8f) } };
        _warningLabelStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, normal = { textColor = WarningColor }, fontStyle = FontStyle.Bold };
        _oneShotButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, fixedHeight = 45, fontStyle = FontStyle.Bold, fontSize = 13, richText = true, normal = { textColor = Color.white } };

        OptionContent[PlayModeOption.ReloadDomainAndScene] = new GUIContent(" Reload Domain & Scene", EditorGUIUtility.IconContent("d_Refresh").image, "Default Unity behavior. Slowest but safest.");
        OptionContent[PlayModeOption.ReloadSceneOnly] = new GUIContent(" Reload Scene Only (Fast Play)", EditorGUIUtility.IconContent("SceneLoadIn").image, "Skips C# code reload. Much faster entry.");
        OptionContent[PlayModeOption.ReloadDomainOnly] = new GUIContent(" Reload Domain Only", EditorGUIUtility.IconContent("cs Script Icon").image, "Reloads C# code but keeps the scene.");
        OptionContent[PlayModeOption.DoNotReload] = new GUIContent(" Do Not Reload (Fastest)", EditorGUIUtility.IconContent("d_SpeedScale").image, "Fastest but most likely to cause issues.");
    }
    
    private static void DrawOptions()
    {
        var currentOption = GetCurrentOption();
        DrawOptionButton(PlayModeOption.ReloadDomainAndScene, currentOption);
        DrawOptionButton(PlayModeOption.ReloadSceneOnly, currentOption);
        DrawOptionButton(PlayModeOption.ReloadDomainOnly, currentOption);
        DrawOptionButton(PlayModeOption.DoNotReload, currentOption);
        if (currentOption == PlayModeOption.Custom) { EditorGUILayout.HelpBox("Your Enter Play Mode settings are in a custom configuration.", MessageType.Warning); }
    }
    
    private static void DrawOneShotReloadSection()
    {
        EditorGUILayout.LabelField("For Resetting Static Values:", EditorStyles.boldLabel);
        if (IsOneShotReloadActive)
        {
            EditorGUILayout.HelpBox("ARMED: The next Play session will use a full Domain & Scene reload. Your previous setting will be restored when you exit Play Mode.", MessageType.Warning);
        }
        else
        {
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = OneShotColor;
            var buttonContent = new GUIContent(" Play Once with Full Reload & Reset", EditorGUIUtility.IconContent("PlayButton").image, "Enters Play Mode ONCE with full reload to fix static variables, then reverts to your chosen fast setting.");
            if (GUILayout.Button(buttonContent, _oneShotButtonStyle)) { ArmOneShotReloadAndPlay(); }
            GUI.backgroundColor = originalColor;
        }
    }

    private static void DrawOptionButton(PlayModeOption option, PlayModeOption currentOption)
    {
        bool isActive = (option == currentOption);
        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = isActive ? ActiveColor : InactiveColor;
        var style = isActive ? _activeButtonStyle : _inactiveButtonStyle;
        if (GUILayout.Button(OptionContent[option], style)) { if (!isActive) ApplyOption(option, false); }
        GUI.backgroundColor = originalColor;
    }

    private static void DrawInfoBox()
    {
        EditorGUILayout.HelpBox("Disabling Domain or Scene Reload can significantly speed up entering Play Mode, but may lead to unexpected behavior with static variables or script initializations.", MessageType.Info);
        EditorGUILayout.LabelField("⚠ If you experience issues, use the 'Play Once with Full Reload' button to perform a reset.", _warningLabelStyle);
    }

    // --- Core Logic ---
    public static void ArmOneShotReloadAndPlay()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        
        SessionState.SetBool(OneShotOriginalEnabledKey, EditorSettings.enterPlayModeOptionsEnabled);
        SessionState.SetInt(OneShotOriginalOptionsKey, (int)EditorSettings.enterPlayModeOptions);
        
        IsOneShotReloadActive = true;
        EditorApplication.playModeStateChanged += OnPlayModeChange_HandleOneShotReload;
        
        ApplyOption(PlayModeOption.ReloadDomainAndScene, true);
        Debug.Log("ONE-SHOT: Armed for a full Domain & Scene reload. Entering Play Mode now...");
        
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChange_HandleOneShotReload(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            bool originalEnabled = SessionState.GetBool(OneShotOriginalEnabledKey, false);
            var originalOptions = (EnterPlayModeOptions)SessionState.GetInt(OneShotOriginalOptionsKey, 0);
            
            EditorSettings.enterPlayModeOptionsEnabled = originalEnabled;
            EditorSettings.enterPlayModeOptions = originalOptions;
            
            IsOneShotReloadActive = false;
            SessionState.EraseString(OneShotOriginalEnabledKey);
            SessionState.EraseString(OneShotOriginalOptionsKey);
            
            EditorApplication.playModeStateChanged -= OnPlayModeChange_HandleOneShotReload;
            
            Debug.Log("ONE-SHOT: Exited Play Mode. Restored previous Play Mode settings.");
            
            if (EditorWindow.HasOpenInstances<PlayModeSwitchWindow>())
            {
                EditorWindow.GetWindow<PlayModeSwitchWindow>().Repaint();
            }
        }
    }

    private static void ApplyOption(PlayModeOption option, bool isOneShot)
    {
        switch (option) {
            case PlayModeOption.ReloadDomainAndScene: EditorSettings.enterPlayModeOptionsEnabled = false; break;
            case PlayModeOption.ReloadSceneOnly: EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload; break;
            case PlayModeOption.ReloadDomainOnly: EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableSceneReload; break;
            case PlayModeOption.DoNotReload: EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload; break;
        }
        if(!isOneShot) { Debug.Log($"Play Mode setting changed to: {option}"); }
    }

    private static PlayModeOption GetCurrentOption()
    {
        if (!EditorSettings.enterPlayModeOptionsEnabled) return PlayModeOption.ReloadDomainAndScene;
        var options = EditorSettings.enterPlayModeOptions;
        if (options == EnterPlayModeOptions.DisableDomainReload) return PlayModeOption.ReloadSceneOnly;
        if (options == EnterPlayModeOptions.DisableSceneReload) return PlayModeOption.ReloadDomainOnly;
        if (options == (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload)) return PlayModeOption.DoNotReload;
        return PlayModeOption.Custom;
    }
}