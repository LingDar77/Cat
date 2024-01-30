namespace TUI.Intergration.Localization
{
    using System;
    using TMPro;
    using TUI.Utillities;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Components;

    [System.Serializable]
    public class LocalizedFont : LocalizedAsset<TMP_FontAsset> { }

    public class LocalizedText : LocalizedMonoBehaviour
    {
        public TextMeshProUGUI text;
        public LocalizedString StringReference = new();
        public LocalizedFont FontReference = new();

        private void OnValidate()
        {
            this.EnsureComponent(ref text);
            RefreshString();
        }

        private void OnEnable()
        {
            if (StringReference != null)
                StringReference.StringChanged += OnStringChanged;
            if (FontReference != null)
                FontReference.AssetChanged += OnFontChanged;
        }

        private void OnDisable()
        {
            if (StringReference != null)
                StringReference.StringChanged -= OnStringChanged;
            if (FontReference != null)
                FontReference.AssetChanged -= OnFontChanged;
        }

        private void OnStringChanged(string value)
        {
            text.text = value;
        }
        private void OnFontChanged(TMP_FontAsset value)
        {
            text.font = value;
        }
        public void RefreshString()
        {
            StringReference?.RefreshString();
        }
    }
}