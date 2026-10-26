using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Helpers to configure components that live in assemblies the editor tools do not reference.
    /// </summary>
    public static class EditorReflection
    {
        /// <summary>
        /// Finds a type by its full name in any loaded assembly.
        /// </summary>
        /// <param name="fullName">Namespace-qualified type name.</param>
        /// <returns>The type.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no loaded assembly has the type.</exception>
        public static Type FindType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName))
                .FirstOrDefault(found => found != null);

            return type ?? throw new InvalidOperationException($"Type '{fullName}' was not found.");
        }

        /// <summary>
        /// Edits serialized fields of a component without a compile-time reference to its type.
        /// </summary>
        /// <param name="component">The component to edit.</param>
        /// <param name="change">The edit to apply to its serialized form.</param>
        public static void Set(Component component, Action<SerializedObject> change)
        {
            var serialized = new SerializedObject(component);
            change(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Finds the first component on a game object, or its children, whose type has the given simple name.
        /// </summary>
        /// <param name="root">The object to search.</param>
        /// <param name="typeName">Simple type name such as <c>CharacterStatsHolder</c>.</param>
        /// <returns>The component, or null when none matches.</returns>
        public static Component FindByTypeName(GameObject root, string typeName)
        {
            return root.GetComponentsInChildren<Component>(true)
                .FirstOrDefault(component => component != null && component.GetType().Name == typeName);
        }
    }
}
