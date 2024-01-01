using UnityEngine.UIElements;


namespace TUI.EditorScript.UIToolkitExtensions
{
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
