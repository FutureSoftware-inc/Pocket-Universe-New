using System;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Вспомогательный фабричный класс для динамического создания экземпляров типов данных в редакторе Unity.
    /// <br/><br/>
    /// A helper factory class for dynamically creating instances of data types within the Unity Editor.
    /// </summary>
    public static class ReferenceFactory
    {
        /// <summary>
        /// Создает экземпляр указанного типа, используя его конструктор по умолчанию (без параметров).
        /// В случае неудачи выводит подробное сообщение об ошибке в консоль Unity.
        /// <br/><br/>
        /// Creates an instance of the specified type using its default (parameterless) constructor.
        /// Logs a detailed error message to the Unity console in case of failure.
        /// </summary>
        /// <param name="type">Системный тип создаваемого объекта. Не может быть null. / The system type of the object to create. Cannot be null.</param>
        /// <returns>Новый экземпляр объекта или null при возникновении ошибки. / A new instance of the object, or null if an error occurs.</returns>
        /// <exception cref="ArgumentNullException">Вызывается, если переданный параметр <paramref name="type"/> равен null. / Thrown when the specified <paramref name="type"/> parameter is null.</exception>
        public static object CreateInstance(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[ReferenceFactory] Не удалось создать экземпляр типа {type.FullName}. " +
                               $"Убедитесь, что у класса есть... Ошибка: {exception.Message}");
                return null;
            }
        }
    }
}