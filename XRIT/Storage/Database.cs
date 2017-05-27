using SQLite;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public class Database {
        readonly SQLiteConnection conn;

        public Database (string filename) {
            conn = new SQLiteConnection (filename);
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
                var res = conn.Table<DBConfig> ().Where (a => a.Name == key);
                if (res.Count() > 0) {
                    return res.First ().Value;
                }
                return null;
            }
            set {
                conn.Insert (new DBConfig () { Name = key, Value = value }, "OR REPLACE");
            }
        }
    }
}

