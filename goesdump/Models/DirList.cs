using System;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    public class DirList: BaseModel {
        public List<DHInfo> Listing { get; set; }

        public DirList (List<DHInfo> listing) : base("dirlist")  {
            this.Listing = listing;
        }
    }
}

