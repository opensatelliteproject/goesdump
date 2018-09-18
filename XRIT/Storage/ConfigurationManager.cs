using System;

namespace OpenSatelliteProject {
    public static class ConfigurationManager {
        const string ConfigFileName = "config.db";
        static readonly Database db;

        static ConfigurationManager() {
            db = new Database (ConfigFileName);
        }

        public static void PutStatistics(DBStatistics statistics, bool autoTimestamp = true) {
            db.PutStatistic (statistics, autoTimestamp);
        }

        public static string Get(string key, string def = null) {
            return db[key] ?? def;
        }

        public static void Set(string key, string value) {
            db[key] = value;
        }

        public static void Set(string key, bool value) {
            db.Set (key, value);
        }

        public static void Set(string key, int value) {
            db.Set (key, value);
        }

        public static void Set(string key, float value) {
            db.Set (key, value);
        }

        public static void Set(string key, double value) {
            db.Set (key, value);
        }

        public static int GetInt(string key) {
            return db.GetInt (key);
        }

        public static bool GetBool(string key) {
            return db.GetBool (key);
        }

        public static float GetFloat(string key) {
            return db.GetFloat (key);
        }

        public static double GetDouble(string key) {
            return db.GetDouble (key);
        }
    }
}

