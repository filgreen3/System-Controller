using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;
using System;
using Filgreen3.SystemController;

namespace Filgreen3.SystemController.Editor
{
    public class SystemDisplayer : EditorWindow
    {
        private SystemController _systemsContainer;
        private int _index;

        private Assembly[] _assembles = new Assembly[0];
        private int _assemblyID;
        private int _typeID;

        private Dictionary<ISystem, bool> _systemsFolout = new Dictionary<ISystem, bool>();

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

        private void DisplaySystem()
        {
            var list = _systemsContainer.GetComponentsInChildren<ISystem>();

            foreach (var item in list)
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

        private void AddSystem()
        {
            GUILayout.BeginHorizontal();
            if (_assembles.Length > 0 && GUILayout.Button("+"))
            {
                var assembly = _assembles[_assemblyID];
                var type = assembly.GetType(GetSystems(_assembles[_assemblyID])[_typeID]);
                new GameObject(type.Name, type).transform.SetParent(_systemsContainer.transform);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
                _assembles = System.AppDomain.CurrentDomain.GetAllAssembles(isSystemAcceptable);
                _typeID = 0;
                _assemblyID = 0;

            }

            if (_assembles.Length > 0)
            {
                _typeID = EditorGUILayout.Popup(_typeID, GetSystems(_assembles[_assemblyID]));
                EditorGUI.BeginChangeCheck();
                _assemblyID = EditorGUILayout.Popup(_assemblyID, GetAssembles());
            }
            if (EditorGUI.EndChangeCheck())
            {
                _typeID = 0;
            }

            GUILayout.EndHorizontal();
        }

        public void OnGUI()
        {
            if (!_systemsContainer)
            {
                if (GUILayout.Button("Init Systems"))
                {
                    var _systemsObject = new GameObject("Systems");
                    _systemsContainer = _systemsObject.AddComponent<SystemController>();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorSceneManager.SaveOpenScenes();
                    Debug.Log("Init SystemController");
                }
            }
            else
            {
                if (GUILayout.Button("Refresh"))
                {
                    OnEnable();
                }
                DisplaySystem();
                AddSystem();
            }
        }

        private void GetSceneSystem()
        {
            var array = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var item in array)
            {
                if (item.name == "Systems")
                {
                    if (!item.TryGetComponent<SystemController>(out _systemsContainer))
                    {
                        _systemsContainer = item.AddComponent<SystemController>();
                    }
                }
            }
        }

        private Type[] GetSystemTypes(Assembly assembly)
        {
            var result = new List<Type>();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (isSystemAcceptable(type))
                {
                    result.Add(type);
                }
            }
            return result.ToArray();
        }

        private bool isSystemAcceptable(Type type) =>
            type != null
            && typeof(ISystem).IsAssignableFrom(type)
            && type.IsClass
            && !type.IsAbstract
            && typeof(MonoBehaviour).IsAssignableFrom(type)
            && !_systemsContainer.ContainType(type);

        private string[] GetSystems(Assembly assembly)
        {
            var result = new List<string>();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (isSystemAcceptable(type))
                {
                    result.Add(type.Name);
                }
            }
            return result.ToArray();
        }

        private string[] GetAssembles()
        {
            var result = new List<string>();
            foreach (var assembly in _assembles)
            {
                result.Add(assembly.GetName().Name);
            }
            return result.ToArray();
        }
    }

    public static class ClassFindHelper
    {
        public static Assembly[] GetAllAssembles(this System.AppDomain aAppDomain, Func<Type, bool> compereFunc)
        {
            var result = new List<Assembly>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (IsAcceptedAssembly(assembly, compereFunc))
                {
                    result.Add(assembly);
                }
            }
            return result.ToArray();
        }

        private static bool IsAcceptedAssembly(Assembly assembly, Func<Type, bool> compere)
        {
            foreach (var type in assembly.ExportedTypes)
            {
                if (compere(type))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
