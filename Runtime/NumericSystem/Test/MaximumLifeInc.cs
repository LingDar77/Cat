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
    }
}