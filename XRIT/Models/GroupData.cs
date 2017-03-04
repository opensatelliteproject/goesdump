using System;

namespace OpenSatelliteProject {
    public class GroupData {
        public string SatelliteName { get; set; }
        public string RegionName { get; set; }
        public DateTime FrameTime { get; set; }
        public OrganizerData Visible { get; set; }
        public OrganizerData Infrared { get; set; }
        public OrganizerData WaterVapour { get; set; }

        public bool IsProcessed { get; set; }
        public bool IsComplete { get { return Visible.IsComplete && Infrared.IsComplete && WaterVapour.IsComplete; } }

        public GroupData() {
            SatelliteName = "Unknown";
            RegionName = "Unknown";
            FrameTime = DateTime.Now;
            Visible = new OrganizerData();
            Infrared = new OrganizerData();
            WaterVapour = new OrganizerData();
            IsProcessed = false;
        }

        public override string ToString() {
            return string.Format(
                "Satellite Name: {0}\n" +
                "Region Name: {1}\n" +
                "Frame Time: {2}\n" +
                "Visible Segments: {3} ({4})\n" +
                "Infrared Segments: {5} ({6})\n" +
                "Water Vapour Segments: {7} ({8})\n" +
                "\n",
                SatelliteName,
                RegionName,
                FrameTime,
                Visible.Segments.Count,
                Visible.IsComplete ? "Complete" : "Incomplete",
                Infrared.Segments.Count,
                Infrared.IsComplete ? "Complete" : "Incomplete",
                WaterVapour.Segments.Count,
                WaterVapour.IsComplete ? "Complete" : "Incomplete"
            );
        }
    }
}

