using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        // The buttons list (array, but you get it).
        private readonly string[] BUTTONS =
        {
            "Switch Scenes",
            "Toggle UI",
            "Toggle Inspector Debug",
        };

        private void OnGUI()
        {
            int buttonsCount = BUTTONS.Length;
            float buttonWidth = position.width / buttonsCount;
            
            GUILayout.BeginHorizontal();
            for (int i = 0; i < buttonsCount; i++)
            {
                // Add a button with dynamic size
                GUI.enabled = IsButtonInteractive(i);
                
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
            Repaint();
            GUILayout.EndHorizontal();
        }

        private void ClickButton(int buttonID)
        {
            switch (buttonID)
            {
                case 0: SwitchToLastScene(); break;
                case 1: ToggleUI(); break;
                case 2: ToggleInspectorMode(); break;
            }
        }

        private bool IsButtonInteractive(int buttonID)
        {
            switch (buttonID)
            {
                case 0: return CanSwitchToLastScene();
                case 1: return CanToggleUI();
                case 2: return CanToggleInspectorMode();
            }

            return false;
        }

        private bool CanToggleUI()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return canvases.Length > 0;
        }

        private bool CanToggleInspectorMode()
        {
            EditorWindow targetInspector = GetActiveInspectorWindow();
            return targetInspector;
        }

        private bool CanSwitchToLastScene()
        {
            return !String.IsNullOrEmpty(sceneB);
        }
      
        // Buttons logic and functionality
        #region Button 2: Toggle UI
        private void ToggleUI()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                SceneVisibilityManager.instance.ToggleVisibility(canvas.gameObject, true);
            }
        }
        #endregion
        
        #region Button 3: Toggle Inspector Debug Mode
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

        #region Button 1: Switch to Last Scene
        private static string sceneA, sceneB;
        
        static SuperToolbeltWindow()
        {
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        private static void SwitchToLastScene()
        {
            EditorSceneManager.OpenScene(sceneB, OpenSceneMode.Single);
        }
        
        private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.path != sceneA)
            {
                sceneB = sceneA;
                sceneA = newScene.path;
            }
        }
        #endregion
    }
}