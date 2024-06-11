namespace Cat.NumericSystem
{
    using System.Collections.Generic;
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.Events;

    public enum ModifierType
    {
        Add,
        Increase,
        Multiply,

        Max,
    }

    public abstract class NumericBase : MonoBehaviour
    {
        [SerializeField] protected float BaseValue;
        [SerializeField] protected float currentValue;
        public float CurrentValue
        {
            get => GetCurrentValue();
            set => SetCurrentValue(value);
        }
        public float MaxValue => cacheValue;
        public readonly Dictionary<ModifierType, List<ModifierBase>> Modifiers = new();

        public CatDriver<float> OnValueRecalculated;
        public CatDriver<float, float> OnCurrentValueChanged;
        public UnityEvent OnValueChanged;

        protected bool IsDirty = true;
        [ReadOnlyInEditor]
        [SerializeField] private float cacheValue;
        private int cacheHash;

        protected virtual void Update()
        {
            UpdateHash();
            NotifyUpdate();
        }

        public virtual float GetCurrentValue()
        {
            return currentValue;
        }

        public virtual void SetCurrentValue(float value)
        {
            currentValue = Mathf.Clamp(value, 0, MaxValue);
        }

        public void NotifyUpdate()
        {
            OnValueChanged.Invoke();
            if (OnCurrentValueChanged == null) return;
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
        public override int GetHashCode()
        {
            int hash = enabled ? 1 : 0;
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
            var param = new float[(int)ModifierType.Max] { 0, 0, 0 };
            for (int i = 0; i != (int)ModifierType.Max; ++i)
            {
                var key = (ModifierType)i;
                if (!Modifiers.ContainsKey(key)) continue;
                foreach (var modifier in Modifiers[key])
                {
                    param[i] += modifier.CurrentValue;
                }
            }
            cacheValue = (cacheValue + param[0]) * ((param[1] * .01f) + 1) * ((param[2] * .01f) + 1);

            IsDirty = false;

            if (OnValueRecalculated != null)
            {
                OnValueRecalculated.Drive(cacheValue);
            }

            SetCurrentValue(cacheValue);
        }
    }


}