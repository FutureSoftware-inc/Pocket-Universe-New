using System;
using System.Collections.Generic;

namespace CrystalEngine
{
    /// <summary>
    /// Компонент "доски данных" (Blackboard) для динамического хранения и обмена данными между системами.
    /// Использует статическое типизированное хранилище во избежание упаковки значений (boxing) разнородных типов.
    /// Переведен на хэшированные int-ключи для обеспечения максимальной производительности в Update-кадрах.
    /// <br/><br/>
    /// A Blackboard component for dynamically storing and sharing data between systems.
    /// Uses static typed storage to prevent boxing of heterogeneous value types.
    /// Upgraded to hashed int-keys to ensure maximum performance in Update frames.
    /// </summary>
    public sealed class Blackboard
    {
        /// <summary>
        /// Кэш для хранения вычисленных хэшей строк во избежание повторных расчетов в рантайме.
        /// <br/><br/>
        /// Cache to store calculated string hashes and avoid recalculations in runtime.
        /// </summary>
        private static readonly Dictionary<string, int> _stringToHashCache = new(128);

        /// <summary>
        /// Реестр действий очистки для всех дженерик-типов хранилищ для предотвращения утечек памяти.
        /// <br/><br/>
        /// Registry of clear actions for all generic storage types to prevent memory leaks.
        /// </summary>
        private static readonly List<Action<Blackboard>> _clearActions = new();

        /// <summary>
        /// Внутреннее статическое хранилище, изолированное под каждый конкретный тип данных <typeparamref name="T"/>.
        /// Связывает экземпляры Blackboard с их персональными словарями "ключ-значение".
        /// <br/><br/>
        /// Internal static storage isolated for each specific data type <typeparamref name="T"/>.
        /// Maps Blackboard instances to their personal key-value dictionaries.
        /// </summary>
        /// <typeparam name="T">Тип сохраняемых данных.<br/><br/>The type of data being stored.</typeparam>
        private static class Storage<T>
        {
            /// <summary>
            /// Глобальный реестр данных, где для каждого экземпляра Blackboard хранится свой словарь значений типа <typeparamref name="T"/>.
            /// <br/><br/>
            /// Global data registry where each Blackboard instance holds its own dictionary of values of type <typeparamref name="T"/>.
            /// </summary>
            public static readonly Dictionary<Blackboard, Dictionary<int, T>> Data = new();

            static Storage()
            {
                _clearActions.Add(instance =>
                {
                    if (Data.TryGetValue(instance, out var registry))
                    {
                        registry.Clear();
                    }
                });
            }

            /// <summary>
            /// Возвращает или создает реестр данных типа <typeparamref name="T"/> для указанного экземпляра Blackboard.
            /// <br/><br/>
            /// Retrieves or creates a data registry of type <typeparamref name="T"/> for the specified Blackboard instance.
            /// </summary>
            /// <param name="instance">Экземпляр доски данных Blackboard.<br/><br/>The Blackboard instance.</param>
            /// <returns>Словарь "ключ-значение" для хранения данных типа <typeparamref name="T"/>.<br/><br/>A key-value dictionary for storing data of type <typeparamref name="T"/>.</returns>
            public static Dictionary<int, T> GetRegistry(Blackboard instance)
            {
                if (!Data.TryGetValue(instance, out var registry))
                {
                    registry = new Dictionary<int, T>(32);
                    Data[instance] = registry;
                }
                return registry;
            }
        }

        /// <summary>
        /// Конвертирует строковый ключ в уникальный числовой идентификатор (Хэш).
        /// <br/><br/>
        /// Converts a string key into a unique numerical identifier (Hash).
        /// </summary>
        /// <param name="key">Уникальный строковый идентификатор.<br/><br/>The unique string identifier.</param>
        /// <returns>Числовой хэш строки.<br/><br/>The numerical hash of the string.</returns>
        public static int GetHash(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            if (!_stringToHashCache.TryGetValue(key, out int hash))
            {
                hash = key.GetHashCode();
                _stringToHashCache[key] = hash;
            }
            return hash;
        }

        /// <summary>
        /// Сохраняет или обновляет значение типа <typeparamref name="T"/> по заданному числовому хэшу ключа.
        /// <br/><br/>
        /// Stores or updates a value of type <typeparamref name="T"/> under the specified key hash.
        /// </summary>
        /// <typeparam name="T">Тип записываемого значения.<br/><br/>The type of the value being set.</typeparam>
        /// <param name="keyId">Хэшированный идентификатор ключа.<br/><br/>The hashed key identifier.</param>
        /// <param name="value">Записываемое значение.<br/><br/>The value to store.</param>
        public void Set<T>(int keyId, T value)
        {
            Storage<T>.GetRegistry(this)[keyId] = value;
        }

