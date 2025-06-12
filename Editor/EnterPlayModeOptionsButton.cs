using UnityEditor;
using UnityEngine;

public class PlayModeSwitchWindow : EditorWindow
{
    private static readonly Color activeColor = new Color(0.1f, 0.6f, 1f);
    private static readonly Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);
    private static readonly Color warningColor = new Color(1f, 0.5f, 0f);

    [MenuItem("Window/Play Mode Switch")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayModeSwitchWindow>("Play Mode Switch");
        window.minSize = new Vector2(300, 240);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Play Mode Settings", EditorStyles.largeLabel);
        EditorGUILayout.Space(5);

        DrawCurrentSetting();
        EditorGUILayout.Space(15);

        DrawOptions();
        EditorGUILayout.Space(15);

        DrawInfoBox();
    }

    private void DrawCurrentSetting()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);
        
        var currentOption = GetCurrentOption();
        var rect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.6f, rect.height), "Mode:");
        EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, rect.height), 
                            currentOption, 
                            new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold });
        
        EditorGUILayout.EndVertical();
    }

    private void DrawOptions()
    {
        EditorGUILayout.LabelField("Change Mode:", EditorStyles.boldLabel);
        
        DrawOptionButton("Reload Domain & Scene", 
                        "Traditional Unity behavior.\nFully resets everything when entering Play mode.",
                        !EditorSettings.enterPlayModeOptionsEnabled);
        
        DrawOptionButton("Reload Scene Only", 
                        "Keeps loaded assemblies in memory.\nFaster but may cause issues with static variables.",
                        EditorSettings.enterPlayModeOptionsEnabled && 
                        EditorSettings.enterPlayModeOptions == EnterPlayModeOptions.DisableDomainReload);
        
        DrawOptionButton("Reload Domain Only", 
                        "Keeps the current scene state.\nUseful when working with complex scene setups.",
                        EditorSettings.enterPlayModeOptionsEnabled && 
                        EditorSettings.enterPlayModeOptions == EnterPlayModeOptions.DisableSceneReload);
        
        DrawOptionButton("Do Not Reload", 
                        "Fastest but most dangerous option.\nMay cause unexpected behavior.",
                        EditorSettings.enterPlayModeOptionsEnabled && 
                        EditorSettings.enterPlayModeOptions == (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload));
    }

    private void DrawOptionButton(string label, string tooltip, bool isActive)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(15, 5, 5, 5),
            fixedHeight = 30
        };

        var bgColor = isActive ? activeColor : inactiveColor;
        var textColor = isActive ? Color.white : new Color(0.8f, 0.8f, 0.8f);

        var origColor = GUI.backgroundColor;
        var origTextColor = GUI.contentColor;

        GUI.backgroundColor = bgColor;
        GUI.contentColor = textColor;

        if (GUILayout.Button(new GUIContent(label, tooltip), style))
        {
            ApplyOption(label);
        }

        GUI.backgroundColor = origColor;
        GUI.contentColor = origTextColor;
    }

    private void ApplyOption(string option)
    {
        switch (option)
        {
            case "Reload Domain & Scene":
                EditorSettings.enterPlayModeOptionsEnabled = false;
                break;
            case "Reload Scene Only":
                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
                break;
            case "Reload Domain Only":
                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableSceneReload;
                break;
            case "Do Not Reload":
                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
                break;
        }
    }

    private void DrawInfoBox()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("ℹ Information", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("These settings control how Unity enters Play mode:", EditorStyles.wordWrappedMiniLabel);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("• Domain Reload: Resets all C# code", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("• Scene Reload: Resets the current scene", EditorStyles.wordWrappedMiniLabel);
        
        EditorGUILayout.Space(5);
        
        var warningStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
        {
            normal = { textColor = warningColor }
        };
        
        EditorGUILayout.LabelField("⚠ Warning: Disabling reloads may cause unexpected behavior with static variables and scene objects.", warningStyle);
        EditorGUILayout.EndVertical();
    }

    private string GetCurrentOption()
    {
        if (!EditorSettings.enterPlayModeOptionsEnabled)
        {
            return "Reload Domain & Scene";
        }

        var options = EditorSettings.enterPlayModeOptions;

        if (options == EnterPlayModeOptions.DisableDomainReload)
        {
            return "Reload Scene Only";
        }

        if (options == EnterPlayModeOptions.DisableSceneReload)
        {
            return "Reload Domain Only";
        }

        if (options == (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload))
        {
            return "Do Not Reload";
        }

        return "Custom Configuration";
    }
}
