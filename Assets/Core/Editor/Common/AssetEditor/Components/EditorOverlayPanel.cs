using UnityEngine;
using UnityEngine.UIElements;

namespace CrystalEditor
{
    public sealed class EditorOverlayPanel : VisualElement
    {
        private VisualElement _header;
        private VisualElement _content;
        private VisualElement _resizer;

        private bool _isDragging;
        private bool _isResizing;
        private Vector2 _resizeStartSize;
        private Vector2 _resizeStartMousePos;
        private Vector2 _dragStartOffset;

        public string Title { get; }

        public EditorOverlayPanel(string title)
        {
            Title = title;
            SetBaseStyle();
            BuildInternalStructures();
            RegisterDragEvents();
            RegisterResizeEvents();
        }

        public new void Add(VisualElement childElement)
        {
            if (childElement != null && _content != null)
            {
                _content.Add(childElement);
            }
        }

        private void SetBaseStyle()
        {
            style.position = Position.Absolute;
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            style.borderLeftWidth = 1; style.borderLeftColor = new Color(0.12f, 0.12f, 0.12f);
            style.borderRightWidth = 1; style.borderRightColor = new Color(0.12f, 0.12f, 0.12f);
            style.borderTopWidth = 1; style.borderTopColor = new Color(0.12f, 0.12f, 0.12f);
            style.borderBottomWidth = 1; style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f);
            style.borderBottomRightRadius = 4;
            style.borderBottomLeftRadius = 4;
        }

        private void BuildInternalStructures()
        {
            CreateHeader();
            CreateContent();
            CreateResizer();
        }

        private void CreateHeader()
        {
            _header = new VisualElement();
            _header.style.height = 24;
            _header.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            _header.style.flexDirection = FlexDirection.Row;
            _header.style.alignItems = Align.Center;
            _header.style.paddingLeft = 6;

            Label titleLabel = new Label(Title)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 }
            };
            _header.Add(titleLabel);
            Add(_header);
        }

        private void CreateContent()
        {
            _content = new VisualElement();
            _content.style.flexGrow = 1;
            _content.style.paddingLeft = 6;
            _content.style.paddingRight = 6;
            _content.style.paddingTop = 6;
            _content.style.paddingBottom = 6;
            Add(_content);
        }

        private void CreateResizer()
        {
            _resizer = new VisualElement();
            _resizer.style.width = 10;
            _resizer.style.height = 10;
            _resizer.style.position = Position.Absolute;
            _resizer.style.bottom = 0;
            _resizer.style.right = 0;
            _resizer.AddToClassList("unity-resizable-element__resizer");
            Add(_resizer);
        }

        private void RegisterDragEvents()
        {
            if (_header == null) return;
            _header.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                _isDragging = true;
                _dragStartOffset = evt.localMousePosition;
                _header.CaptureMouse();
                evt.StopPropagation();
            });

            _header.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!_isDragging) return;
                Vector2 mouseInParentSpace = parent.WorldToLocal(_header.LocalToWorld(evt.localMousePosition));
                style.left = mouseInParentSpace.x - _dragStartOffset.x;
                style.top = mouseInParentSpace.y - _dragStartOffset.y;
            });

            _header.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!_isDragging) return;
                _isDragging = false;
                _header.ReleaseMouse();
            });
        }

        private void RegisterResizeEvents()
        {
            if (_resizer == null) return;
            _resizer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                _isResizing = true;
                _resizeStartSize = new Vector2(resolvedStyle.width, resolvedStyle.height);
                _resizeStartMousePos = evt.mousePosition;
                _resizer.CaptureMouse();
                evt.StopPropagation();
            });

            _resizer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!_isResizing) return;
                Vector2 deltaMouse = evt.mousePosition - _resizeStartMousePos;
                float newWidth = _resizeStartSize.x + deltaMouse.x;
                float newHeight = _resizeStartSize.y + deltaMouse.y;
                style.width = Mathf.Max(newWidth, 150f);
                style.height = Mathf.Max(newHeight, 100f);
                evt.StopPropagation();
            });

            _resizer.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!_isResizing) return;
                _isResizing = false;
                _resizer.ReleaseMouse();
                evt.StopPropagation();
            });
        }
    }
}