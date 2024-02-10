namespace Cat.NumericSystem
{
    public class MaximumLifeInc : ModifierBase
    {
        public override float GetValue()
        {
            return BaseValue;
        }

        public override ModifierType GetModifierType()
        {
            return ModifierType.Increase;
        }
        public override bool IsNumericValid(NumericBase numeric)
        {
            return numeric is MaximumLife;
        }
    }
}