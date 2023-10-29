using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Filgreen3.SystemController;
using System.Linq;

namespace Filgreen3.SystemController.Editor
{
    public class SystemDisplayer : SystemInspectorWindow
    {

        private Vector2 _scroll;
        private Dictionary<Object, bool> _systemsFolout = new Dictionary<Object, bool>();
        private System.Type _filterType = typeof(ISystem);

        [MenuItem("Tools/SystemController/Systems")]
        private static void ShowWindow()
        {
            var window = GetWindow<SystemDisplayer>();
            window.titleContent = new GUIContent("SystemDisplayer");
            window.Show();
        }

        private void OnEnable()
        {
            GetSceneSystem();
            _assemblyID = 0;
            _assembles = System.AppDomain.CurrentDomain.GetAllAssembles(isSystemAcceptable);
        }

        public override void OnGUI()
        {
            if (!_systemsContainer)
            {
                if (GUILayout.Button("Init Systems"))
                {
                    GetSceneSystem();
                    if (_systemsContainer != null) return;
                    var _systemsObject = new GameObject("<Systems>");
                    _systemsContainer = _systemsObject.AddComponent<SystemController>();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorSceneManager.SaveOpenScenes();
                    Debug.Log("Init SystemController");
                }
            }
            else
            {
                UpperMenu();
                DisplaySystem();
                BottomMenu();
            }
        }

        private void UpperMenu()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All"))
            {
                _filterType = typeof(MonoBehaviour);
            }
            if (GUILayout.Button("System"))
            {
                _filterType = typeof(ISystem);
            }
            GUILayout.EndHorizontal();
        }

        private void DisplaySystem()
        {
            var list = _systemsContainer.GetComponentsInChildren(_filterType);

            _scroll = GUILayout.BeginScrollView(_scroll);
            DisplayObjects(list.Cast<Object>().ToArray());
            GUILayout.EndScrollView();
        }


        private void BottomMenu()
        {
            GUILayout.BeginHorizontal();
            if (_assembles.Length > 0 && GUILayout.Button("+"))
            {
                AddSystem();
            }

            if (_assembles.Length > 0)
            {
                _typeID = EditorGUILayout.Popup(_typeID, GetSystems(_assembles[_assemblyID]));
                EditorGUI.BeginChangeCheck();
                _assemblyID = EditorGUILayout.Popup(_assemblyID, GetAssembles());
                if (EditorGUI.EndChangeCheck())
                {
                    _typeID = 0;
                }
            }
            if (GUILayout.Button(_assembles.Length > 0 ? "R" : "Refresh"))
            {
                OnEnable();
            }
            GUILayout.EndHorizontal();
        }

        private void DisplayObjects(params Object[] objects)
        {
            foreach (var item in objects)
            {
                var obj = (UnityEngine.Object)item;
                _systemsFolout.TryAdd(item, false);
                if (_systemsFolout.TryGetValue(item, out var foldout))
                {
                    if (_systemsFolout[item] = EditorGUILayout.InspectorTitlebar(foldout, obj))
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        var editor = UnityEditor.Editor.CreateEditor((UnityEngine.Object)item);
                        editor.OnInspectorGUI();
                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
    }
}
