using System;
using UnityEngine;

namespace CrystalEngine
{
    /// <summary>
    /// Структура данных, представляющая метаданные узла для отображения и позиционирования в окне редактора GraphView.
    /// <br/><br/>
    /// A data structure representing node metadata for visualization and positioning inside the GraphView editor window.
    /// </summary>
    [Serializable]
    public struct GraphNodeData
    {
        /// <summary>
        /// Уникальный глобальный идентификатор узла (GUID).
        /// <br/><br/>
        /// The unique global identifier (GUID) of the node.
        /// </summary>
        [SerializeField] private string _guid;

        /// <summary>
        /// Двумерные координаты положения узла на холсте редактора GraphView.
        /// <br/><br/>
        /// The two-dimensional coordinates of the node's position on the GraphView editor canvas.
        /// </summary>
        [SerializeField] private Vector2 _position;

        /// <summary>
        /// Отображаемое имя (название) узла в интерфейсе графа.
        /// <br/><br/>
        /// The display name of the node in the graph interface.
        /// </summary>
        [SerializeField] private string _nodeName;

        /// <summary>
        /// Возвращает уникальный идентификатор узла.
        /// <br/><br/>
        /// Gets the unique identifier of the node.
        /// </summary>
        public string Guid => _guid;

        /// <summary>
        /// Возвращает позицию узла на холсте.
        /// <br/><br/>
        /// Gets the position of the node on the canvas.
        /// </summary>
        public Vector2 Position => _position;

        /// <summary>
        /// Возвращает отображаемое имя узла.
        /// <br/><br/>
        /// Gets the display name of the node.
        /// </summary>
        public string NodeName => _nodeName;

        /// <summary>
        /// Инициализирует новый экземпляр данных узла графа с указанием его идентификатора, позиции и имени.
        /// <br/><br/>
        /// Initializes a new instance of graph node data with the specified identifier, position, and name.
        /// </summary>
        /// <param name="guid">Уникальный идентификатор узла. Не может быть null. / The unique identifier of the node. Cannot be null.</param>
        /// <param name="position">Позиция узла на холсте редактора. / The position of the node on the editor canvas.</param>
        /// <param name="name">Отображаемое имя узла. / The display name of the node.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если переданный <paramref name="guid"/> равен null. / Thrown when the specified <paramref name="guid"/> is null.</exception>
        public GraphNodeData(string guid, Vector2 position, string name)
        {
            _guid = guid ?? throw new ArgumentNullException(nameof(guid));
            _position = position;
            _nodeName = name;
        }
    }
}