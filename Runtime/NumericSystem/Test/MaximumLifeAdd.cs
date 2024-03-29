namespace Cat.NumericSystem
{
    public class MaximumLifeAdd : ModifierBase
    {
        public override float GetValue()
        {
            return BaseValue;
        }

        public override ModifierType GetModifierType()
        {
            return ModifierType.Add;
        }
        public override bool IsNumericValid(NumericBase numeric)
        {
            return numeric is MaximumLife;
        }
    }
}