using System;
using UnityEngine;

namespace Crystal.Common.Editor
{
    public static class ReferenceFactory
    {
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
                               $"Убедитесь, что у класса есть конструктор по умолчанию без параметров. Ошибка: {exception.Message}");
                return null;
            }
        }
    }
}
