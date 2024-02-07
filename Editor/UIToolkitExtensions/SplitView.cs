#if UNITY_EDITOR
namespace Cat.EditorScript.UIToolkitExtensions
{
    using UnityEngine.UIElements;
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits>
        { }

        public SplitView()
        {
            orientation = TwoPaneSplitViewOrientation.Horizontal;
        }
    }
}
#endif