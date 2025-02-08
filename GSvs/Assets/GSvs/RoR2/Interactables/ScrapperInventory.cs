using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace GSvs.RoR2.Interactables
{
    public class ScrapperInventory : MonoBehaviour
    {
        public readonly Dictionary<ItemIndex, int> collectedItemCounts = new Dictionary<ItemIndex, int>();

        public void OnItemAdded(ItemIndex itemIndex, int count)
        {
            collectedItemCounts[itemIndex] = collectedItemCounts.GetValueOrDefault(itemIndex) + count;
        }

        private void Start()
        {
            SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
        }

        private void OnDestroy()
        {
            SceneDirector.onPrePopulateSceneServer -= OnPrePopulateSceneServer;
        }

        private void OnPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            collectedItemCounts.Clear();
        }
    }
}
