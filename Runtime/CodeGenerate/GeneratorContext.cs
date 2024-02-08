namespace Cat.CodeGen
{
    using System.Collections.Generic;
    public sealed class GeneratorContext
    {
        public readonly List<CodeText> CodeList = new();

        public string GerateFolderPath = null;

        public void AddCode(string fileName, string text)
        {
            CodeList.Add(new CodeText() { FileName = fileName, Text = text });
        }

        public void GenerateFolder(string path)
        {
            GerateFolderPath = path;
        }
    }
}