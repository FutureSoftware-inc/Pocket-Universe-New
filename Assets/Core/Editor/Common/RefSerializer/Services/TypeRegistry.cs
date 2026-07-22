using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CrystalEditor
{
    /// <summary>
    /// Глобальный реестр типов редактора, обеспечивающий кэширование и быстрый доступ к реализациям классов и их метаданным.
    /// Использует оптимизированный внутренний API Unity <see cref="TypeCache"/> для ускорения индексации сборок проекта.
    /// <br/><br/>
    /// Global editor type registry providing caching and fast access to class implementations and their metadata.
    /// Leverages Unity's optimized internal <see cref="TypeCache"/> API to accelerate project assembly indexing.
    /// </summary>
    public static class TypeRegistry
    {
        private static readonly Dictionary<Type, List<Type>> _typeCache = new();
        private static readonly Dictionary<Type, Dictionary<Type, TypeMetadataExtension>> _metadataCache = new();

        /// <summary>
        /// Автоматически вызывается при компиляции или запуске редактора Unity для очистки и инвалидации кэша типов.
        /// <br/><br/>
        /// Automatically invoked upon compilation or Unity Editor startup to clear and invalidate the type cache.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void ClearCache()
        {
            _typeCache.Clear();
        }

        /// <summary>
        /// Возвращает список всех валидных (конкретных и неабстрактных) реализаций для указанного базового типа или интерфейса.
        /// Корректно разворачивает закрытые обобщенные типы до их определений дженериков для правильного поиска.
        /// <br/><br/>
        /// Returns a list of all valid (concrete and non-abstract) implementations for the specified base type or interface.
        /// Correctly unwraps closed generic types to their generic type definitions for accurate lookup.
        /// </summary>
        /// <param name="baseType">Базовый тип или интерфейс для поиска наследников. Не может быть null. / The base type or interface to find inheritors for. Cannot be null.</param>
        /// <returns>Список доступных типов-реализаций. / A list of available implementation types.</returns>
        /// <exception cref="ArgumentNullException">Вызывается, если переданный параметр <paramref name="baseType"/> равен null. / Thrown when the specified <paramref name="baseType"/> parameter is null.</exception>
        public static IReadOnlyList<Type> GetImplementations(Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }
            Type searchType = baseType.IsGenericType && !baseType.IsGenericTypeDefinition ? baseType.GetGenericTypeDefinition() : baseType;
            if (_typeCache.TryGetValue(searchType, out List<Type> cachedTypes))
            {
                return cachedTypes;
            }
            List<Type> derivedTypes = TypeCache.GetTypesDerivedFrom(searchType).Where(IsValidImplementation).ToList();
            if (IsValidImplementation(searchType) && !derivedTypes.Contains(searchType))
            {
                derivedTypes.Insert(0, searchType);
            }
            _typeCache[searchType] = derivedTypes;
            return derivedTypes;
        }

        /// <summary>
        /// Возвращает или лениво инициализирует расширение метаданных указанного типа <typeparamref name="T"/> относительно базового типа.
        /// <br/><br/>
        /// Retrieves or lazily initializes the metadata extension of the specified type <typeparamref name="T"/> relative to the base type.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого расширения метаданных, наследуемый от TypeMetadataExtension. / The type of the requested metadata extension, derived from TypeMetadataExtension.</typeparam>
        /// <param name="type">Исследуемый тип реализации. / The implementation type to inspect.</param>
        /// <param name="baseType">Базовый тип или интерфейс поля контекста. / The base type or interface of the context field.</param>
        /// <returns>Экземпляр расширения метаданных или null, если расширение не найдено. / The metadata extension instance, or null if not found.</returns>
        public static T GetExtension<T>(Type type, Type baseType) where T : TypeMetadataExtension
        {
            if (!_metadataCache.TryGetValue(type, out var extensions))
            {
                extensions = new Dictionary<Type, TypeMetadataExtension>();

                foreach (var factory in _extensionFactories)
                {
                    var extension = factory();
                    extension.Initialize(type, baseType);
                    extensions[extension.GetType()] = extension;
                }

                _metadataCache[type] = extensions;
            }

            return extensions.TryGetValue(typeof(T), out var result) ? (T)result : null;
        }

        private static readonly List<Func<TypeMetadataExtension>> _extensionFactories = new()
        {
            () => new PathMetadataExtension(),
            () => new DisplayNameMetadataExtension(),
            () => new TooltipMetadataExtension(),
            () => new IconMetadataExtension()
        };

        /// <summary>
        /// Проверяет, является ли тип валидной реализацией для инспектора (исключает интерфейсы и абстрактные классы).
        /// <br/><br/>
        /// Validates whether the type is a valid implementation for the Inspector (excludes interfaces and abstract classes).
        /// </summary>
        /// <param name="type">Проверяемый системный тип. / The system type to validate.</param>
        /// <returns>True, если тип не абстрактный и не интерфейс; иначе false. / True if the type is neither abstract nor an interface; otherwise, false.</returns>
        private static bool IsValidImplementation(Type type)
        {
            return !type.IsInterface && !type.IsAbstract;
        }
    }
}