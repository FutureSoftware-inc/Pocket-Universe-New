using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    /// <summary>
    /// Кастомный UI-компонент кнопки со значком C# скрипта для мгновенного открытия файла исходного кода класса.
    /// </summary>
    public sealed class OpenScriptButtonComponent : VisualComponent<Button>
    {
        private readonly Func<Type> _getCurrentType;

        /// <summary>
        /// Инициализирует компонент кнопки открытия скрипта.
        /// </summary>
        /// <param name="getCurrentType">Делегат, возвращающий текущий тип полиморфного объекта для поиска файла.</param>
        public OpenScriptButtonComponent(Func<Type> getCurrentType) : base()
        {
            _getCurrentType = getCurrentType ?? throw new ArgumentNullException(nameof(getCurrentType));

            // Безопасно подписываемся на клик, который запускает внутренний метод поиска и открытия файла
            Root.clickable.clicked += OnButtonClicked;
        }

        /// <summary>
        /// Этап 1: Настройка внешнего вида и загрузка встроенной иконки Unity "cs Script Icon".
        /// </summary>
        protected override void SetBaseStyle()
        {
            Root.style.width = 20f;
            Root.style.height = 18f;
            Root.style.marginLeft = 2f;
            Root.style.paddingLeft = 0f;
            Root.style.paddingRight = 0f;

            Texture2D scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            if (scriptIcon != null)
            {
                Root.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                Root.style.backgroundImage = scriptIcon;
            }
            else
            {
                Root.text = "#"; // Запасной текстовый вариант, если иконка Unity не загрузилась
            }
        }

        /// <summary>
        /// Этап 2: У кнопки нет вложенной структуры UI элементов.
        /// </summary>
        protected override void BuildStructure() { }

        /// <summary>
        /// Этап 3: Обновление видимости кнопки (скрыта, если полиморфное поле пустое).
        /// </summary>
        /// <param name="hasValue">True, если объект не null.</param>
        public void Refresh(bool hasValue)
        {
            Root.style.display = hasValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Внутренний обработчик клика. Логика поиска MonoScript перенесена сюда из PropertyDrawer.
        /// </summary>
        private void OnButtonClicked()
        {
            Type type = _getCurrentType();
            if (type == null) return;

            // Разворачиваем generic-определения для правильного поиска файла ассета
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            string searchName = type.Name.Contains('`') ? type.Name.Split('`')[0] : type.Name;
            string[] guids = AssetDatabase.FindAssets($"{searchName} t:MonoScript");

            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning($"[OpenScriptButton] Не удалось найти C# файл для класса: {searchName}");
                return;
            }

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (script != null && script.GetClass() == type)
                {
                    AssetDatabase.OpenAsset(script);
                    return;
                }
            }

            // Если точное совпадение класса не найдено, открываем первый попавшийся по имени файл (как бэкап)
            string backupPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            MonoScript backupScript = AssetDatabase.LoadAssetAtPath<MonoScript>(backupPath);
            if (backupScript != null)
            {
                AssetDatabase.OpenAsset(backupScript);
            }
        }
    }
}
