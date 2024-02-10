namespace Cat.NumericSystem
{
    using Cat.Utillities;
    public class MaximumLife : NumericBase
    {
        private void Start()
        {
            OnCurrentValueChanged.AddListener((value, max) =>
            {
                if (value > 0)
                    this.LogToScreen("MaximumLife: " + value + " / " + max);
                else
                {
                    this.LogToScreen("Dead");
                }
            });
        }
    }
}