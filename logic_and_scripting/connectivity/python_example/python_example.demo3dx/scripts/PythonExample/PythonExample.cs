#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Demo3D.Common;
using Demo3D.Gui;
using Demo3D.Native;
using Demo3D.Utilities;
using Demo3D.Visuals;
using Microsoft.DirectX;
using NuGet.Configuration;
using Python.Runtime;
using static Demo3D.Time.RealTime;
#endregion

namespace Demo3D.Components {
    [Auto] public class PythonExample {
        #region Examples
        [Auto] IBuilder app;
        [Auto] Document document;
        [Auto] PrintDelegate print;
        [Auto] VectorDelegate vector;
        #endregion

        [Auto] void OnReset(Visual sender) {
            //Get the python nuget directory, version is the version of the nuget python packaged 
            var settings = Settings.LoadDefaultSettings(null);
            var dir = SettingsUtility.GetGlobalPackagesFolder(settings);
            var version = "3.12.4";
            var pythonDllDir = Path.Combine(dir, "python", version, "tools");
            //Initialize python dll
            if (Runtime.PythonDLL == null) {
                Runtime.PythonDLL = Path.Combine(pythonDllDir, "python312.dll");
                PythonEngine.Initialize();
            }

            var scriptDir = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(PythonExample)).Location), "Scripts");
            var scriptPath = Path.Combine(scriptDir, "getpyobject.py");
            var script = File.ReadAllText(scriptPath);
            using var py = Py.GIL();

            using var scope = Py.CreateScope();
            dynamic pythonScriptObj = scope.Exec(script);
            dynamic pythonObj = pythonScriptObj.getSimpleObject();
            dynamic value = pythonObj.getMyString();
            print(value);

            pythonObj.setMyString("Hello from Python Object: Updated");
            value = pythonObj.getMyString();
            print(value);
        }
    }
}