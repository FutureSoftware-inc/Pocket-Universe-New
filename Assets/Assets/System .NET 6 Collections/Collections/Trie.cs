// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a high-performance, iterative Prefix Tree (Trie) 
    /// optimized for memory layout and zero-allocation lookups.
    /// </summary>
    /// <typeparam name="TValue">The type of value associated with each string key.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class Trie<TValue>
    {
        private TrieNode<TValue> _root;
        private int _size;
        private int _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie{TValue}"/> class.
        /// </summary>
        public Trie()
        {
            _root = new TrieNode<TValue>();
        }

        /// <summary>
        /// Gets the total number of words (keys with values) stored in the Trie.
        /// </summary>
        public int Count => _size;

        /// <summary>
        /// Adds a string key and its associated value to the Trie. 
        /// If the key already exists, updates its value.
        /// </summary>
        public void Add(string key, TValue value)
        {
            key.ThrowIfNull(nameof(key));
            if (key.Length == 0)
            {
                throw new ArgumentException("Key cannot be empty.", nameof(key));
            }

            _version++;

            // Нам нужно итеративно спускаться по структурам. 
            // Поскольку структуры в C# передаются по значению (копируются), 
            // мы не можем просто сделать "TrieNode current = _root" и менять её детей.
            // Вместо этого мы будем работать с индексами массивов.

            // Начнем с корня. Нам нужен специальный костыль для итерации по структурам — 
            // ссылка на массивы текущего уровня.
            int rootChildIndex = _root.AddChild(key[0]);

            // Чтобы обойти ограничение копирования структур, мы напишем приватный итеративный хелпер,
            // который будет безопасно прокидывать изменения по цепочке массивов вверх.
            InsertInternal(key, value);
        }

        private void InsertInternal(string key, TValue value)
        {
            // Используем стек индексов, чтобы в конце итерации правильно собрать 
            // измененные копии структур обратно в их родительские массивы.
            // Размер стека равен максимальной длине ключа.
            int length = key.Length;
            int[] nodeIndices = new int[length];
            TrieNode<TValue>[] pathNodes = new TrieNode<TValue>[length];

            TrieNode<TValue> current = _root;

            for (int i = 0; i < length; i++)
            {
                char c = key[i];
                int index = current.AddChild(c);

                nodeIndices[i] = index;
                pathNodes[i] = current;

                current = current._childrenNodes[index];
            }

            // Устанавливаем значение в самой глубокой структуре
            if (!current._hasValue)
            {
                _size++;
            }
            current._value = value;
            current._hasValue = true;

            // Обратный проход (Backpropagation): сохраняем измененные структуры-значения 
            // обратно в массивы их родителей, так как это типы-значения (struct).
            for (int i = length - 1; i >= 0; i--)
            {
                TrieNode<TValue> parent = pathNodes[i];
                int childIndex = nodeIndices[i];

                parent._childrenNodes[childIndex] = current;
                current = parent;
            }

            _root = current;
        }

        /// <summary>
        /// Tries to get the value associated with the specified key.
        /// </summary>
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
        {
            key.ThrowIfNull(nameof(key));

            TrieNode<TValue> current = _root;
            int length = key.Length;

            for (int i = 0; i < length; i++)
            {
                int index = current.FindChildIndex(key[i]);
                if (index < 0)
                {
                    value = default;
                    return false; // Префикс не найден, строки точно нет
                }

                current = current._childrenNodes[index];
            }

            if (current._hasValue)
            {
                value = current._value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Determines whether the Trie contains a specific word.
        /// </summary>
        public bool Contains(string key)
        {
            return TryGetValue(key, out _);
        }

        /// <summary>
        /// Checks if there is any word in the Trie that starts with the specified prefix.
        /// </summary>
        public bool StartsWith(string prefix)
        {
            prefix.ThrowIfNull(nameof(prefix));

            TrieNode<TValue> current = _root;
            int length = prefix.Length;

            for (int i = 0; i < length; i++)
            {
                int index = current.FindChildIndex(prefix[i]);
                if (index < 0)
                {
                    return false;
                }

                current = current._childrenNodes[index];
            }

            return true;
        }

        /// <summary>
        /// Clears all elements from the Trie.
        /// </summary>
        public void Clear()
        {
            _root = new TrieNode<TValue>();
            _size = 0;
            _version++;
        }

        /// <summary>
        /// Scans the input text and masks any forbidden words found in the Trie with asterisks (*).
        /// Optimized for high-throughput MMO chat filtering.
        /// </summary>
        /// <param name="text">The raw chat message from a player.</param>
        /// <param name="maskChar">The character used to mask bad words. Default is '*'.</param>
        /// <returns>The filtered text string.</returns>
        public string FilterText(string text, char maskChar = '*')
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Используем StringBuilder для минимизации аллокаций строк в куче
            var output = new Text.StringBuilder(text.Length);
            int textLength = text.Length;
            int i = 0;

            while (i < textLength)
            {
                TrieNode<TValue> current = _root;
                int matchLength = 0;
                int longestMatchLength = 0;

                // Проверяем, начинается ли с текущего индекса i какое-либо запрещенное слово
                for (int j = i; j < textLength; j++)
                {
                    char c = text[j];

                    // Приводим к нижнему регистру для инвариантности фильтра, 
                    // если база слов была записана в нижнем регистре.
                    char lookupChar = char.ToLowerInvariant(c);

                    int childIndex = current.FindChildIndex(lookupChar);
                    if (childIndex < 0)
                    {
                        break; // Дальнейшего совпадения по префиксу нет
                    }

                    matchLength++;
                    current = current._childrenNodes[childIndex];

                    if (current._hasValue)
                    {
                        longestMatchLength = matchLength; // Фиксируем самое длинное совпадение
                    }
                }

                if (longestMatchLength > 0)
                {
                    // Слово найдено! Заменяем его длину на маскирующие символы
                    for (int m = 0; m < longestMatchLength; m++)
                    {
                        output.Append(maskChar);
                    }
                    i += longestMatchLength; // Сдвигаем указатель за пределы плохого слова
                }
                else
                {
                    // Совпадений нет, просто копируем символ игрока (сохраняя оригинальный регистр)
                    output.Append(text[i]);
                    i++;
                }
            }

            return output.ToString();
        }
    }
}