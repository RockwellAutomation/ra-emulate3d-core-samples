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

#nullable enable

using System;
using Demo3D.Common;
using Demo3D.Native;
using Demo3D.Visuals;
using Microsoft.DirectX;

namespace Demo3D.Components {
    [Auto] sealed public class LoadCreator : IDisposable {
        readonly LoadCreatorVisual? lc;

        [Auto, ExportProperty(DontInitialize = true)]
        readonly TemporaryPropertyValue<LoadDescription> LoadDescription = null!;

        public LoadCreator(Visual sender) {
            sender.SetNativeObject(this);

            if (sender is not LoadCreatorVisual lc) {
                Logger.Log("Error", $"Visual \"{sender.FullName}\" is not a load creator");
            } else {
                this.lc = lc;
                LoadDescription.ValueChanged += LoadDescription_ValueChanged;
            }
        }

        void LoadDescription_ValueChanged(BindableItem _) {
            if (lc is null || LoadDescription?.Value?.Name is null) return;

            var load = lc.CreateLoadNow();
            if (load is null) return;  // congestion zone

            var loadDescr      = LoadDescription.Value;
            load.Name          = loadDescr.Name;
            var offset         = new Vector3(loadDescr.OffsetX, loadDescr.OffsetY, loadDescr.OffsetZ);
            load.WorldLocation = lc.WorldLocation + lc.BoundingBox.Bottom + offset;
        }

        public void Dispose() {
            LoadDescription?.ValueChanged -= LoadDescription_ValueChanged;
        }
    }
}