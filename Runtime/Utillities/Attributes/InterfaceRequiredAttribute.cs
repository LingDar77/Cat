using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace SFC
{

    public sealed class InterfaceRequiredAttribute : PropertyAttribute
    {

        public System.Type InterfaceType { get; }

        public InterfaceRequiredAttribute(System.Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InterfaceRequiredAttribute))]
    class RequireInterfaceDrawer : PropertyDrawer
    {

        static System.Type GetObjectFieldType(Rect position, System.Type fieldType, System.Type interfaceType, out bool? dragAndDropAssignable)
        {
            dragAndDropAssignable = null;

            // Used to correctly display the interface type name
            if (Event.current.type == EventType.Repaint)
                return interfaceType;

            // Used to correctly update the DragAndDrop.visualMode when dragging references that are not assignable
            if (GUI.enabled &&
                (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) &&
                DragAndDrop.objectReferences.Length > 0 &&
                position.Contains(Event.current.mousePosition))
            {
                var referencedValue = DragAndDrop.objectReferences[0];
                if (referencedValue != null)
                {
                    dragAndDropAssignable = TryGetAssignableObject(referencedValue, fieldType, interfaceType, out _);
                    if (!dragAndDropAssignable.Value)
                        return interfaceType;
                }
            }

            return fieldType;
        }

        static bool TryGetAssignableObject(Object objectToValidate, System.Type fieldType, System.Type interfaceType, out Object assignableObject)
        {
            if (objectToValidate == null)
            {
                assignableObject = null;
                return true;
            }

            var valueType = objectToValidate.GetType();
            if (fieldType.IsAssignableFrom(valueType) && interfaceType.IsAssignableFrom(valueType))
            {
                assignableObject = objectToValidate;
                return true;
            }

            // If the given objectToValidate is a GameObject, search its components as well
            if (objectToValidate is GameObject gameObject)
            {
                assignableObject = gameObject.GetComponent(interfaceType);
                if (assignableObject != null && fieldType.IsInstanceOfType(assignableObject) && interfaceType.IsInstanceOfType(assignableObject))
                    return true;
            }

            assignableObject = null;
            return false;
        }

        static System.Type GetFieldOrElementType(System.Type fieldType)
        {
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var types = fieldType.GetGenericArguments();
                return types.Length <= 0 ? null : types[0];
            }

            if (fieldType.IsArray)
                return fieldType.GetElementType();

            return fieldType;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = base.GetPropertyHeight(property, label);
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null &&
                attribute is InterfaceRequiredAttribute requireInterfaceAttr &&
                requireInterfaceAttr.InterfaceType.IsInterface &&
                !requireInterfaceAttr.InterfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                propertyHeight += 20f + 4f;
            }

            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var objectPickerID = GUIUtility.GetControlID(FocusType.Passive);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return;
            }

            if (attribute is not InterfaceRequiredAttribute requireInterfaceAttr)
            {
                return;
            }

            if (requireInterfaceAttr.InterfaceType == null || !requireInterfaceAttr.InterfaceType.IsInterface)
            {
                return;
            }

            if (property.objectReferenceValue != null && !requireInterfaceAttr.InterfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                var messagePosition = position;
                position.height -= 20f + 4f;
                messagePosition.y = position.yMax + 2f;
                messagePosition.height = 20f;
            }

            var fieldType = GetFieldOrElementType(fieldInfo.FieldType);

            using var scope = new EditorGUI.PropertyScope(position, label, property);
            using var check = new EditorGUI.ChangeCheckScope();
            var allowSceneObjs = !EditorUtility.IsPersistent(property.serializedObject.targetObject);
            var objectFieldType = GetObjectFieldType(position, fieldType, requireInterfaceAttr.InterfaceType, out var dragAndDropAssignable);

            if (dragAndDropAssignable.HasValue && !dragAndDropAssignable.Value)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                Event.current.Use();
            }

            var value = EditorGUI.ObjectField(position, scope.content, property.objectReferenceValue, objectFieldType, allowSceneObjs);

            // Get the value of the selected Object in the Object Selector window
            if (EditorGUIUtility.GetObjectPickerControlID() == objectPickerID)
            {
                GUI.changed = true;
                value = EditorGUIUtility.GetObjectPickerObject();
            }

            if (check.changed && TryGetAssignableObject(value, fieldType, requireInterfaceAttr.InterfaceType, out var assignableValue))
                property.objectReferenceValue = assignableValue;
        }
    }

#endif
}