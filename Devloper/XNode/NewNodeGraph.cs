using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class NewNodeGraph : NodeGraph
{

}
[CreateNodeMenu("Test/Simple")]
public class SimpleNode : Node
{
    [Input] public float value1;
    [Input(dynamicPortList = true)] public float[] value2;

    [Output(dynamicPortList = true)] public float[] result;
}