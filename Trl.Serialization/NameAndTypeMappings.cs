using System;
using System.Collections.Generic;

namespace Trl.Serialization
{
    /// <summary>
    /// Custom mappings for term and identifier names to .NET types.
    /// </summary>
    public class NameAndTypeMappings
    {
        private readonly Dictionary<string, Type> _termNameToTypeMappings;
        private readonly Dictionary<Type, string> _typeToTermNameMappings;

        public NameAndTypeMappings()
        {
            _termNameToTypeMappings = new Dictionary<string, Type>();
            _typeToTermNameMappings = new Dictionary<Type, string>();
        }

        public void MapTermNameToType<TTargetType>(string termName)
        {
            if (_termNameToTypeMappings.ContainsKey(termName))
            {
                throw new Exception($"Unable to map term name {termName} more than once.");
            }

            _termNameToTypeMappings.Add(termName, typeof(TTargetType));
            _typeToTermNameMappings.Add(typeof(TTargetType), termName);
        }

        public void MapIdentifierNameToConstant<T>(string identifierName, T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type to convert the term to. 
        /// This can be used with inheritance heirarchies where the target type may not be instantiable
        /// and a subtype may be needed.
        /// </summary>
        /// <param name="termName">Name of the term being converted.</param>
        /// <param name="currentTargetType">The type the term's values will be assigned to.</param>
        /// <returns>The preferred type to create for the given term.</returns>
        public Type GetTypeForTermName(string termName, Type currentTargetType)
        {
            var returnType = _termNameToTypeMappings.TryGetValue(termName, out var retType) switch
            {
                true => retType,
                _ => currentTargetType
            };
            if (!currentTargetType.IsAssignableFrom(returnType))
            {
                throw new Exception($"Unable to assign value of type {returnType.FullName} to type {currentTargetType.FullName} for term name {termName}");
            }
            return returnType;
        }

        /// <summary>
        /// Gets the term name for the term that will represent the given type.
        /// This can be used with inheritance heirarchies.
        /// </summary>
        /// <param name="type">The type for which a name is needed.</param>
        /// <returns>The name of the term that will be created.</returns>
        public string GetTermNameForType(Type type)
        {
            return _typeToTermNameMappings.TryGetValue(type, out var retType) switch
            {
                true => retType,
                _ => type.Name
            };
        }

        public T GetConstantValueForIdentifier<T>(string identifierName)
        {
            throw new NotImplementedException();
        }

        public string GetIdentifierForConstantValue<T>(T constantValue)
        {
            throw new NotImplementedException();
        }
    }
}
