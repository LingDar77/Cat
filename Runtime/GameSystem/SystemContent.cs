namespace Cat
{
    public class SystemContent : SingletonSystemBase<SystemContent>
    {
        public static SystemContent Instance
        {
            get => ISingletonSystem<SystemContent>.GetChecked();
        }

    }
}