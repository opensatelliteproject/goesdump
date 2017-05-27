using System;

namespace OpenSatelliteProject {
    public static class ConfigurationManager {
        private const string ConfigFileName = "config.db";
        private static Database db;

        static ConfigurationManager() {
            ConfigurationManager.db = new Database (ConfigFileName);
        }

        public static string Get(string key, string def = null) {
            return ConfigurationManager.db [key] ?? def;
        }

        public static void Set(string key, string value) {
            ConfigurationManager.db [key] = value;
        }

        public static void Set(string key, bool value) {
            ConfigurationManager.db.Set (key, value);
        }

        public static void Set(string key, int value) {
            ConfigurationManager.db.Set (key, value);
        }

        public static void Set(string key, float value) {
            ConfigurationManager.db.Set (key, value);
        }

        public static void Set(string key, double value) {
            ConfigurationManager.db.Set (key, value);
        }

        public static int GetInt(string key) {
            return  ConfigurationManager.db.GetInt (key);
        }

        public static bool GetBool(string key) {
            return  ConfigurationManager.db.GetBool (key);
        }

        public static float GetFloat(string key) {
            return  ConfigurationManager.db.GetFloat (key);
        }

        public static double GetDouble(string key) {
            return  ConfigurationManager.db.GetDouble (key);
        }
    }
}

