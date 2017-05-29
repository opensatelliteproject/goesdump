using System;
using System.Collections.Generic;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public static class StatisticsManager {
        const int checkInterval = 5; // Seconds
        static readonly List<DBStatistics> statistics;
        static int lastCheck;
        static long lastTimestamp;

        static StatisticsManager() {
            statistics = new List<DBStatistics> ();
            lastCheck = LLTools.Timestamp ();
            lastTimestamp = LLTools.TimestampMS ();
        }

        public static void Update(DBStatistics stats) {
            lock (statistics) {
                stats.Timestamp = LLTools.TimestampMS ();
                if (stats.Timestamp == lastTimestamp) {
                    stats.Timestamp++;
                }
                lastTimestamp = stats.Timestamp;

                statistics.Add (stats);
                if (LLTools.Timestamp () - lastCheck > checkInterval) {
                    long startTime = LLTools.TimestampMS ();
                    // Time to dump
                    foreach (var s in statistics) {
                        ConfigurationManager.PutStatistics (s, false);
                    }
                    statistics.Clear ();
                    lastCheck = LLTools.Timestamp ();
                    Console.WriteLine($"Save time {LLTools.TimestampMS() - startTime} ms");
                }
            }
        }
    }
}

