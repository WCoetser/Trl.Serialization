using System.IO;
using System.Text;
using Trl.Serialization.Translator;
using Trl.TermDataRepresentation.Parser.AST;

namespace Trl.Serialization
{
    public class TextStreamSerializer
    {
        private readonly ObjectToAstTranslator _objectToAstTranslator;
        private readonly StringToObjectTranslator _stringToObjectTranslator;

        public TextStreamSerializer(NameAndTypeMappings customMappings)
        {
            _objectToAstTranslator = new ObjectToAstTranslator(customMappings);
            _stringToObjectTranslator = new StringToObjectTranslator(customMappings);
        }

        public TObject Deserialize<TObject>(StreamReader input, string rootLabel = "root", int maxRewriteIterations = 100000)
        {
            var inputStr = input.ReadToEnd();
            return _stringToObjectTranslator.BuildObject<TObject>(inputStr, rootLabel, maxRewriteIterations);
        }

        public void Serialize<TObject>(TObject inputObject, StreamWriter outputStream, string rootLabel = "root", bool prettyPrint = false)
        {
            StatementList output = _objectToAstTranslator.BuildAst(inputObject, rootLabel);
            output.WriteToStream(outputStream, prettyPrint);
        }
    }
}
