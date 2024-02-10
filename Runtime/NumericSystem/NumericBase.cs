namespace Cat.NumericSystem
{
    using System.Collections.Generic;
    using Cat.Utillities;
    using UnityEngine;

    public abstract class NumericBase : MonoBehaviour
    {
        [SerializeField] protected float BaseValue;
        [SerializeField] protected float currentValue;
        public float CurrentValue { get => currentValue; set => SetCurrentValue(value); }
        protected float MaxValue => cacheValue;
        public readonly Dictionary<ModifierBase.ModifierType, HashSet<ModifierBase>> Modifiers = new();

        public CatDriver<float> OnValueRecalculated;
        public CatDriver<float, float> OnCurrentValueChanged;

        protected bool IsDirty = true;
        private float cacheValue;
        private int cacheHash;

        protected virtual void Update()
        {
            UpdateHash();
        }

        public virtual void SetCurrentValue(float value)
        {
            currentValue = Mathf.Clamp(value, 0, MaxValue);

            if (OnCurrentValueChanged == null || currentValue == 0) return;
            OnCurrentValueChanged.Drive(CurrentValue, MaxValue);
        }

        protected virtual void OnEnable()
        {
            currentValue = GetValue();
        }

        protected virtual void OnDisable()
        {
        }

        public virtual float GetValue()
        {
            if (!IsDirty) return cacheValue;

            UpdateValue();

            return cacheValue;
        }

        protected virtual void ApplyModifier(ModifierBase.ModifierType key, ModifierBase modifier)
        {
            switch (key)
            {
                case ModifierBase.ModifierType.Add:
                    cacheValue += modifier.GetValue();
                    break;
                case ModifierBase.ModifierType.Increase:
                case ModifierBase.ModifierType.Multiply:
                    cacheValue *= (modifier.CurrentValue / 100) + 1;
                    break;
                default:
                    throw new System.Exception("Unknown modifier type!");
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var mod in Modifiers)
            {
                foreach (var modifier in mod.Value)
                {
                    hash += modifier.GetHashCode();
                }
            }
            return hash;
        }

        protected void UpdateHash()
        {
            var currentHash = GetHashCode();
            if (cacheHash == currentHash)
                return;
            cacheHash = currentHash;
            IsDirty = true;
            UpdateValue();

        }

        protected void UpdateValue()
        {
            if (!IsDirty) return;
            cacheValue = BaseValue;
            for (int i = 0; i != (int)ModifierBase.ModifierType.Max; ++i)
            {
                var key = (ModifierBase.ModifierType)i;
                if (!Modifiers.ContainsKey(key)) continue;
                foreach (var modifier in Modifiers[key])
                {
                    ApplyModifier(key, modifier);
                }
            }

            IsDirty = false;

            if (OnValueRecalculated != null)
                OnValueRecalculated.Drive(cacheValue);

            if (currentValue > cacheValue)
                SetCurrentValue(cacheValue);
        }
    }


}