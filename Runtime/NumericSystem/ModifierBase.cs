namespace Cat.NumericSystem
{
    public abstract class ModifierBase : NumericBase
    {
        public enum ModifierType
        {
            Add,
            Increase,
            Multiply,

            Max,
        }
        
        public abstract ModifierType GetModifierType();

        public virtual bool IsNumericValid(NumericBase numeric)
        {
            return !numeric.GetType().IsSubclassOf(typeof(ModifierBase));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var nums = GetComponentsInParent<NumericBase>();
            foreach (var num in nums)
            {
                if (!IsNumericValid(num)) continue;
                if (!num.Modifiers.ContainsKey(GetModifierType()))
                {
                    num.Modifiers.Add(GetModifierType(), new());
                }
                num.Modifiers[GetModifierType()].Add(this);
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            var nums = GetComponentsInParent<NumericBase>();
            foreach (var num in nums)
            {
                if (!IsNumericValid(num)) continue;
                num.Modifiers[GetModifierType()].Remove(this);
            }
        }

        public override int GetHashCode()
        {
            return (int)CurrentValue + (int)GetModifierType();
        }

    }
}