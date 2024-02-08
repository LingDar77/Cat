namespace Cat.CodeGen
{
    using System.Collections.Generic;
    public sealed class GeneratorContext
    {
        private List<CodeText> _codeList = new List<CodeText>();
        internal IReadOnlyList<CodeText> codeList => _codeList;

        private string _overrideFolderPath = null;
        internal string overrideFolderPath => _overrideFolderPath;

        public void AddCode(string fileName, string text)
        {
            _codeList.Add(new CodeText() { fileName = fileName, text = text });
        }

        public void OverrideFolderPath(string path)
        {
            _overrideFolderPath = path;
        }
    }
}