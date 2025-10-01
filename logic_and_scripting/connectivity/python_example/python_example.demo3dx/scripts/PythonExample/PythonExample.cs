//==============================================================================  
/// MIT License  
///  
/// Copyright (c) 2023 Rockwell Automation, Inc.  
///  
/// Permission is hereby granted, free of charge, to any person obtaining a copy  
/// of this software and associated documentation files (the "Software"), to deal  
/// in the Software without restriction, including without limitation the rights  
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell  
/// copies of the Software, and to permit persons to whom the Software is  
/// furnished to do so, subject to the following conditions:  
///  
/// The above copyright notice and this permission notice shall be included in all  
/// copies or substantial portions of the Software.  
///  
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR  
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,  
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE  
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER  
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE  
/// SOFTWARE.  
///  
//==============================================================================

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
