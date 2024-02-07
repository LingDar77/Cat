namespace Cat
{
    public class Cat : SingletonSystemBase<Cat>
    {
        public static Cat Instance
        {
            get => ISingletonSystem<Cat>.GetChecked();
        }

    }
}