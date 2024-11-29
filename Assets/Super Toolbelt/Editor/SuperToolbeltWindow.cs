using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Super_Toolbelt.Editor
{
    public class SuperToolbeltWindow : EditorWindow
    {
        #region Window
        [MenuItem("Window/Super Toolbelt Window")]
        public static void OpenWindow()
        {
            SuperToolbeltWindow wnd = GetWindow<SuperToolbeltWindow>();
            wnd.titleContent = new GUIContent("Super Toolbelt Window");
        }
        #endregion
        
        /// <summary>
        /// The buttons list (array, but you get it).
        /// 
        /// If the string matches a path for an icon, it will display the icon.
        /// If not, the button will display it as text instead.
        /// </summary>
        private readonly string[] BUTTONS =
        {
            "Toggle UI",
            "Toggle Inspector Debug Mode",
            "Path_to_icon"
        };

        private void OnGUI()
        {
            int buttonsCount = BUTTONS.Length;
            float buttonWidth = position.width / buttonsCount;
            
            GUILayout.BeginHorizontal();
            for (int i = 0; i < buttonsCount; i++)
            {
                // Add a button with dynamic size
                // TODO: Add check for icon path
                if (GUILayout.Button(BUTTONS[i], GUILayout.Width(buttonWidth), GUILayout.Height(position.height)))
                {
                    int buttonIndex = i;

                    // Wrap the click logic in a delay call to ensure layout integrity
                    EditorApplication.delayCall += () =>
                    {
                        ClickButton(buttonIndex);
                    };
                }
            }
            GUILayout.EndHorizontal();
        }

        private void ClickButton(int buttonID)
        {
            switch (buttonID)
            {
                case 0: ToggleUI(); break;
                case 1: ToggleInspectorMode(); break;
            }
        }
      
        // Buttons logic and functionality
        #region Button 1: Toggle UI
        private void ToggleUI()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                SceneVisibilityManager.instance.ToggleVisibility(canvas.gameObject, true);
            }
        }
        #endregion
        
        #region Button 2: Toggle Inspector Debug Mode
        private void ToggleInspectorMode()
        {
            EditorWindow targetInspector = GetActiveInspectorWindow();
 
            if (targetInspector != null  && targetInspector.GetType().Name == "InspectorWindow")
            {
                Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");
                FieldInfo field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    InspectorMode mode = (InspectorMode)field.GetValue(targetInspector);
                    mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);
                    
                    MethodInfo method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(targetInspector, new object[] { mode });
                    }
                }
            
                targetInspector.Repaint();
            }
        }

        private static EditorWindow GetActiveInspectorWindow()
        {
            Type inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            EditorWindow[] windows = (EditorWindow[]) Resources.FindObjectsOfTypeAll(inspectorType);
            if (windows != null)
            {
                return windows.Length > 0 ? windows[0] : null;
            }

            return null;
        }
        #endregion
    }
}