namespace Cat.NumericSystem
{
    public class NumericInc : ModifierBase
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
            return numeric is not ModifierBase;
        }
    }
}