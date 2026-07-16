using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Crystal.Common.Editor
{
    public static class SOUtils
    {
        public static void RegisterUndo(SerializedProperty property, string label)
        {
            if (property == null)
            {
                return;
            }
            Object[] targets = property.serializedObject.targetObjects;
            if (targets.Length == 1)
            {
                Undo.RecordObject(targets[0], label);
            }
            else
            {
                Undo.RecordObjects(targets, label);
            }
        }

        public static void RegisterUndo(Object[] unityObjects, string label)
        {
            if (unityObjects == null || unityObjects.Length == 0) return;
            Undo.RecordObjects(unityObjects, label);
        }

        public static bool TraverseSO(Object unityObject, Func<SerializedProperty, bool> isCompleteFunc)
        {
            if (unityObject == null || isCompleteFunc == null) return false;
            using SerializedObject so = new SerializedObject(unityObject);
            using SerializedProperty iterator = so.GetIterator();
            if (!iterator.NextVisible(true)) return false;
            return TraversePropertyImpl(iterator, isCompleteFunc);
        }

        private static bool TraversePropertyImpl(SerializedProperty property, Func<SerializedProperty, bool> isCompleteFunc)
        {
            using SerializedProperty iterator = property.Copy();
            using SerializedProperty endProperty = property.GetEndProperty();
            while (iterator.Next(true) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                if (iterator.propertyType == SerializedPropertyType.ManagedReference && !iterator.isArray)
                {
                    if (isCompleteFunc.Invoke(iterator))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}