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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Demo3D.Common;
using Demo3D.Gui;
using Demo3D.Native;
using Demo3D.Utilities;
using Demo3D.Visuals;
using Microsoft.DirectX;
#endregion

namespace Demo3D.Components {
    [Auto] public class CustomSelectionLogic : NativeObject {
        public CustomSelectionLogic(Visual sender) : base(sender) { }


        [Auto, Category("-Custom"), ReadOnly(true), Description("The last visual selected.")]
        CustomVisualPropertyValue<Visual> LastVisualSelected;
        private static int PADDING_LENGTH = 30;

        [Auto] void OnLoaded(Visual visual) { ResetListeners(); }
        [Auto] void OnInitialize(Visual sender) { ResetListeners(); }
        [Auto] void OnReset(Visual sender) { ResetListeners(); }

        private void ResetListeners() {
            DeleteListeners();
            CreateListeners();
        }

        private void DeleteListeners() {
            if (app.SelectionManager is not null) {
                app.SelectionManager.SelectionChanged -= SelectionChanged_Listener;
                print("Listener action: deleting.");
            }
        }

        private void CreateListeners() {
            if (app.SelectionManager is not null) {
                app.SelectionManager.SelectionChanged -= SelectionChanged_Listener;
                app.SelectionManager.SelectionChanged += SelectionChanged_Listener;
                print("Listener action: creating.");
            }
        }

        private void SelectionChanged_Listener(object sender, EventArgs e) {
            // Include any "resetting" actions here.
            LastVisualSelected.Value.Visual.FadeToColorDeep(Color.White, 0);
            
            // Multi-select logic. Caution is advised as this can fast become an expensive operation inhibiting performance.
            if (app.Selection.Count > 1) { 
                StringBuilder sb = new StringBuilder();
                foreach (var v in app.Selection) {
                    sb.Append(v.Name + ", ");
                }
                sb.Remove(sb.Length - 2, 2);
                print("Multiple visuals selected.".PadRight(PADDING_LENGTH) + $"No multi-select hardcoded actions for visuals ({sb.ToString()}).");
                return; 
            }

            // Single-select logic.
            else if (app.Selection.Count == 1) {
                if (app.Selection[0].Name == "Box1") {
                    CustomSelectionOptions(Color.Green);
                }
                else if (app.Selection[0].Name == "Box2") {
                    CustomSelectionOptions(Color.Purple);
                }
                else {
                    print("Single visual selected.".PadRight(PADDING_LENGTH) + $"Currently no hardcoded actions for currently selected visual {app.Selection[0].Name}.");
                }
            }

            // None-select logic.
            else {
                print("No visual selected.".PadRight(PADDING_LENGTH) + $"Fading previous selection to white.");
            }
        }

        private void CustomSelectionOptions(Color color) {
            var currentVisual = document.FindAnyVisual(app.Selection[0].Name);
            currentVisual.FadeToColorDeep(color, 0);
            LastVisualSelected.Value = currentVisual;
            print("Single visual selected.".PadRight(PADDING_LENGTH) + $"Changing {currentVisual.Name} color to {color.Name}.");
        }

        public override void Dispose() {
            base.Dispose();
            DeleteListeners();
            print("Listener action: disposing.");
        }
    }
}