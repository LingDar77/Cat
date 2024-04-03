namespace Cat.NumericSystem
{
    using UnityEngine;
    public class NumericIncRandom : ModifierBase
    {
        protected override void Update()
        {
            CurrentValue = BaseValue * Mathf.Sin(Time.time);
            base.Update();
        }
        public override ModifierType GetModifierType()
        {
            return ModifierType.Increase;
        }
        public override bool IsNumericValid(NumericBase numeric)
        {
            return numeric is not ModifierBase;
        }
    }
}