using System;

namespace AriadnesThread.Core.Economy
{
    /// <summary>
    /// Step-based (not time-based) torch fuel — same fairness reasoning as the marker
    /// economy: device speed or AFK time must never change how far the fuel goes.
    /// </summary>
    public sealed class TorchEconomy
    {
        public int Fuel { get; private set; }
        public int MaxFuel { get; }
        public bool IsLit { get; private set; }
        public float BaseVisionRadius { get; }
        public float LitVisionRadius { get; }

        public TorchEconomy(int startingFuel, int maxFuel, float baseVisionRadius = 1f, float litVisionRadius = 4.5f)
        {
            Fuel = startingFuel;
            MaxFuel = maxFuel;
            BaseVisionRadius = baseVisionRadius;
            LitVisionRadius = litVisionRadius;
        }

        public float CurrentVisionRadius => IsLit ? LitVisionRadius : BaseVisionRadius;

        /// <summary>Player-controlled — never forced on by the tension state machine.</summary>
        public void SetLit(bool lit) => IsLit = lit && Fuel > 0;

        /// <summary>Call exactly once per grid-cell crossing, not per frame.</summary>
        public void OnStep()
        {
            if (!IsLit) return;

            Fuel--;
            if (Fuel <= 0)
            {
                Fuel = 0;
                IsLit = false;
            }
        }

        public void AddFuel(int amount) => Fuel = Math.Min(MaxFuel, Fuel + amount);
    }
}
