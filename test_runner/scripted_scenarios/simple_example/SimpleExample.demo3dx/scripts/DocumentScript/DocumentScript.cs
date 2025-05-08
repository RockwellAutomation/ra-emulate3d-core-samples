using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Demo3D.Native;
using Demo3D.TestRunner;
using Demo3D.Visuals;
using NUnit.Framework;

[Auto] public class DocumentScript : NativeObject {
    public DocumentScript(Visual sender) : base(sender) {}
}

public class Tests : Emulate3DTestFixture {

    private double ModelRunLength = 40; // seconds

    [SetUp]
    public void Setup() {
        ModelAction(App.Reset);
    }

    [Emulate3DTest] public async Task Test1() {
        ModelAction( () => {
            SetInitialParameters(0.5f);
        });
        await RunModelForSecondsAsync(ModelRunLength, -1);
        EvaluateModel();
    }

    [Emulate3DTest] public async Task Test2() {
        ModelAction( () => {
            SetInitialParameters(5f);
        });
        await RunModelForSecondsAsync(ModelRunLength, -1);
    }

    private void SetInitialParameters(float convSpeed) {
        var conv = Document.FindVisual("StraightBeltConveyor1") as StraightBeltConveyor;
        Assert.IsNotNull(conv);
        conv.MotorSpeed = convSpeed;
    }

    private void EvaluateModel()
    {
        var photoEye = Document.FindVisual("PhotoEye1");
        Assert.IsNotNull(photoEye);

        var myIntProp = photoEye.FindCustomProperty("BlockedCount");
        Assert.That(myIntProp.ValueAs<int>(), Is.GreaterThan(20));
    }
}