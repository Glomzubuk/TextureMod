using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TextureMod
{
    public class ModDebugging : MonoBehaviour
    {
        public Dictionary<string, Dictionary<string, string>> windows = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, Rect> windowRects = new Dictionary<string, Rect>();

        public bool showHud = true;
        public List<bool> enabledWindows = new List<bool>();
        public Rect windowSelectionWindowRect = new Rect(10, 10, 100, 100);

        private void OnGUI()
        {
            if (TextureMod.showDebugInfo.Value)
            {

                GUIStyle ss = new GUIStyle("Box");
                GUI.skin.window = ss;
                GUI.skin.window.fontSize = 20;
                GUI.skin.window.padding.top = 35;
                GUI.skin.window.contentOffset = new Vector2(0, -25f);
                GUI.skin.window.alignment = TextAnchor.UpperCenter;

                // window selection window size calculation 
                if (enabledWindows.Count < windows.Count)
                {
                    do { enabledWindows.Add(false);  Debug.Log("did it"); } while (enabledWindows.Count < windows.Count);
                }

                int wSW_Size = -1;
                string wSW_Str = "";
                foreach (KeyValuePair<string, Dictionary<string, string>> window in windows)
                {
                    var s = window.Key + ": ";
                    if (s.Length > wSW_Size)
                    {
                        wSW_Size = s.Length;
                        wSW_Str = s;
                    }
                }
                GUIContent wSWS_GuiContent = new GUIContent(wSW_Str);

                if (showHud) windowSelectionWindowRect = GUILayout.Window(909089, new Rect(0, 0, GUI.skin.window.CalcSize(wSWS_GuiContent).x + 100, windowSelectionWindowRect.height), new GUI.WindowFunction(WindowSelectionWindow), "Debug Windows (Shift + D)");


                //
                int i = 909090;
                foreach (KeyValuePair<string, Dictionary<string, string>> window in windows)
                {
                    if (enabledWindows[i - 909090] == true)
                    {
                        int size = -1;
                        string str = "";
                        foreach (KeyValuePair<string, string> values in window.Value)
                        {
                            var s = values.Key + " = " + values.Value;
                            if (s.Length > size)
                            {
                                size = s.Length;
                                str = s;
                            }
                        }
                        GUIContent guiContent = new GUIContent(str);

                        if (!windowRects.ContainsKey(window.Key)) windowRects.Add(window.Key, new Rect(0, 0, 100, 20));

                        windowRects[window.Key] = GUILayout.Window(i, new Rect(windowRects[window.Key].x, windowRects[window.Key].y, GUI.skin.window.CalcSize(guiContent).x + 50, windowRects[window.Key].height), new GUI.WindowFunction(DebugWindow), window.Key);
                    }
                    i++;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.D)) showHud = !showHud;
            }
        }

        private void WindowSelectionWindow(int _windowId)
        {
            GUI.skin.label.fontSize = 16;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            {
                var i = 0;

                if (windows.Count > 0)
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> window in windows)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            var str = window.Key + ": ";
                            GUILayout.Label(str);
                            if (GUILayout.Button(enabledWindows[i].ToString())) enabledWindows[i] = !enabledWindows[i];
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        i++;
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DebugWindow(int _windowId)
        {
            Dictionary<string, string> values = windows.ElementAt(_windowId - 909090).Value;
            GUI.skin.label.fontSize = 16;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            {
                foreach (KeyValuePair<string, string> pair in values)
                {
                    GUILayout.BeginHorizontal();
                    {
                        var str = pair.Key + " = " + pair.Value;
                        GUILayout.Label(str);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        public void AddToWindow(string windowName, string varName, string varValue)
        {
            if (windows.ContainsKey(windowName))
            {
                if (windows[windowName].ContainsKey(varName)) windows[windowName][varName] = varValue;
                else windows[windowName].Add(varName, varValue);
            } else
            {
                windows.Add(windowName, new Dictionary<string, string>());
                windows[windowName].Add(varName, varValue);
            }
        }

        private Texture2D ColorToTexture2D(Color color)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

    }
}
