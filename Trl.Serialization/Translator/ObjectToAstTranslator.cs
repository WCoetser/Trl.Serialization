using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Trl.TermDataRepresentation.Database;
using Trl.TermDataRepresentation.Database.Mutations;
using Trl.TermDataRepresentation.Parser.AST;

namespace Trl.Serialization.Translator
{
    /// <summary>
    /// Converts an object into an AST, which can be converted into a string.
    /// </summary>
    internal class ObjectToAstTranslator
    {
        public const BindingFlags Bindings = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
        private readonly NameAndTypeMappings _nameAndTypeMappings;

        public ObjectToAstTranslator(NameAndTypeMappings nameAndTypeMappings)
        {
            _nameAndTypeMappings = nameAndTypeMappings;
        }

        internal StatementList BuildAst<TObject>(TObject inputObject, string rootLabel)
        {
            var termDatabase = new TermDatabase();
            Symbol root = BuildAstForObject(inputObject, termDatabase);
            termDatabase.Writer.LabelTerm(root.TermIdentifier.Value, rootLabel);
            termDatabase.Writer.SetAsRootTerm(root.TermIdentifier.Value);
            termDatabase.MutateDatabase(new ConvertCommonTermsToRewriteRules());
            return termDatabase.Reader.ReadCurrentFrame();
        }

        /// <summary>
        /// Loads the object into the database, returns the ID.
        /// </summary>
        /// <param name="inputObject">Object to load</param>
        /// <param name="termDatabase">Database to load it into.</param>
        /// <returns>Unique integer mapped ID of term.</returns>
        private Symbol BuildAstForObject(object inputObject, TermDatabase termDatabase)
        {
            var knownConstant = _nameAndTypeMappings.GetIdentifierForConstantValue(inputObject);
            if (!string.IsNullOrWhiteSpace(knownConstant))
            {
                return termDatabase.Writer.StoreAtom(knownConstant, SymbolType.Identifier);
            }
            else if (inputObject is string)
            {
                return termDatabase.Writer.StoreAtom(Convert.ToString(inputObject), SymbolType.String);
            }
            // NB: IEnumerable must be after string because string is IEnumerable
            else if (inputObject is IEnumerable)
            {
                var listMembers = new List<Symbol>();
                var inputEnumerable = (IEnumerable)inputObject;
                foreach (var item in inputEnumerable)
                {
                    listMembers.Add(BuildAstForObject(item, termDatabase));
                }
                return termDatabase.Writer.StoreTermList(listMembers.ToArray());
            }
            else if (IsNumeric(inputObject))
            {
                return termDatabase.Writer.StoreAtom(Convert.ToString(inputObject), SymbolType.Number);
            }
            else
            {
                return GenerateNonAcTerm(inputObject, termDatabase);
            }
        }

        private Symbol GenerateNonAcTerm(object inputObject, TermDatabase termDatabase)
        {
            // Assume we are creating a non ac term in the default case
            var type = inputObject.GetType();
            var properties = type.GetProperties(Bindings)
                                .Where(p => p.CanRead).OrderBy(p => p.Name);
            var fields = type.GetFields(Bindings)
                                .OrderBy(p => p.Name);

            // Build arguments
            var fieldMappingIdentifiers = new List<Symbol>();
            var arguments = new List<Symbol>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(inputObject);
                if (value != null)
                {
                    fieldMappingIdentifiers.Add(termDatabase.Writer.StoreAtom(prop.Name, SymbolType.Identifier));
                    arguments.Add(BuildAstForObject(value, termDatabase));
                }
            }
            foreach (var field in fields)
            {
                var value = field.GetValue(inputObject);
                if (value != null)
                {
                    fieldMappingIdentifiers.Add(termDatabase.Writer.StoreAtom(field.Name, SymbolType.Identifier));
                    arguments.Add(BuildAstForObject(value, termDatabase));
                }
            }

            Dictionary<TermMetaData, Symbol> metadata = new Dictionary<TermMetaData, Symbol>();            
            var fieldList = termDatabase.Writer.StoreTermList(fieldMappingIdentifiers.ToArray());
            metadata.Add(TermMetaData.ClassMemberMappings, fieldList);

            return termDatabase.Writer.StoreNonAcTerm(_nameAndTypeMappings.GetTermNameForType(type), arguments.ToArray(), metadata);
        }

        private static bool IsNumeric(object inputObject)
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
