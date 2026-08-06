using System;
using System.Text;
using UnityEditor;

namespace CrystalEngineEditor
{
    public static class PropertyNameFormatter
    {
        public const string NullText = "None (Null)";

        /// <summary>
        /// Самый надежный форматтер имен дженериков. Полностью вырезает системные апострофы.
        /// </summary>
        public static string GetGenericName(this Type type)
        {
            if (type == null) return string.Empty;

            // Если это не дженерик, просто страхуемся от случайных апострофов в системных именах
            if (!type.IsGenericType)
            {
                return CleanApostrophe(type.Name);
            }

            StringBuilder sb = new StringBuilder();
            string name = type.Name;

            int indexOfApostrophe = name.IndexOf('`');
            if (indexOfApostrophe > 0)
            {
                sb.Append(name.Substring(0, indexOfApostrophe));
            }
            else
            {
                sb.Append(name);
            }

            sb.Append("<");

            Type[] genericArguments = type.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                sb.Append(genericArguments[i].GetGenericName());

                if (i < genericArguments.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(">");
            return sb.ToString();
        }

        /// <summary>
        /// Принудительно очищает любую строку от рантайм-суффиксов дженериков (на всякий случай).
        /// </summary>
        public static string CleanApostrophe(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return string.Empty;

            int index = rawName.IndexOf('`');
            if (index > 0)
            {
                return rawName.Substring(0, index);
            }
            return rawName;
        }

        public static Type GetFieldType(this SerializedProperty property)
        {
            if (property == null) return null;

            string fullTypeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(fullTypeName)) return null;

            string[] split = fullTypeName.Split(' ', 2);
            if (split.Length < 2) return null;

            string assemblyName = split[0];
            string typeName = split[1];

            return Type.GetType($"{typeName}, {assemblyName}");
        }

        public static string GetDisplayValueName(this SerializedProperty property)
        {
            if (property == null || property.managedReferenceValue == null)
            {
                return NullText;
            }

            Type type = property.managedReferenceValue.GetType();

            // Проверяем, может у типа есть кастомный атрибут SelectorName?
            // Если геймдизайнер написал [SelectorName("Input Condition`1")], мы тоже очистим этот апостроф!
            var selectorNameAttr = type.GetCustomAttribute<CrystalEngine.SelectorNameAttribute>();
            if (selectorNameAttr != null && !string.IsNullOrEmpty(selectorNameAttr.Name))
            {
                return CleanApostrophe(selectorNameAttr.Name);
            }

            return type.GetGenericName();
        }

        // Вспомогательный метод рефлексии для безопасного извлечения атрибутов без аллокаций
        private static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            return Attribute.GetCustomAttribute(type, typeof(T)) as T;
        }
    }
}
