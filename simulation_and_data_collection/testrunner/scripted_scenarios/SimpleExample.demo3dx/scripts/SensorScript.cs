using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Demo3D.Native;
using Demo3D.Visuals;

[Auto] public class SensorScript : NativeObject {

    [Auto] CustomPropertyValue<int> BlockedCount;

    public SensorScript(Visual sender) : base(sender) {}
    
    [Auto] void OnReset(Visual sender) {
        BlockedCount.Value = 0;
    }

    [Auto] void OnBlocked(PhotoEye sender, Visual load) {
        BlockedCount.Value++;
    }

}