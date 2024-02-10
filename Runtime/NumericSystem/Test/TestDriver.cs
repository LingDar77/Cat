namespace Cat.NumericSystem
{
    using Cat.Utillities;

    public class TestDriver : CatDriver<float, float>
    {
        public override void Drive(float p1, float p2)
        {
            this.LogFormatToScreen("Current Value: {0}, Max Value: {1}", UnityEngine.LogType.Log, p1, p2);
        }
    }
}