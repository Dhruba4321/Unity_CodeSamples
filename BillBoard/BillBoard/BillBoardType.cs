/// <summary>
/// This code is a custom Unity editor script for the Billboard class, which creates an interactive interface in the Unity Inspector.
/// Depending on the option selected from the dropdown menu (Video, Picture, or GIF), it dynamically displays the corresponding field
/// (VideoClip, Texture2D, or GIF respectively) for user input.
/// </summary>

using UnityEditor;
using UnityEngine;
using Tropyverse;
using UnityEngine.Video;

namespace Tropyverse
{
    // Specify the custom editor for the Billboard class
    [CustomEditor(typeof(Billboard))]
    public class BillBoardEditor : Editor
    {
        // Override the GUI for the Inspector
        public override void OnInspectorGUI()
        {
            // Cast the target to Billboard
            Billboard myScript = (Billboard)target;

            // Create an EnumPopup in the Inspector and allow it to change the options field of the Billboard
            myScript.options = (Billboard.MyOptions)EditorGUILayout.EnumPopup("Options", myScript.options);

            // Depending on the selected option, display a different field
            switch (myScript.options)
            {
                case Billboard.MyOptions.Video:
                    // Enable/Disable video looping
                    myScript.looping = EditorGUILayout.Toggle("Loop Video", myScript.looping);

                    break;

                case Billboard.MyOptions.Image:
                    // // If the selected option is Image, display a field for a video url
                    // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    // EditorGUILayout.LabelField("Enter Image API", EditorStyles.boldLabel);
                    // myScript.imageUrl = EditorGUILayout.TextField("", myScript.imageUrl);
                    break;

                case Billboard.MyOptions.GIF:
                    // If the selected option is GIF, display all these fields
                    myScript.gifEnable = EditorGUILayout.Toggle("GIF", myScript.gifEnable);

                    myScript.m_renderer = (Renderer)EditorGUILayout.ObjectField("Renderer", myScript.m_renderer, typeof(Renderer), true);

                    myScript.m_filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", myScript.m_filterMode);

                    myScript.m_wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", myScript.m_wrapMode);

                    myScript.m_loadOnStart = EditorGUILayout.Toggle("Load On Start", myScript.m_loadOnStart);

                    myScript.m_rotateOnLoading = EditorGUILayout.Toggle("Rotate On Loading", myScript.m_rotateOnLoading);

                    myScript.m_outputDebugLog = EditorGUILayout.Toggle("Output Debug Log", myScript.m_outputDebugLog);

                    break;

                case Billboard.MyOptions.Browser:
                    // // If the selected option is Website, display a field for web url
                    // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    // EditorGUILayout.LabelField("Website URL", EditorStyles.boldLabel);
                    // myScript.webUrl = EditorGUILayout.TextField("", myScript.webUrl);
                    break;
            }

            // If any changes have been made in the Inspector, mark the object as dirty to ensure that the changes are saved
            if (GUI.changed)
            {
                EditorUtility.SetDirty(myScript);
            }
        }
    }
}