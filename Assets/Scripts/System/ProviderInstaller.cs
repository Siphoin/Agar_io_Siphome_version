using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Zenject;
namespace AgarIOSiphome.System
{
    public abstract class ProviderInstaller<T> : MonoInstaller where T : UnityEngine.Object
    {
        [SerializeField] private T[] _elements = new T[0];

        public override void InstallBindings()
        {
            for (int i = 0; i < _elements.Length; i++)
            {
                var element = _elements[i];
                Container.Bind(element.GetType()).FromInstance(element);

                if (element is IInitializable initializable)
                {
                    initializable.Initialize();
                }
            }
        }

#if UNITY_EDITOR
        public void SetDataEditor (IEnumerable<T> elements)
        {
            if (elements is null)
            {
                throw new NullReferenceException(nameof(elements));
            }

            _elements = elements.ToArray();
        }
#endif
    }
}