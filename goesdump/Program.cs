#region Using Statements

using System;
using System.Collections.Generic;
using System.Linq;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#endif
#endregion

namespace OpenSatelliteProject {
    static class Program {
        

        private static HeadlessMain main;
        internal static void RunProg() {
            UIConsole.Init();
            main = new HeadlessMain();
            main.Start();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        #if !MONOMAC
        [STAThread]
        #endif
        static void Main(string[] args) {
            #if MONOMAC
            NSApplication.Init ();

            using (var p = new NSAutoreleasePool ()) {
                NSApplication.SharedApplication.Delegate = new AppDelegate();
                NSApplication.Main(args);
            }
            #else
            RunProg();
            #endif
        }

    }

    #if MONOMAC
    class AppDelegate : NSApplicationDelegate
    {
        public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs a) =>  {
                if (a.Name.StartsWith("MonoMac")) {
                    return typeof(MonoMac.AppKit.AppKitFramework).Assembly;
                }
                return null;
            };
            Program.RunProg();
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
        {
            return true;
        }
    }  
    #endif
}

