using System;
using UnityEngine;

namespace Src.Configs
{
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
    public class DungeonGeneratorConfig : ScriptableObject
    {
        public string configName;
        
        public Vector2Int size;
        public int roomCount;
        public Vector2Int roomMaxSize;
        public GameObject prefab;
        public Material roomMaterial;
        public Material pathMaterial;
        
        /// Chance to add edge to to tree
        public double luckyNumber = 0.125;
    }
}