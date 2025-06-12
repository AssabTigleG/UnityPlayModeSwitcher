using UnityEditor;
using UnityEngine;

public class PlayModeSwitchWindow : EditorWindow
{
    [MenuItem("Window/Play Mode Switch")]
    public static void ShowWindow()
    {
        GetWindow<PlayModeSwitchWindow>("Play Mode Switch");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Current Setting: " + GetCurrentOption(), EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reload Domain & Scene"))
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
        }

        if (GUILayout.Button("Reload Scene Only"))
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        }

        if (GUILayout.Button("Reload Domain Only"))
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableSceneReload;
        }

        if (GUILayout.Button("Do Not Reload"))
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        }
    }

    private string GetCurrentOption()
    {
        if (!EditorSettings.enterPlayModeOptionsEnabled)
        {
            return "Reload Domain & Scene";
        }

        var options = EditorSettings.enterPlayModeOptions;

        if (options == EnterPlayModeOptions.DisableDomainReload && options != EnterPlayModeOptions.DisableSceneReload)
        {
            return "Reload Scene Only";
        }

        if (options == EnterPlayModeOptions.DisableSceneReload && options != EnterPlayModeOptions.DisableDomainReload)
        {
            return "Reload Domain Only";
        }

        if (options == (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload))
        {
            return "Do Not Reload";
        }

        return "Default";
    }
}