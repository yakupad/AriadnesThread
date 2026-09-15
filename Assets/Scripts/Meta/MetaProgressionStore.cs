using AriadnesThread.Core.Meta;
using UnityEngine;

namespace AriadnesThread.Meta
{
    /// <summary>
    /// PlayerPrefs-backed persistence — adequate for a prototype. A real save file would
    /// replace this without CrystalWallet/UpgradeTrack (Core, engine-agnostic) ever knowing.
    /// </summary>
    public static class MetaProgressionStore
    {
        private const string CrystalKey = "AriadnesThread.Crystals";
        private const string MarkerUpgradeKey = "AriadnesThread.MarkerUpgradeLevel";
        private const string TorchUpgradeKey = "AriadnesThread.TorchUpgradeLevel";
        private const string EchoUpgradeKey = "AriadnesThread.EchoUpgradeLevel";

        public static CrystalWallet LoadWallet() => new CrystalWallet(PlayerPrefs.GetInt(CrystalKey, 0));

        public static void SaveWallet(CrystalWallet wallet) => PlayerPrefs.SetInt(CrystalKey, wallet.Balance);

        public static UpgradeTrack LoadMarkerUpgrade(int maxLevel, int baseCost) =>
            new UpgradeTrack(maxLevel, baseCost, startingLevel: PlayerPrefs.GetInt(MarkerUpgradeKey, 0));

        public static UpgradeTrack LoadTorchUpgrade(int maxLevel, int baseCost) =>
            new UpgradeTrack(maxLevel, baseCost, startingLevel: PlayerPrefs.GetInt(TorchUpgradeKey, 0));

        public static UpgradeTrack LoadEchoUpgrade(int maxLevel, int baseCost) =>
            new UpgradeTrack(maxLevel, baseCost, startingLevel: PlayerPrefs.GetInt(EchoUpgradeKey, 0));

        public static void SaveUpgrades(UpgradeTrack marker, UpgradeTrack torch, UpgradeTrack echo)
        {
            PlayerPrefs.SetInt(MarkerUpgradeKey, marker.Level);
            PlayerPrefs.SetInt(TorchUpgradeKey, torch.Level);
            PlayerPrefs.SetInt(EchoUpgradeKey, echo.Level);
            PlayerPrefs.Save();
        }
    }
}
