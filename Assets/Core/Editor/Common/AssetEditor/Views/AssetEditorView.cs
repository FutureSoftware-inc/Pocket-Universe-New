using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crystal.Common.Editor
{
    public abstract class AssetEditorView
    {
        protected EditorWindow HostWindow { get; private set; }

        public VisualElement Root { get; private set; }

        public void Initialize(EditorWindow hostWindow)
        {
            HostWindow = hostWindow;
            Root = new VisualElement { name = "ViewRoot" };
            Root.style.flexGrow = 1;

            OnInitialize();
        }

        // Универсальный метод создания файлов, доступный для ВСЕХ наследников вида!
        protected void CreateNewAsset<T>(string defaultFileName) where T : ScriptableObject
        {
            // 1. Через рефлексию или прямое приведение запрашиваем тип ассета у главного окна
            Type assetType = typeof(T);

            // 2. Вызываем наш готовый механизм получения пути, привязанный к типу ассета
            string defaultFolder = AssetPathSelector.GetDefaultPathForAsset(assetType);

            // 3. Открываем нативный проводник Unity сразу в нужной дефолтной папке
            string path = EditorUtility.SaveFilePanelInProject(
                $"Создать {defaultFileName}",
                $"New{defaultFileName}",
                "asset",
                "Выберите место для сохранения ассета",
                defaultFolder
            );

            if (string.IsNullOrEmpty(path)) return;

            // 4. Генерируем инстанс ScriptableObject на лету и сохраняем в базу данных Unity
            T newAsset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();

            // 5. Автоматически скармливаем только что созданный ассет текущему открытому виду
            OpenAsset(newAsset);
        }

        protected abstract void OnInitialize();

        public abstract void OpenAsset(ScriptableObject asset);

        public abstract void OnDisable();
    }
}