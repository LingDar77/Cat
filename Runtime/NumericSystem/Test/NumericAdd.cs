namespace Cat.NumericSystem
{
    public class NumericAdd : ModifierBase
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
            return numeric is not ModifierBase;
        }
    }
}