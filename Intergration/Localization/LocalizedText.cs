namespace Cat.Intergration.Localization
{
    using TMPro;
    using Cat.Utillities;
    using UnityEngine.Localization;
    using UnityEngine.Localization.Components;
    using UnityEditor;

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
            if (text == null) return;
            text.text = value;
        }
        private void OnFontChanged(TMP_FontAsset value)
        {
            if (text == null) return;
            text.font = value;
        }
        public void RefreshString()
        {
            if (StringReference == null) return;
            StringReference.RefreshString();
        }
#if UNITY_EDITOR

        [MenuItem("CONTEXT/TextMeshProUGUI/Localize")]
        private static void LocalizedTextMeshPro(MenuCommand command)
        {
            var context = command.context as TextMeshProUGUI;
            context.gameObject.AddComponent<LocalizedText>();
        }
#endif
    }
}