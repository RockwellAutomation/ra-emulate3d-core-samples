//==============================================================================  
/// MIT License  
///  
/// Copyright (c) 2026 Rockwell Automation, Inc.  
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


using System;
using System.Collections.Generic;
using System.Linq;
using Demo3D.Visuals;

public class SelectedVisualService : IDisposable
{
    private readonly IBuilder app;
    private readonly List<Visual> selectedVisuals = [];
    public event Action SelectedVisualChanged;

    public IReadOnlyList<Visual> SelectedVisuals => selectedVisuals;

    public SelectedVisualService(IBuilder app)
    {
        this.app = app;
        app.SelectionManager.SelectionChanged += OnSelectionChanged;
        RefreshSelectedVisuals();
    }

    private void OnSelectionChanged(object sender, EventArgs e)
    {
        RefreshSelectedVisuals();
    }

    private void RefreshSelectedVisuals()
    {
        UnsubscribeTrackedSources();
        selectedVisuals.Clear();
        selectedVisuals.AddRange(app.SelectionManager.Selection.OfType<Visual>());
        SubscribeTrackedSources();

        NotifySelectedVisualChanged();
    }

    private void OnMoved(Visual sender, MatrixUpdateType arg)
    {
        //NOTE: We could be more efficient here by only notifying about the specific visual that moved, 
        // but for simplicity we'll just notify that the selection changed.
        NotifySelectedVisualChanged();
    }

    private void SubscribeTrackedSources()
    {
        foreach (var visual in selectedVisuals)
        {
            visual.OnMoved.NativeListeners += OnMoved;
        }
    }

    private void UnsubscribeTrackedSources()
    {
        foreach (var visual in selectedVisuals)
        {
            visual.OnMoved.NativeListeners -= OnMoved;
        }
    }

    private void NotifySelectedVisualChanged()
    {
        SelectedVisualChanged?.Invoke();
    }

    public void Dispose()
    {
        app.SelectionManager.SelectionChanged -= OnSelectionChanged;
        UnsubscribeTrackedSources();
    }
}
