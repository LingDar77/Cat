namespace Cat.NumericSystem
{
    using Cat.Utillities;

    public class TestDriver : CatDriver<float, float>
    {
        public override void Drive(float p1, float p2)
        {
            this.LogToScreen("drive");
        }
    }
}