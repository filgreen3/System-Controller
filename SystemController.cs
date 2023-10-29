using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Filgreen3.SystemController
{
    public class SystemController : MonoBehaviour
    {
        [SerializeField] private ISystem[] _systems;

        public bool ContainType(Type system)
        {
            GetSystems();
            foreach (var item in _systems)
            {
                if (item.GetType() == system)
                    return true;
            }
            return false;
        }

        private void GetSystems()
        {
            _systems = GetComponentsInChildren<ISystem>();
        }

        private void Awake()
        {
            GetSystems();
        }
    }
}
