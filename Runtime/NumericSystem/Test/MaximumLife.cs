namespace Cat.NumericSystem
{
    using Cat.Utillities;
    public class MaximumLife : NumericBase
    {
        private void Start()
        {
            OnCurrentValueChanged.AddListener((value, max) =>
            {
                this.LogToScreen($"Life changed: {value} / {max}");
            });
        }
    }
}