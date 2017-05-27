using SQLite;
using OpenSatelliteProject.Tools;
using System.Collections.Generic;
using System;

namespace OpenSatelliteProject {
    public class Database {
        readonly SQLiteConnection conn;
        readonly Dictionary<string, string> configCache;

        public Database (string filename) {
            conn = new SQLiteConnection (filename);
            configCache = new Dictionary<string, string> ();
            Init ();
        }

        void Init() {
            var x = conn.GetTableInfo ("DBConfig");
            if (x.Count == 0) {
                conn.CreateTable<DBConfig> ();
            }
            x = conn.GetTableInfo ("DBStatistics");
            if (x.Count == 0) {
                conn.CreateTable<DBStatistics> ();
            }
        }

        public void Close() {
            conn.Close ();
        }

        public void PutStatistic(DBStatistics statistics) {
            statistics.Timestamp = LLTools.Timestamp ();
            conn.Insert (statistics);
        }

        public string this[string key] {
            get {
                if (!configCache.ContainsKey (key)) {
                    // Fetch from DB
                    var res = conn.Table<DBConfig> ().Where (a => a.Name == key);
                    configCache [key] = res.Count () > 0 ? res.First ().Value : null;
                }
                return configCache[key];
            }
            set {
                conn.Insert (new DBConfig () { Name = key, Value = value }, "OR REPLACE");
                configCache [key] = value;
            }
        }

        #region Typed Setter / Getter

        public void Set(string key, bool value) {
            this [key] = value.ToString ();
        }

        public void Set(string key, int value) {
            this [key] = value.ToString ();
        }

        public void Set(string key, float value) {
            this [key] = value.ToString ();
        }

        public void Set(string key, double value) {
            this [key] = value.ToString ();
        }

        public int GetInt(string key) {
            try {
                return int.Parse(this[key]);
            } catch (Exception) {
                return 0;
            }
        }

        public bool GetBool(string key) {
            return this[key] == "true";
        }

        public float GetFloat(string key) {
            try {
                return float.Parse(this[key]);
            } catch (Exception) {
                return float.NaN;
            }
        }

        public double GetDouble(string key) {
            try {
                return double.Parse(this[key]);
            } catch (Exception) {
                return double.NaN;
            }
        }

        #endregion
    }
}

