using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class StateMachineEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset VisualTreeAsset = default;
    private IMGUIContainer leftPanel;
    private IMGUIContainer rightPanel;

    [MenuItem("Window/The Unified Implementations/State Machine Editor")]
    public static void ShowStateMachineEditor()
    {
        StateMachineEditor window = GetWindow<StateMachineEditor>();
        window.minSize = new Vector2(600, 400);
        GUIContent content = EditorGUIUtility.IconContent("AnimatorStateMachine Icon");
        content.text = "StateMachineEditor";
        window.titleContent = content;
    }

    public void CreateGUI()
    {
        rootVisualElement.Add(VisualTreeAsset.Instantiate());
        leftPanel = rootVisualElement.Q<IMGUIContainer>("LeftPanel");
        rightPanel = rootVisualElement.Q<IMGUIContainer>("RightPanel");
        
    }

}
