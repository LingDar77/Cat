namespace Cat.Utillities
{
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEditor;
    using Type = System.Type;
    using System.Linq;
    using Cat.Library;

    public sealed class ImplementedInterfaceAttribute : PropertyAttribute
    {

        public System.Type InterfaceType { get; }

        public ImplementedInterfaceAttribute(System.Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ImplementedInterfaceAttribute))]
    class RequireInterfaceDrawer : PropertyDrawer
    {
        private static class Contents
        {
            public const float objectFieldMiniThumbnailHeight = 18f;
            public const float objectFieldMiniThumbnailWidth = 32f;
            public const float mismatchImplementationMessageHeight = 20f;

            public static GUIContent invalidTypeMessage = EditorGUIUtility.TrTextContent($"Use {nameof(ImplementedInterfaceAttribute)} with Object reference fields.");
            public static GUIContent invalidAttributeMessage = EditorGUIUtility.TrTextContent($"The attribute is not a {nameof(ImplementedInterfaceAttribute)}.");
            public static GUIContent invalidInterfaceMessage = EditorGUIUtility.TrTextContent("The required type is not an interface.");
            public static GUIContent mismatchImplementationMessage = EditorGUIUtility.TrTextContent("The referenced object does not implement {0}.");
        }

        private static readonly Dictionary<Type, Dictionary<Type, string>> filterMapByFieldType = new();
        private static readonly List<Type> minimumAssignableImplementations = new();

        #region Object Field

        private enum ObjectFieldVisualType
        {
            IconAndText,
            LargePreview,
            MiniPreview,
        }

        private static Rect GetObjectFieldButtonRect(Type objectType, Rect position)
        {
            var hasThumbnail = EditorGUIUtility.HasObjectThumbnail(objectType);
            var visualType = ObjectFieldVisualType.IconAndText;

            if (hasThumbnail && position.height <= Contents.objectFieldMiniThumbnailHeight && position.width <= Contents.objectFieldMiniThumbnailWidth)
                visualType = ObjectFieldVisualType.MiniPreview;
            else if (hasThumbnail && position.height > EditorGUIUtility.singleLineHeight)
                visualType = ObjectFieldVisualType.LargePreview;

            return visualType switch
            {
                ObjectFieldVisualType.IconAndText => new Rect(position.xMax - 19, position.y, 19, position.height),
                ObjectFieldVisualType.MiniPreview => new Rect(position.xMax - 14, position.y, 14, position.height),
                ObjectFieldVisualType.LargePreview => new Rect(position.xMax - 36, position.yMax - 14, 36, 14),
                _ => throw new System.ArgumentOutOfRangeException(),
            };
        }

        private static Type GetObjectFieldType(Rect position, Type fieldType, Type interfaceType, out bool? dragAndDropAssignable)
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

        #endregion

        #region Search Filter

        private static bool IsDirectImplementation(Type type, Type interfaceType)
        {
            var directImplementedInterfaces = type.BaseType == null ? type.GetInterfaces() : type.GetInterfaces().Except(type.BaseType.GetInterfaces());
            return directImplementedInterfaces.Contains(interfaceType);
        }

        private static void GetDirectImplementations(Type fieldType, Type interfaceType, List<Type> resultList)
        {
            if (!interfaceType.IsInterface)
                return;

        }

        private static string GetSearchFilter(Type fieldType, Type interfaceType)
        {
            if (!filterMapByFieldType.TryGetValue(fieldType, out var filterByInterfaceType))
            {
                filterByInterfaceType = new Dictionary<Type, string>();
                filterMapByFieldType.Add(fieldType, filterByInterfaceType);
            }
            else if (filterByInterfaceType.TryGetValue(interfaceType, out var cachedSearchFilter))
            {
                return cachedSearchFilter;
            }

            minimumAssignableImplementations.Clear();
            GetDirectImplementations(fieldType, interfaceType, minimumAssignableImplementations);

            using var block = zstring.Block();
            zstring searchFilter = "";
            foreach (var type in minimumAssignableImplementations)
            {
                searchFilter = zstring.Concat(searchFilter, "t:");
                searchFilter = zstring.Concat(searchFilter, type.Name);
                searchFilter = zstring.Concat(searchFilter, " ");
            }

            filterByInterfaceType.Add(interfaceType, searchFilter);
            return searchFilter;
        }

        #endregion

        #region Helper Methods

        private static bool TryGetAssignableObject(Object objectToValidate, Type fieldType, Type interfaceType, out Object assignableObject)
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

        private static Type GetFieldOrElementType(Type fieldType)
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

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = base.GetPropertyHeight(property, label);
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null &&
                attribute is ImplementedInterfaceAttribute requireInterfaceAttr &&
                requireInterfaceAttr.InterfaceType.IsInterface &&
                !requireInterfaceAttr.InterfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                propertyHeight += Contents.mismatchImplementationMessageHeight + 4f;
            }

            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var objectPickerID = GUIUtility.GetControlID(FocusType.Passive);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label, Contents.invalidTypeMessage);
                return;
            }

            if (!(attribute is ImplementedInterfaceAttribute requireInterfaceAttr))
            {
                EditorGUI.LabelField(position, label, Contents.invalidAttributeMessage);
                return;
            }

            if (requireInterfaceAttr.InterfaceType == null || !requireInterfaceAttr.InterfaceType.IsInterface)
            {
                EditorGUI.LabelField(position, label, Contents.invalidInterfaceMessage);
                return;
            }

            if (property.objectReferenceValue != null && !requireInterfaceAttr.InterfaceType.IsInstanceOfType(property.objectReferenceValue))
            {
                var messagePosition = position;
                position.height -= Contents.mismatchImplementationMessageHeight + 4f;
                messagePosition.y = position.yMax + 2f;
                messagePosition.height = Contents.mismatchImplementationMessageHeight;
                EditorGUI.HelpBox(messagePosition, string.Format(Contents.mismatchImplementationMessage.text, requireInterfaceAttr.InterfaceType.Name), MessageType.Warning);
            }

            var fieldType = GetFieldOrElementType(fieldInfo.FieldType);

            using (var scope = new EditorGUI.PropertyScope(position, label, property))
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var allowSceneObjs = !EditorUtility.IsPersistent(property.serializedObject.targetObject);
                var objectFieldType = GetObjectFieldType(position, fieldType, requireInterfaceAttr.InterfaceType, out var dragAndDropAssignable);

                // Override the Object Field button to call the Object Selector window with a filter containing the minimum set of assignable field types that implement the required interface
                if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && position.Contains(Event.current.mousePosition))
                {
                    var buttonRect = GetObjectFieldButtonRect(objectFieldType, position);
                    if (buttonRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.editingTextField = false;

                        var searchFilter = GetSearchFilter(fieldType, requireInterfaceAttr.InterfaceType);
                        EditorGUIUtility.ShowObjectPicker<Object>(property.objectReferenceValue, allowSceneObjs, searchFilter, objectPickerID);

                        Event.current.Use();
                        GUIUtility.ExitGUI();
                    }
                }

                if (dragAndDropAssignable.HasValue && !dragAndDropAssignable.Value)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    Event.current.Use();
                }

                var value = EditorGUI.ObjectField(position, scope.content, property.objectReferenceValue, objectFieldType, allowSceneObjs);

                // Get the value of the selected Object in the Object Selector window
                if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == objectPickerID)
                {
                    GUI.changed = true;
                    value = EditorGUIUtility.GetObjectPickerObject();
                }

                if (check.changed && TryGetAssignableObject(value, fieldType, requireInterfaceAttr.InterfaceType, out var assignableValue))
                    property.objectReferenceValue = assignableValue;
            }
        }

    }
#endif
}