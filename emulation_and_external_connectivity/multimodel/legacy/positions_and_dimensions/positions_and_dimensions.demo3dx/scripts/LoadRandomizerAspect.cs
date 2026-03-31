using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Demo3D.Common;
using Demo3D.Visuals;

namespace Demo3D.Components {
    public class LoadRandomizerAspect : VisualAspect
    {
        private Random random = new Random();
        public LoadCreatorVisual LoadCreator { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (LoadCreator == null)
            {
                Logger.Log("Error", "LoadCreator is not set for LoadRandomizerAspect.", this);
                return;
            }

            LoadCreator.OnLoadCreated.NativeListeners += OnLoadCreated;
        }

        private void OnLoadCreated(Visual sender, Visual arg)
        {
            arg.RotationYDegrees = random.Next(0, 360);
            arg.AddCustomProperty("MM_RotationY", typeof(float), arg.RotationYDegrees, string.Empty);
            if (arg is ImportedMeshVisual mo)
            {
                mo.Depth = random.Next(1, 5) / 10.0f; // Random depth between 0.1 and 0.4
                mo.Width = random.Next(1, 5) / 10.0f;
                mo.Height = random.Next(1, 5) / 10.0f; // Random width between 0.1 and 0.4
                arg.AddCustomProperty("MM_Depth", typeof(float), mo.Depth, string.Empty);
                arg.AddCustomProperty("MM_Width", typeof(float), mo.Width, string.Empty);
                arg.AddCustomProperty("MM_Height", typeof(float), mo.Height, string.Empty);
            }
                
        }
    }
}