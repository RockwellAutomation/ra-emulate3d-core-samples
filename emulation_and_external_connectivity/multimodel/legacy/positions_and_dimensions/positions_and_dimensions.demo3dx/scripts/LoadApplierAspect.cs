using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Demo3D.Common;
using Demo3D.MultiModel.Client;
using Demo3D.Visuals;

namespace Demo3D.Components {
    public class LoadApplierAspect : VisualAspect
    {


        public MultiModelSenderReceiverAspect ReceiverAspect { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (ReceiverAspect == null)
            {
                Logger.Log("Error", "LoadCreator is not set for LoadRandomizerAspect.", this);
                return;
            }

            ReceiverAspect.OnLoadReceived += OnLoadReceived;
        }

        private void OnLoadReceived(IMultiModelReceiver receiver, Visual visual)
        {
            visual.RotationYDegrees = GetValue<float>(visual, "MM_RotationY");
            if (visual is ImportedMeshVisual mo)
            {

                mo.Depth = GetValue<float>(visual, "MM_Depth");
                mo.Width = GetValue<float>(visual, "MM_Width");

                //If we change the height, we need to move the load down half the height difference.
                var initialHeight = mo.Height;
                mo.Height = GetValue<float>(visual, "MM_Height");
                if (initialHeight != mo.Height)
                {
                    mo.LocationY -= (initialHeight - mo.Height) / 2.0f;
                }

            }
        }
        
        private T GetValue<T>(Visual arg, string propertyName)
        {
            var property = arg.FindCustomProperty(propertyName);
            if (property == null)
            {
                Logger.Log("Error", $"{propertyName} property not found on the created load visual.", this);
                return default;
            }
            return property.ValueAs<T>();
        }
    }
}