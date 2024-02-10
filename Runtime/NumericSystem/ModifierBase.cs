using Cat.Utillities;

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

        public abstract bool IsNumericValid(NumericBase numeric);

        public override void SetCurrentValue(float value)
        {
            currentValue = value;
            
            if (OnCurrentValueChanged == null) return;
            OnCurrentValueChanged.Drive(CurrentValue, MaxValue);
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
            return base.GetHashCode() + (int)CurrentValue + (int)GetModifierType();
        }

    }
}