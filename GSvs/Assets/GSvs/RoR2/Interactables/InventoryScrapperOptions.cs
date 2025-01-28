using RoR2;
using UnityEngine;

namespace GSvs.RoR2.Interactables
{
    public class InventoryScrapperOptions : MonoBehaviour
    {
        private ulong seed;
        private Xoroshiro128Plus rng;

        private void Awake()
        {
            ResetSeed();
        }

        private void OnEnable()
        {
            SceneCatalog.onMostRecentSceneDefChanged += OnMostRecentSceneDefChanged;
        }

        private void OnDisable() 
        {
            SceneCatalog.onMostRecentSceneDefChanged -= OnMostRecentSceneDefChanged;
        }

        private void OnMostRecentSceneDefChanged(SceneDef sceneDef)
        {
            ResetSeed();
        }

        public void ResetSeed()
        {
            seed = Run.instance.stageRng.nextUlong;
            rng = new Xoroshiro128Plus(seed);
        }

        public Xoroshiro128Plus GetSelectionRng()
        {
            rng.ResetSeed(seed);
            return rng;
        }
    }
}
