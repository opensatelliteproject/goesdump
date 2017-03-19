using System;

namespace OpenSatelliteProject.Log {

    public class Message {
        public int Facility { get; set; }

        public int Level { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        public Message() {
            Name = "OpenSatelliteProject";
        }

        public Message(int facility, int level, string text) {
            this.Facility = facility;
            this.Level = level;
            this.Text = text;
            this.Name = "OpenSatelliteProject";
        }

        public Message(Facility facility, Level level, string text) {
            this.Facility = (int)facility;
            this.Level = (int)level;
            this.Text = text;
            this.Name = "OpenSatelliteProject";
        }

        public Message(int facility, int level, string name, string text) {
            this.Facility = facility;
            this.Level = level;
            this.Text = text;
            this.Name = name;
        }

        public Message(Facility facility, Level level, string name, string text) {
            this.Facility = (int)facility;
            this.Level = (int)level;
            this.Text = text;
            this.Name = name;
        }
    }

}

