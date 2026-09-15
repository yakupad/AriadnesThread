using System;
using Apple.GameKit;
using Apple.GameKit.Leaderboards;
using UnityEngine;

namespace AriadnesThread.GameCenterIntegration
{
    /// <summary>
    /// Thin wrapper around Apple.GameKit. GameKit only actually authenticates on a real
    /// iOS/macOS build signed into Game Center — it cannot in the Unity Editor's Play Mode
    /// (Apple.Core's macOS native library still lets the Editor compile/run against it, but
    /// GKLocalPlayer.Authenticate() will fail there since there's no Game Center session).
    /// Every public method here fails soft with a log rather than throwing, so the rest of
    /// the game never needs its own "is GameKit available" checks.
    /// </summary>
    public class GameCenterManager : MonoBehaviour
    {
        public static GameCenterManager Instance { get; private set; }

        public bool IsAuthenticated { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public async void Authenticate()
        {
            try
            {
                await GKLocalPlayer.Authenticate();
                IsAuthenticated = GKLocalPlayer.Local.IsAuthenticated;
                Debug.Log($"Game Center: authentication {(IsAuthenticated ? "succeeded" : "did not complete")}.");
            }
            catch (Exception ex)
            {
                IsAuthenticated = false;
                Debug.LogWarning($"Game Center: authentication failed — {ex.Message}");
            }
        }

        /// <summary>Fire-and-forget: a failed submission should never block the level-end flow.</summary>
        public async void SubmitScore(long score, string leaderboardId)
        {
            if (!IsAuthenticated) return;

            try
            {
                var leaderboards = await GKLeaderboard.LoadLeaderboards(leaderboardId);
                if (leaderboards.Count == 0)
                {
                    Debug.LogWarning($"Game Center: leaderboard '{leaderboardId}' not found — check the ID matches App Store Connect exactly.");
                    return;
                }

                await leaderboards[0].SubmitScore(score, context: 0, GKLocalPlayer.Local);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Game Center: failed to submit score to '{leaderboardId}' — {ex.Message}");
            }
        }

        public async void ReportAchievement(string achievementId, double percentComplete = 100)
        {
            if (!IsAuthenticated) return;

            try
            {
                var achievement = GKAchievement.Init(achievementId);
                achievement.PercentComplete = percentComplete;
                achievement.ShowCompletionBanner = true;
                await GKAchievement.Report(achievement);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Game Center: failed to report achievement '{achievementId}' — {ex.Message}");
            }
        }
    }
}
