namespace CrystalEngine
{
    /// <summary>
    /// Перечисление типов проверки пользовательского ввода клавиш.
    /// Определяет фазу нажатия кнопки для обработки в условиях переходов машины состояний.
    /// <br/><br/>
    /// Enumeration of input check types for user key presses.
    /// Determines the key interaction phase to process within state machine transition conditions.
    /// </summary>
    public enum InputCheckType : byte
    {
        /// <summary>
        /// Проверка на мгновенное нажатие клавиши в текущем кадре (GetKeyDown).
        /// <br/><br/>
        /// Checks for the exact frame the key was pressed down (GetKeyDown).
        /// </summary>
        Down = 0,

        /// <summary>
        /// Проверка на удержание или непрерывное нажатие клавиши (GetKey).
        /// <br/><br/>
        /// Checks for continuous holding or pressing of the key (GetKey).
        /// </summary>
        Pressed = 1,

        /// <summary>
        /// Проверка на мгновенное отпускание клавиши в текущем кадре (GetKeyUp).
        /// <br/><br/>
        /// Checks for the exact frame the key was released (GetKeyUp).
        /// </summary>
        Up = 2
    }
}
