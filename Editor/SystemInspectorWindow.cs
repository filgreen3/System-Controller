using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;
using System;

namespace Filgreen3.SystemController.Editor
{
    public abstract class SystemInspectorWindow : EditorWindow
    {
        protected SystemController _systemsContainer;

        protected Assembly[] _assembles = new Assembly[0];
        protected int _assemblyID;
        protected int _typeID;


        public abstract void OnGUI();

        protected void AddSystem()
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

        protected void GetSceneSystem()
        {
            var array = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var item in array)
            {
                if (item.name == "<Systems>")
                {
                    if (!item.TryGetComponent<SystemController>(out _systemsContainer))
                    {
                        _systemsContainer = item.AddComponent<SystemController>();
                    }
                }
            }
        }

        protected Type[] GetSystemTypes(Assembly assembly)
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

        protected bool isSystemAcceptable(Type type) =>
            type != null
            && typeof(ISystem).IsAssignableFrom(type)
            && type.IsClass
            && !type.IsAbstract
            && typeof(MonoBehaviour).IsAssignableFrom(type)
            && !_systemsContainer.ContainType(type);

        protected string[] GetSystems(Assembly assembly)
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

        protected string[] GetAssembles()
        {
            var result = new List<string>();
            foreach (var assembly in _assembles)
            {
                result.Add(assembly.GetName().Name);
            }
            return result.ToArray();
        }
    }
}
