using System.Collections.Generic;
using System.Linq;

namespace LiDARMimic {
    // Tracks enabled lidar_receiver components and assigns each a stable id (> 0, small range).
    // id 0 is reserved for background / non-receiver surfaces in the LiDAR pass.
    static class lidar_receiver_registry {
        static readonly List<lidar_receiver> active = new();

        public static IReadOnlyList<lidar_receiver> receivers => active;

        public static void register(lidar_receiver r) {
            var used = active.Select(x => x.id).ToHashSet();
            r.id = Enumerable.Range(1, active.Count + 1).First(i => !used.Contains(i));
            active.Add(r);
        }

        public static void unregister(lidar_receiver r) {
            active.Remove(r);
        }
    }
}
