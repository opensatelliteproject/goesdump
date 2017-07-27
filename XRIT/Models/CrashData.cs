using System;
using System.Web.Script.Serialization;
using OpenSatelliteProject.Tools;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenSatelliteProject {
    public class CrashData {

        public string ID { get; private set; }
        public CrashData InnerCrashData { get; private set; }
        public string StackTrace { get; private set; }
        public string Source { get; private set; }
        public int ExceptionCode { get; private set; }
        public string ExceptionName { get; private set; }
        public string Date { get; private set; }
        public string Time { get; private set; }
        public long Timestamp { get; private set; }
        public string XRITVersion { get; private set; }
        public string XRITCommit { get; private set; }
        public List<StackTraceData> StackTraceData { get; private set; }
        public string Username { get; private set; }

        public string Filename { 
            get {
                return (StackTraceData.Count > 0) ? StackTraceData [0].Filename : "No File";
            } 
        }

        public int Line { 
            get {
                return (StackTraceData.Count > 0) ? StackTraceData [0].Line : 0;
            } 
        }

        public CrashData(Exception e) : this(e, "Not Defined") {}

        public CrashData (Exception e, string Username) {
            this.Username = Username;
            StackTrace trace = new StackTrace(e, true);
            StackTraceData = trace.GetFrames ().Select ((sf) => {
                return new StackTraceData {
                    Line = sf.GetFileLineNumber (),
                    Column = sf.GetFileColumnNumber (),
                    Filename = sf.GetFileName (),
                    ClassName =  sf.GetMethod ().ReflectedType.FullName,
                    Method = sf.GetMethod ().Name,
                };
            }).ToList ();

            ID = Guid.NewGuid ().ToString ();
            ExceptionCode = e.HResult;
            Source = e.Source;
            StackTrace = e.StackTrace;
            ExceptionName = e.GetType ().Name;
            Timestamp = LLTools.TimestampMS ();
            Date = DateTime.Now.ToLongDateString ();
            Time = DateTime.Now.ToLongTimeString ();
            XRITVersion = LibInfo.Version;
            XRITCommit = LibInfo.CommitID;
            if (e.InnerException != null) {
                InnerCrashData = new CrashData (e);
            }
        }

        public string ToJSON() {
            try {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            } catch (Exception e) {
                Console.WriteLine ($"FATAL: Cannot serialize Crash Data: {e}");
            }
            return "error";
        }
    }
}