        /// <summary>
        /// Сохраняет или обновляет значение типа <typeparamref name="T"/> по заданному текстовому ключу.
        /// <br/><br/>
        /// Stores or updates a value of type <typeparamref name="T"/> under the specified text key.
        /// </summary>
        /// <typeparam name="T">Тип записываемого значения.<br/><br/>The type of the value being set.</typeparam>
        /// <param name="key">Уникальный строковый идентификатор.<br/><br/>The unique string identifier.</param>
        /// <param name="value">Записываемое значение.<br/><br/>The value to store.</param>
        public void Set<T>(string key, T value)
        {
            Set(GetHash(key), value);
        }

        /// <summary>
        /// Возвращает значение типа <typeparamref name="T"/> по хэшу ключа. Если ключ отсутствует, возвращает значение по умолчанию.
        /// <br/><br/>
        /// Retrieves a value of type <typeparamref name="T"/> by its key hash. Returns a default value if the key does not exist.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого значения.<br/><br/>The type of the value to retrieve.</typeparam>
        /// <param name="keyId">Хэшированный идентификатор ключа.<br/><br/>The hashed key identifier.</param>
        /// <param name="defaultValue">Значение, возвращаемое при отсутствии ключа.<br/><br/>The value returned if the key is not found.</param>
        /// <returns>Найденное значение или значение по умолчанию.<br/><br/>The retrieved value or the default value.</returns>
        public T Get<T>(int keyId, T defaultValue = default)
        {
            var registry = Storage<T>.GetRegistry(this);
            return registry.TryGetValue(keyId, out T value) ? value : defaultValue;
        }

        /// <summary>
        /// Возвращает значение типа <typeparamref name="T"/> по ключу. Если ключ отсутствует, возвращает значение по умолчанию.
        /// <br/><br/>
        /// Retrieves a value of type <typeparamref name="T"/> by its key. Returns a default value if the key does not exist.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого значения.<br/><br/>The type of the value to retrieve.</typeparam>
        /// <param name="key">Уникальный строковый идентификатор.<br/><br/>The unique string identifier.</param>
        /// <param name="defaultValue">Значение, возвращаемое при отсутствии ключа.<br/><br/>The value returned if the key is not found.</param>
        /// <returns>Найденное значение или значение по умолчанию.<br/><br/>The retrieved value or the default value.</returns>
        public T Get<T>(string key, T defaultValue = default)
        {
            return Get(GetHash(key), defaultValue);
        }

        /// <summary>
        /// Проверяет, зарегистрировано ли на этой доске значение типа <typeparamref name="T"/> с указанным хэшем ключа.
        /// <br/><br/>
        /// Checks if a value of type <typeparamref name="T"/> with the specified key hash is registered on this blackboard.
        /// </summary>
        /// <typeparam name="T">Тип проверяемого значения.<br/><br/>The type of the value to check.</typeparam>
        /// <param name="keyId">Хэшированный идентификатор ключа.<br/><br/>The hashed key identifier.</param>
        /// <returns>True, если ключ найден; иначе false.<br/><br/>True if the key is found; otherwise, false.</returns>
        public bool HasKey<T>(int keyId)
        {
            return Storage<T>.GetRegistry(this).ContainsKey(keyId);
        }

        /// <summary>
        /// Проверяет, зарегистрировано ли на этой доске значение типа <typeparamref name="T"/> с указанным ключом.
        /// <br/><br/>
        /// Checks if a value of type <typeparamref name="T"/> with the specified key is registered on this blackboard.
        /// </summary>
        /// <typeparam name="T">Тип проверяемого значения.<br/><br/>The type of the value to check.</typeparam>
        /// <param name="key">Уникальный строковый идентификатор.<br/><br/>The unique string identifier.</param>
        /// <returns>True, если ключ найден; иначе false.<br/><br/>True if the key is found; otherwise, false.</returns>
        public bool HasKey<T>(string key)
        {
            return HasKey<T>(GetHash(key));
        }

        /// <summary>
        /// Очищает данные. Метод предназначен для удаления кэша, чтобы исключить утечки памяти и накопление ссылок в Storage.
        /// <br/><br/>
        /// Clears data. This method is intended to clear the cache to prevent memory leaks and accumulation of references in Storage.
        /// </summary>
        public void ClearAll()
        {
            for (int i = 0; i < _clearActions.Count; i++)
            {
                _clearActions[i].Invoke(this);
            }
        }
    }
}
