using System.Collections.Generic;

namespace LiDARMimic {
    // Tracks active lidar devices so the render feature (main camera) can reach the current device's buffers.
    static class lidar_registry {
        static readonly List<lidar> active = new();

        public static IReadOnlyList<lidar> devices => active;

        public static void register(lidar d) {
            active.Add(d);
        }

        public static void unregister(lidar d) {
            active.Remove(d);
        }
    }
}
