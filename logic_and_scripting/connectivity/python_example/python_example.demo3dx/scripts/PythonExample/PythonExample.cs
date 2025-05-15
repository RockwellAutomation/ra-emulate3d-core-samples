#region Namespaces
using System.IO;
using System.Reflection;
using Demo3D.Common;
using Demo3D.TestRunner;
using Microsoft.VisualBasic.Logging;
using NuGet.Configuration;
using NUnit.Framework;
using Python.Runtime;
#endregion

namespace Demo3D.Components
{
    public class PythonExampleTest : Emulate3DTestFixture
    {
        [Emulate3DTest]
        public void TestPythonExample()
        {
            //Get the python nuget directory, version is the version of the nuget python packaged 
            var settings = Settings.LoadDefaultSettings(null);
            var dir = SettingsUtility.GetGlobalPackagesFolder(settings);
            var version = "3.12.4";
            var pythonDllDir = Path.Combine(dir, "python", version, "tools");
            //Initialize python dll
            if (Runtime.PythonDLL == null)
            {
                Runtime.PythonDLL = Path.Combine(pythonDllDir, "python312.dll");
                PythonEngine.Initialize();
                Logger.Log("Python DLL initialized");
            }

            var scriptDir = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(PythonExampleTest)).Location), "Scripts");
            var scriptPath = Path.Combine(scriptDir, "getpyobject.py");
            var script = File.ReadAllText(scriptPath);
            Logger.Log($"Python script loaded from {scriptPath}");
            using var py = Py.GIL();
            using var scope = Py.CreateScope();
            dynamic pythonScriptObj = scope.Exec(script);
            dynamic pythonObj = pythonScriptObj.getSimpleObject();
            dynamic value = pythonObj.getMyString();
            Assert.That(value.ToString() == "Hello from Python Object");

            pythonObj.setMyString("Hello from Python Object: Updated");
            value = pythonObj.getMyString();
            Assert.That(value.ToString() == "Hello from Python Object: Updated");
        }
    }
    
}