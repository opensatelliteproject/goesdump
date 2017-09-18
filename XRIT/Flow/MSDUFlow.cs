using System;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.FlowData;

namespace OpenSatelliteProject.Flow {
    /// <summary>
    /// A Interface to make a processor for the MSDU Part of the Flow Processor
    /// </summary>
    public interface MSDUFlow {
        /// <summary>
        /// Creates a context object for the FinishMSDU calls.
        /// </summary>
        /// <returns>The context.</returns>
        /// <param name="toTransportFlow">The function to be called when the data is ready to transport flow</param>
        object CreateContext (Func<TransportFileData, bool> toTransportFlow);

        /// <summary>
        /// The function to be called when an MSDU was finished being received.
        /// </summary>
        /// <param name="msdu">MSDU that has finished</param>
        /// <param name="ctx">Context object created with CreateContext</param>
        void FinishMSDU (MSDU msdu, object ctx);
    }
}

