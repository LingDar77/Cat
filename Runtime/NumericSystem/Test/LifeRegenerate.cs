namespace Cat.NumericSystem
{
    using UnityEngine;

    public class LifeRegenerate : NumericBase
    {
        protected MaximumLife life;
        protected override void OnEnable()
        {
            base.OnEnable();
            life = GetComponentInParent<MaximumLife>();
        }
        protected override void Update()
        {
            if (life.CurrentValue <= 0) return;
            life.CurrentValue += CurrentValue * Time.deltaTime;
            base.Update();
        }
    }
}