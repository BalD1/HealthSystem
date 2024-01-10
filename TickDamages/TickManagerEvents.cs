using System;

namespace StdNounou
{
    public static class TickManagerEvents
    {
        public static event Action<int> OnTick;
        public static void PerformTick(this TickManager manager, int tick)
            => OnTick?.Invoke(tick);
    }

}