using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEngineEditor
{
    public sealed class LibraryGridComponent<TAsset> : VisualComponent<ScrollView> where TAsset : ScriptableObject
    {
        private readonly Func<string> _getSearchPath;
        private readonly Action<TAsset> _onAssetSelected;
        private VisualElement _cardsContainer;

        public LibraryGridComponent(Func<string> getSearchPath, Action<TAsset> onAssetSelected) : base()
        {
            _getSearchPath = getSearchPath ?? throw new ArgumentNullException(nameof(getSearchPath));
            _onAssetSelected = onAssetSelected ?? throw new ArgumentNullException(nameof(onAssetSelected));
        }

        protected override void SetBaseStyle()
        {
            Root.name = "LibraryGrid";
            Root.style.flexGrow = 1;
            Root.style.paddingTop = 10;
            Root.style.paddingLeft = 10;
        }

        protected override void BuildStructure()
        {
            // Создаем внутренний контейнер с поддержкой автоматического переноса строк (Wrap)
            _cardsContainer = new VisualElement();
            _cardsContainer.style.flexDirection = FlexDirection.Row;
            _cardsContainer.style.flexWrap = Wrap.Wrap;
            Root.Add(_cardsContainer);
        }

        public override void Refresh()
        {
            if (_cardsContainer == null) return;
            _cardsContainer.Clear();

            string currentFolder = _getSearchPath.Invoke();
            if (string.IsNullOrEmpty(currentFolder)) currentFolder = "Assets";

            string[] searchFolders = new string[] { currentFolder };

            // Универсальный нативный поиск по переданному типу TAsset
            string filter = $"t:{typeof(TAsset).Name}";
            string[] guids = AssetDatabase.FindAssets(filter, searchFolders);

            if (guids.Length == 0)
            {
                RenderEmptyState(currentFolder);
                return;
            }

            // Внутри метода Refresh() класса LibraryGridComponent<TAsset>:
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // ИСПРАВЛЕНИЕ: Используем универсальный безопасный метод загрузки с контролем ошибок
                TAsset asset = SaveLoadService.Load<TAsset>(path);

                if (asset == null) continue;

                RenderAssetCard(asset);
            }
        }

        private void RenderEmptyState(string folderPath)
        {
            Label emptyLabel = new Label($"In the folder '{folderPath}' no {typeof(TAsset).Name} assets found.")
            {
                style = { color = new Color(0.5f, 0.5f, 0.5f), marginTop = 20, marginLeft = 10 }
            };
            _cardsContainer.Add(emptyLabel);
        }

        private void RenderAssetCard(TAsset asset)
        {
            Button assetCard = new Button(() => _onAssetSelected?.Invoke(asset))
            {
                text = asset.name
            };

            // Стандартизированный нативный стиль карточки
            assetCard.style.width = 100;
            assetCard.style.height = 100;
            assetCard.style.marginRight = 10;
            assetCard.style.marginBottom = 10;
            assetCard.style.unityTextAlign = TextAnchor.MiddleCenter;

            _cardsContainer.Add(assetCard);
        }
    }
}
