using System;
using System.Collections.Generic;

namespace CrystalEngine.DI
{
    internal static class DIErrorFormatter
    {
        /// <summary>
        /// Добавляет дескриптор связи в потокобезопасный список регистраций по указанному типу-ключу.
        /// <br/><br/>
        /// Adds a binding descriptor to the thread-safe registry list under the specified key type.
        /// </summary>
        /// <param name="key">Тип-ключ, по которому будет производиться поиск зависимости.<br/><br/>The key type under which the dependency will be looked up.</param>
        /// <param name="binding">Дескриптор связи, содержащий метаданные регистрации.<br/><br/>The binding descriptor containing registration metadata.</param>
        internal static string BuildResolutionTraceError(Type failedType, Stack<Type> resolutionStack)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<color=red>[DI Trace Error] Не удалось разрешить зависимость для типа: <b>{failedType.Name}</b></color>");
            sb.AppendLine("Цепочка вызовов (Resolution Trace):");
            Type[] trace = resolutionStack.ToArray();
            Array.Reverse(trace);
            for (int i = 0; i < trace.Length; i++)
            {
                sb.Append($"  {trace[i].Name}");
                if (i < trace.Length - 1) sb.Append(" <b>-></b> ");
            }
            sb.AppendLine(" <b>-></b> <color=red>[ЗДЕСЬ ОШИБКА!]</color>");
            return sb.ToString();
        }
    }
}