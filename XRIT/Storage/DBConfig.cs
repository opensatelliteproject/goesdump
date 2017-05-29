using System;
using SQLite;

namespace OpenSatelliteProject {
    public class DBConfig {
        [PrimaryKey, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Value { get; set; }
    }
}

