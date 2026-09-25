using UnityEngine;

namespace HatHop
{
    // Full scene paths avoid ambiguous scene names. The custom Inspector uses SceneAsset pickers.
    [CreateAssetMenu(menuName = "Hat Hop/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        public string prototypeScenePath = "Assets/HatHop/Scenes/GameplayTest.unity";
        public string easyScenePath = "";
        public string mediumScenePath = "";
        public string hardScenePath = "";

        public string PlayScenePath => string.IsNullOrEmpty(easyScenePath)
            ? prototypeScenePath : easyScenePath;
    }
}
