namespace Cat
{
    using UnityEditor;
    using UnityEngine;

    static class TerrainHelper
    {
        [MenuItem("CONTEXT/Terrain/Remove Details")]
        static void RemoveDetails(MenuCommand command)
        {
            var context = command.context as Terrain;
            context.terrainData.detailPrototypes = null;
        }
        
        [MenuItem("CONTEXT/Terrain/Remove Trees")]
        static void RemoveTress(MenuCommand command)
        {
            var context = command.context as Terrain;
            context.terrainData.treePrototypes = null;
        }
    }
}
