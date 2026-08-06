using System;
using System.Collections.Generic;

namespace CrystalEngineEditor
{
    /// <summary>
    /// Сервис-контекст для логического разрешения, валидации и сборки закрытых generic-типов.
    /// Изолирует математику конструирования типов от UI-слоя.
    /// </summary>
    public sealed class GenericParameterContext
    {
        private readonly Type _openGenericType;
        private readonly List<Type> _resolvedArguments;
        private readonly int _expectedArgumentsCount;

        /// <summary>
        /// Возвращает true, если все необходимые generic-параметры успешно выбраны.
        /// </summary>
        public bool IsFullyResolved => _resolvedArguments.Count == _expectedArgumentsCount;

        /// <summary>
        /// Список уже выбранных аргументов.
        /// </summary>
        public IReadOnlyList<Type> ResolvedArguments => _resolvedArguments;

        public GenericParameterContext(Type openGenericType)
        {
            if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));

            if (!openGenericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"Type '{openGenericType.Name}' must be an open generic definition.", nameof(openGenericType));
            }

            _openGenericType = openGenericType;
            _resolvedArguments = new List<Type>();
            _expectedArgumentsCount = openGenericType.GetGenericArguments().Length;
        }

        /// <summary>
        /// Добавляет выбранный тип в качестве следующего аргумента дженерика.
        /// </summary>
        public void AddArgument(Type argumentType)
        {
            if (IsFullyResolved)
            {
                throw new InvalidOperationException("All generic arguments are already resolved.");
            }

            _resolvedArguments.Add(argumentType ?? throw new ArgumentNullException(nameof(argumentType)));
        }

        /// <summary>
        /// Очищает выбранные аргументы для повторного ввода.
        /// </summary>
        public void Clear()
        {
            _resolvedArguments.Clear();
        }

        /// <summary>
        /// Пытается автоматически закрыть тип на основе контекста базового поля (если базовое поле само по себе закрытый дженерик).
        /// </summary>
        public bool TryAutoResolveFromBase(Type baseFieldType, out Type closedType)
        {
            closedType = null;

            if (baseFieldType != null && baseFieldType.IsGenericType)
            {
                Type[] baseArgs = baseFieldType.GetGenericArguments();
                if (baseArgs.Length == _expectedArgumentsCount)
                {
                    try
                    {
                        closedType = _openGenericType.MakeGenericType(baseArgs);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Финальная сборка закрытого типа на основе вручную выбранных параметров.
        /// </summary>
        public Type AssembleClosedType()
        {
            if (!IsFullyResolved)
            {
                throw new InvalidOperationException($"Cannot assemble type. Resolved {_resolvedArguments.Count} out of {_expectedArgumentsCount} arguments.");
            }

            try
            {
                return _openGenericType.MakeGenericType(_resolvedArguments.ToArray());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to assemble closed generic type for '{_openGenericType.Name}'. Inner exception: {ex.Message}", ex);
            }
        }
    }
}
