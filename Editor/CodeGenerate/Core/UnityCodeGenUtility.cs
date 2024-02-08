namespace Cat.CodeGen
{
    public static class CodeGenUtility
    {
        public const string defaultFolderPath = "Assets/Cats.Generated";

        public static void Generate()
        {
            ScriptGenerator.Generate();
        }
    }
}