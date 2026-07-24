using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrystalEditor
{
    /// <summary>
    /// Универсальный провайдер дерева поиска для спавна узлов на холсте GraphView.
    /// </summary>
    public sealed class GraphSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private Texture2D _indentIcon;
        private Action<Type, Vector2> _onTypeSelected;
        private Vector2 _targetGraphPosition;
        private Type[] _availableTypes;

        /// <summary>
        /// Инициализирует провайдер списком доступных C# типов и колбэком на выбор элемента.
        /// </summary>
        public void Initialize(Type[] availableTypes, Action<Type, Vector2> onTypeSelected)
        {
            _availableTypes = availableTypes;
            _onTypeSelected = onTypeSelected;

            // Прозрачная заглушка 1х1 для красивого выравнивания текста без иконок в подменю Unity
            _indentIcon = new Texture2D(1, 1);
            _indentIcon.SetPixel(0, 0, Color.clear);
            _indentIcon.Apply();
        }

        /// <summary>
        /// Фиксирует локальные координаты холста, куда кликнул пользователь.
        /// </summary>
        public void SetTargetPosition(Vector2 graphPosition)
        {
            _targetGraphPosition = graphPosition;
        }

        /// <summary>
        /// Создает структуру дерева поиска. Автоматически группирует узлы по нэймспейсам.
        /// </summary>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTree = new List<SearchTreeEntry>
            {
                // КРИТИЧЕСКИЙ ФИКС: Корень дерева (уровень 0) ОБЯЗАН быть группой
                new SearchTreeGroupEntry(new GUIContent("Create Node HFSM"), 0)
            };

            if (_availableTypes == null) return searchTree;

            // Список для отслеживания уже созданных папок подкатегорий, чтобы не дублировать их
            List<string> groups = new List<string>();

            foreach (Type type in _availableTypes)
            {
                // Базовая группировка: берем имя нэймспейса в качестве папки. 
                // Если нэймспейса нет или это CrystalEngine, кладем в корень меню.
                string categoryName = type.Namespace;
                if (string.IsNullOrEmpty(categoryName) || categoryName.Contains("CrystalEngine"))
                {
                    categoryName = "Common states";
                }
                else
                {
                    // Отрезаем лишние префиксы, если они есть (например, MyProject.States -> States)
                    int lastDot = categoryName.LastIndexOf('.');
                    if (lastDot != -1) categoryName = categoryName.Substring(lastDot + 1);
                }

                // Если такой группы еще нет в дереве, создаем её (уровень вложенности 1)
                if (!groups.Contains(categoryName))
                {
                    groups.Add(categoryName);
                    searchTree.Add(new SearchTreeGroupEntry(new GUIContent(categoryName), 1));
                }

                // Добавляем сам узел (уровень вложенности 2 — внутри папки)
                searchTree.Add(new SearchTreeEntry(new GUIContent(type.Name, _indentIcon))
                {
                    level = 2,
                    userData = type
                });
            }

            return searchTree;
        }

        /// <summary>
        /// Срабатывает, когда пользователь кликает по финальному элементу в окне поиска.
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is Type selectedType)
            {
                // Передаем выбранный тип и сохраненные ранее координаты холста обратно в представление
                _onTypeSelected?.Invoke(selectedType, _targetGraphPosition);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Высвобождает созданные ресурсы текстур для предотвращения утечек памяти в редакторе Unity.
        /// </summary>
        private void OnDestroy()
        {
            if (_indentIcon != null)
            {
                DestroyImmediate(_indentIcon);
                _indentIcon = null;
            }
        }
    }
}
