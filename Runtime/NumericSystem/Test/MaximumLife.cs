namespace Cat.NumericSystem
{
    using Cat.Utillities;
    public class MaximumLife : NumericBase
    {
        private void Start()
        {
            OnCurrentValueChanged.AddListener((value, max) => this.LogToScreen("MaximumLife: " + value + " / " + max));
            OnValueRecalculated.AddListener(value =>
            {
                this.LogToScreen("MaximumLife: " + value);
            });
        }
    }
}