using System;
using UnityEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Поставщик ссылок, используемый в качестве сериализуемого поля для внедрения зависимостей через интерфейсы, которые Unity не сериализует по умолчанию.
    /// Позволяет через кастомный выпадающий список (dropdown) напрямую назначать и прокидывать компоненты со сцены в качестве интерфейсных зависимостей.
    /// <br/><br/>
    /// A reference provider used as a serializable field for injecting interface-based dependencies that Unity cannot serialize by default.
    /// Allows directly assigning and passing scene components as interface dependencies via a custom dropdown menu.
    /// </summary>
    [Serializable]
    public sealed class ReferenceProvider<TInterface> where TInterface : class
    {
        /// <summary>
        /// Ссылка на объект Unity (GameObject, MonoBehaviour или ScriptableObject), содержащий нужную реализацию интерфейса.
        /// <br/><br/>
        /// Reference to the Unity object (GameObject, MonoBehaviour, or ScriptableObject) containing the required interface implementation.
        /// </summary>
        [SerializeField] private UnityEngine.Object _targetObject;

        /// <summary>
        /// Полное имя типа интерфейса для его валидации и однозначной идентификации в сборке.
        /// <br/><br/>
        /// The assembly-qualified name of the interface type for validation and unambiguous identification in the assembly.
        /// </summary>
        [SerializeField] private string _interfaceTypeName;

        /// <summary>
        /// Кэшированная ссылка на интерфейс для предотвращения повторных операций приведения типов и поиска компонентов.
        /// <br/><br/>
        /// Cached reference to the interface to avoid repeated type casting and component lookup operations.
        /// </summary>
        private TInterface _cachedReference;

        /// <summary>
        /// Инициализирует новый экземпляр провайдера ссылок на основе переданного объекта интерфейса.
        /// <br/><br/>
        /// Initializes a new instance of the reference provider based on the specified interface object.
        /// </summary>
        /// <param name="target">Объект, реализующий целевой интерфейс. / The object implementing the target interface.</param>
        public ReferenceProvider(TInterface target)
        {
            if (target is UnityEngine.Object obj)
            {
                _targetObject = obj;
                _interfaceTypeName = typeof(TInterface).AssemblyQualifiedName;
            }
        }

        /// <summary>
        /// Возвращает реализацию интерфейса. При первом обращении автоматически приводит тип целевого объекта или извлекает нужный компонент из GameObject.
        /// <br/><br/>
        /// Returns the interface implementation. On the first access, automatically casts the target object type or extracts the required component from the GameObject.
        /// </summary>
        public TInterface Value
        {
            get
            {
                if (_cachedReference == null && _targetObject != null)
                {
                    _cachedReference = _targetObject as TInterface;
                    if (_cachedReference == null && _targetObject is GameObject go)
                    {
                        _cachedReference = go.GetComponent<TInterface>();
                    }
                }
                return _cachedReference;
            }
        }
    }
}
