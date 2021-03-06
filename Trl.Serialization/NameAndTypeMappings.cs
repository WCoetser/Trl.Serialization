﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Trl.TermDataRepresentation.Database;

namespace Trl.Serialization
{
    /// <summary>
    /// Custom mappings for term and identifier names to .NET types.
    /// </summary>
    public class NameAndTypeMappings
    {
        private readonly Dictionary<string, Type> _termNameToTypeMappings;
        private readonly Dictionary<Type, string> _typeToTermNameMappings;

        private readonly Dictionary<string, object> _identifierToConstantValue;
        private readonly Dictionary<object, string> _constantValueToIdentifier;

        private readonly Dictionary<Type, MethodInfo> _typeToDeconstructor;

        public const string NULL = "null";

        public NameAndTypeMappings()
        {
            _termNameToTypeMappings = new Dictionary<string, Type>();
            _typeToTermNameMappings = new Dictionary<Type, string>();
            _identifierToConstantValue = new Dictionary<string, object>();
            _constantValueToIdentifier = new Dictionary<object, string>();
            _typeToDeconstructor = new Dictionary<Type, MethodInfo>();
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
        
        /// <summary>
        /// Gets a deconstructor that will used to create the term arguments.
        /// Allows 1 and 0 argument deconstructors (in C# you need at least 2 out parameters)
        /// </summary>
        /// <param name="type">The type being converted into a term.</param>
        /// <returns>The deconstructor method.</returns>
        public MethodInfo GetBestDeconstructorMethodForAcTerm(Type type)
        {
            if (_typeToDeconstructor.TryGetValue(type, out var knownDeconstructor))
            {
                return knownDeconstructor;
            }

            // Get deconstructors
            var deconstructors = type.GetRuntimeMethods()
                .Where(method => method.Name == "Deconstruct"
                    && method.IsPublic
                    && (method.GetParameters().All(p => p.IsOut || method.GetParameters().Length == 0))
                    && method.ReturnType.FullName == "System.Void").ToList();

            if (!deconstructors.Any())
            {
                return null;
            }

            // Get longest deconstructor
            var maxArgCount = deconstructors.Max(method => method.GetParameters().Length);
            var deconstructor = deconstructors.Where(method => method.GetParameters().Length == maxArgCount)
                                            .FirstOrDefault();
            return deconstructor;
        }

        /// <summary>
        /// Iterates through the extension methods in a class, loading deconstructors.
        /// </summary>
        public void MapExtensionMethodDestructorsFromType(Type type)
        {
            if (!type.IsDefined(typeof(ExtensionAttribute)))
            {
                throw new Exception($"Class does not contain extension methods: {type.FullName}");
            }
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {                
                if (method.Name != "Deconstruct"
                    || !method.IsDefined(typeof(ExtensionAttribute)))
                {
                    continue;
                }

                // Should have at least one param based on the Extension attribute
                var parms = method.GetParameters();
                if (parms.Skip(1).All(p => p.IsOut))
                {
                    var maxParamLen = 0;
                    if (_typeToDeconstructor.TryGetValue(parms[0].ParameterType, out var deconstructor)) 
                    {
                        maxParamLen = deconstructor.GetParameters().Length;
                        if (maxParamLen < parms.Length - 1)
                        {
                            _typeToDeconstructor[parms[0].ParameterType] = method;
                        }
                    }
                    else
                    {
                        _typeToDeconstructor.Add(parms[0].ParameterType, method);
                    }
                }
            }
        }

        public T GetConstantValueForIdentifier<T>(string identifierName)
        {
            if (StringComparer.InvariantCulture.Equals(NULL, identifierName))
            {
                return default;
            }

            return _identifierToConstantValue.TryGetValue(identifierName, out var value) switch
            {
                true => (T)value,
                _ => throw new Exception($"Undefined identifier: {identifierName}")
            };
        }

        public string GetIdentifierForConstantValue<T>(T constantValue)
        {
            if (constantValue == null)
            {
                return NULL;
            }

            return _constantValueToIdentifier.TryGetValue(constantValue, out var id) switch {
                true => id,
                _ => null
            };
        }

        public void MapIdentifierNameToConstant<T>(string identifierName, T value)
        {
            if (value == null)
            {
                return;
            }
            else if (_identifierToConstantValue.ContainsKey(identifierName))
            {
                throw new Exception($"Constant value already assigned for {identifierName}");
            }

            _identifierToConstantValue.Add(identifierName, value);
            _constantValueToIdentifier.Add(value, identifierName);
        }

        public bool IsNumeric(object inputObject)
            => inputObject is sbyte
            || inputObject is byte
            || inputObject is short
            || inputObject is ushort
            || inputObject is int
            || inputObject is uint
            || inputObject is long
            || inputObject is ulong
            || inputObject is BigInteger
            || inputObject is decimal
            || inputObject is float
            || inputObject is double;
    }
}
