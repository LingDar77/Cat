namespace Cat
{
    public class CatContent : SingletonSystemBase<CatContent>
    {
        public static CatContent Instance
        {
            get => ISingletonSystem<CatContent>.GetChecked();
        }

    }
}