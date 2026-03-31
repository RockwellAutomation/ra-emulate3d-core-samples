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


using Demo3D.Common;
using Demo3D.Components;
using Demo3D.Components.Components;
using Demo3D.Visuals;
using Emulate3D.WebApp;
using Emulate3D.WebConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

[Emulate3DWebService("My Service")]
public partial class MyService
{
    private SelectedVisualService selectedVisualService;

    public partial void Build(IBuilder app, WebAppServerBuilder builder)
    {   
        selectedVisualService?.Dispose();
        selectedVisualService = new SelectedVisualService(app);
        builder.WithServices(services =>
        {
            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            services.AddSingleton(selectedVisualService);
            
            services.AddSingleton<IComponentActivator, AssemblyRemappingComponentActivator>();
        });

        var dir = typeof(MyService).Assembly.Location;
        var wwwroot = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dir), "wwwroot");

        if (System.IO.Directory.Exists(wwwroot))
        {
            Logger.Log("Info", $"wwwroot directory found at {wwwroot}, configuring static file serving");
            builder.WithStaticDirectory(string.Empty, wwwroot);
        }
        else
        {
            Logger.Log("Error", $"wwwroot directory not found at {wwwroot}");
        }

        builder.WithPostBuildAction(app =>
        {
            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        });
    }

    public partial void OnServerStarted(IBuilder app)
    {
        // Nothing to do
    }

    public partial void OnServerStopping(IBuilder app)
    {
        selectedVisualService?.Dispose();
        selectedVisualService = null;
    }

    public partial void OnServerStopped(IBuilder app)
    {
        // Nothing to do
    }
}