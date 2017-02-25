using System;

namespace OpenSatelliteProject {
    public class ConstellationModel: BaseModel {

        public float[] data { get; set; }

        public ConstellationModel(float[] data) : base("constellationData") {
            this.data = data;
        }
    }
}

