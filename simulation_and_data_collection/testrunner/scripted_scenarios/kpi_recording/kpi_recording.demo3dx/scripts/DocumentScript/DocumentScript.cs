//==============================================================================  
/// MIT License  
///  
/// Copyright (c) 2025 Rockwell Automation, Inc.  
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
using System.IO;
using System.Threading.Tasks;
using Demo3D.TestRunner;
using Demo3D.TestRunner.KpiRecorders;
using Demo3D.Visuals;
using NUnit.Framework;

[Auto] public class DocumentScript : NativeObject {
    public DocumentScript(Visual sender) : base(sender) {}
}

public class Tests : Emulate3DTestFixture
{
    private double ModelRunLength = 40; // seconds
    private double KPIRecorderStep = 5; // seconds

    private IKpiRecorder kpiRecorder;

    private string ModelDirectory { get
        {
            var modelPath = App.Document.Path;
            var dir = Path.GetDirectoryName(modelPath);
            return dir;
        }
    }

    [SetUp]
    public void Setup()
    {
        ModelAction(App.Reset);
    }

    [TearDown]
    public async Task Teardown()
    {
        await kpiRecorder.FinalizeAsync();
    }

    // A test which uses the same KPI collector that Test Scenarios use.
    [Emulate3DTest]
    public async Task InbuiltCsvKpiRecorder()
    {
        ModelAction(() =>
        {
            SetInitialParameters(5f);
        });

        //Setup the KPI Recorder
        kpiRecorder = await KpiRecorderBuilder.Create()
            .RecordModelTime(App, true)
            .RecordRealTime(false)
            .RegisterCollector<CsvKpiCollector>(r => r.OutputPath = Path.Combine(ModelDirectory, "Results_" + TestContext.CurrentContext.Test.Name + ".csv"))
            .BuildAsync();
        AddKpiRecordingActions(kpiRecorder);

        await RunModelForSecondsAsync(ModelRunLength, -1);
        EvaluateModel();
    }

    // A test which uses a custom CSV collector, with a slow conveyor speed.
    [Emulate3DTest]
    public async Task CustomCsvKpiRecorderSlow()
    {
        ModelAction(() =>
        {
            SetInitialParameters(0.5f);
        });

        //Setup the KPI Recorder
        kpiRecorder = await KpiRecorderBuilder.Create()
            .RecordModelTime(App, true)
            .RegisterCollector<CustomCsvKpiCollector>(r => {
                r.OutputPath = r.OutputPath = Path.Combine(ModelDirectory, "Results_Custom.csv");
                r.Append = true;
                r.TestName = TestContext.CurrentContext.Test.Name;
             })
            .BuildAsync();
        AddKpiRecordingActions(kpiRecorder);

        await RunModelForSecondsAsync(ModelRunLength, -1);
        EvaluateModel();
    }

    // A test which uses a custom CSV collector, with a fast conveyor speed.
    [Emulate3DTest]
    public async Task CustomCsvKpiRecorderFast()
    {
        ModelAction(() =>
        {
            SetInitialParameters(10f);
        });

        //Setup the KPI Recorder
        kpiRecorder = await KpiRecorderBuilder.Create()
            .RecordModelTime(App, true)
            .RegisterCollector<CustomCsvKpiCollector>(r => {
                r.OutputPath = r.OutputPath = Path.Combine(ModelDirectory, "Results_Custom.csv");
                r.Append = true;
                r.TestName = TestContext.CurrentContext.Test.Name;
            })
            .BuildAsync();
        AddKpiRecordingActions(kpiRecorder);

        await RunModelForSecondsAsync(ModelRunLength, -1);
        EvaluateModel();
    }

    private void SetInitialParameters(float convSpeed)
    {
        var conv = Document.FindVisual("StraightBeltConveyor1") as StraightBeltConveyor;
        Assert.IsNotNull(conv);
        conv.MotorSpeed = convSpeed;
    }

    /// <summary>
    /// Add the KPI recording actions to run at the desired times.
    /// </summary>
    /// <param name="recorder"></param>
    private void AddKpiRecordingActions(IKpiRecorder recorder)
    {
        //Record the blocked count every 5 seconds
        for (var i = 0.0; i < ModelRunLength; i += KPIRecorderStep)
        {
            RunActionAtTime(i, () =>
            {
                recorder.Record("Blocked Count", GetBlockedCount());
                recorder.Record("Load Count", App.Document.PhysicsEngine.CountLoads());
            });
        }
    }

    private void EvaluateModel()
    {
        Assert.That(GetBlockedCount(), Is.GreaterThan(20));
    }

    private int GetBlockedCount()
    {
        var photoEye = Document.FindVisual("PhotoEye1");
        Assert.IsNotNull(photoEye);

        var myIntProp = photoEye.FindCustomProperty("BlockedCount");
        return myIntProp.ValueAs<int>();
    }
}