using Microsoft.DirectX;
using System;

namespace Demo3D.Components {
    public class Record {
        public int Id;
        public TimeSpan Time      { get; set; }
        public float    LocationX { get; set; }
        public float    LocationY { get; set; }
        public float    LocationZ { get; set; }
        public float    RotationX { get; set; }
        public float    RotationY { get; set; }
        public float    RotationZ { get; set; }
    }
}