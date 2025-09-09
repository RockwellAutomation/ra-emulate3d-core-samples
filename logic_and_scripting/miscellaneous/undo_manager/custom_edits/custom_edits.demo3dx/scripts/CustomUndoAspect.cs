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

using System.Collections;
using Demo3D.Gui.AspectViewer;
using Demo3D.Visuals;
using Emulate3D.SourceGenerators;

namespace Demo3D.Components {
    public partial class CustomUndoAspect : VisualAspect
    {
        [GenerateProperty]
        private int myInt;

        [AspectMethod]
        public void AddOne()
        {
            var undoManager = app.Document.UndoRedoManager;
            undoManager.AddEdit(new CustomEdit(document, new[] { Visual }, MyInt, MyInt + 1));
            MyInt++;
        }
    }

    public class CustomEdit : DocumentVisualsEdit
    {
        private readonly int originalValue;
        private readonly int newValue;

        public CustomEdit(Document document, IList visuals, int originalValue, int newValue) : base(document, visuals)
        {
            this.originalValue = originalValue;
            this.newValue = newValue;
        }

        public override void Redo()
        {
            SetValues(newValue);
        }

        public override void Undo()
        {
            SetValues(originalValue);
        }

        private void SetValues(int value)
        {
            var visuals = FindVisuals();
            foreach (var visual in visuals)
            {
                var aspect = visual.FindAspect<CustomUndoAspect>();
                if (aspect != null)
                {
                    aspect.MyInt = value;
                }
            }
        }
    }
}