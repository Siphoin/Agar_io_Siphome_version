using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AgarIOSiphome.System
{
    public abstract class ScriptableObjectIdentity : ScriptableObject, IIdentity
    {
        [SerializeField, ReadOnly] private string _guidObject = Guid.NewGuid().ToString();

        public string GUID => _guidObject;
    }
}
