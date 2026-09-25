using System.Collections.Generic;
using UnityEngine;

namespace HatHop
{
    public sealed class LevelStars : MonoBehaviour
    {
        [SerializeField] private LevelFlow flow;
        [SerializeField] private Transform map;
        [SerializeField] private int levelIndex;
        [SerializeField] private Texture2D icon;
        private StarCollectible[] stars;
        private readonly HashSet<StarCollectible> pending = new HashSet<StarCollectible>();
        public int Collected { get; private set; }
        public int Total => stars == null ? 0 : stars.Length;
        public Texture2D Icon => icon;
        public int Best => BestFor(levelIndex);
        public static int BestFor(int index) => Mathf.Clamp(PlayerPrefs.GetInt(Key(index), 0), 0, 5);
        private static string Key(int index) => "HatHop.BestStars.v1." + index;

        public void Configure(LevelFlow level, Transform mapRoot, int index, Texture2D image)
        { flow = level; map = mapRoot; levelIndex = index; icon = image; }

        private void Awake()
        {
            stars = map == null ? new StarCollectible[0] : map.GetComponentsInChildren<StarCollectible>(true);
            if (stars.Length != 5) Debug.LogWarning("This rated level should contain exactly five stars.", this);
        }

        public void Report(StarCollectible star, Rigidbody2D body)
        {
            if (isActiveAndEnabled && flow != null && flow.AcceptsPlayerContact(body) &&
                !star.Collected && System.Array.IndexOf(stars, star) >= 0) pending.Add(star);
        }

        // LevelFlow calls this only after resolving death priority and before committing a win.
        public void ResolvePending()
        {
            foreach (StarCollectible star in pending)
                if (star != null && !star.Collected) { star.SetCollected(true); Collected++; }
            pending.Clear();
        }
        public void DiscardPending() => pending.Clear();
        public void ResetAttempt()
        {
            pending.Clear(); Collected = 0;
            if (stars != null) foreach (StarCollectible star in stars) if (star != null) star.SetCollected(false);
        }
        public void SaveCompletedRun()
        {
            if (levelIndex < 0 || levelIndex > 2) return;
            if (Collected > Best) PlayerPrefs.SetInt(Key(levelIndex), Mathf.Clamp(Collected, 0, 5));
            PlayerPrefs.SetInt("HatHop.Completed.v1." + levelIndex, 1);
            PlayerPrefs.Save();
        }
    }
}
