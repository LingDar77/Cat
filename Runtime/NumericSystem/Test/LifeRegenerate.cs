namespace Cat.NumericSystem
{
    using UnityEngine;

    public class LifeRegenerate : NumericBase
    {
        protected Life life;
        protected override void OnEnable()
        {
            base.OnEnable();
            life = GetComponentInParent<Life>();
        }
        protected override void Update()
        {
            if (life.CurrentValue <= 0) return;
            life.CurrentValue += CurrentValue * Time.deltaTime;
            base.Update();
        }
    }
}