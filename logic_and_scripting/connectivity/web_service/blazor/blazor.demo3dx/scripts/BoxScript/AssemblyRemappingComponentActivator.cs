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
using System.Reflection;
using System.Runtime.Loader;
using Demo3D.Common;
using Microsoft.AspNetCore.Components;

namespace Demo3D.Components;

/// <summary>
/// Intercepts Blazor component creation to detect and remap component types from stale
/// assemblies to the current assembly. This fixes DI failures caused by Emulate3D loading
/// each recompilation into a separate AssemblyLoadContext via Assembly.LoadFile, leaving
/// old assemblies in memory with incompatible types.
/// </summary>
public sealed class AssemblyRemappingComponentActivator : IComponentActivator
{
    private readonly Assembly currentAssembly = typeof(AssemblyRemappingComponentActivator).Assembly;
    private readonly string currentAssemblyName = typeof(AssemblyRemappingComponentActivator).Assembly.GetName().Name!;

    public IComponent CreateInstance(Type componentType)
    {
        var effectiveType = componentType;

        if (componentType.Assembly != currentAssembly
            && string.Equals(componentType.Assembly.GetName().Name, currentAssemblyName, StringComparison.Ordinal))
        {
            // Component type is from a stale assembly — remap to the current one
            var remappedType = currentAssembly.GetType(componentType.FullName!);
            if (remappedType != null)
            {
                var staleContext = AssemblyLoadContext.GetLoadContext(componentType.Assembly)?.Name ?? "unknown";
                var currentContext = AssemblyLoadContext.GetLoadContext(currentAssembly)?.Name ?? "unknown";
                Logger.Log("Warning",
                    $"[Activator] Remapped {componentType.FullName} from stale assembly " +
                    $"({componentType.Assembly.Location}, ALC={staleContext}) to current assembly " +
                    $"({currentAssembly.Location}, ALC={currentContext}).");
                effectiveType = remappedType;
            }
            else
            {
                Logger.Log("Error",
                    $"[Activator] Stale type {componentType.FullName} from {componentType.Assembly.Location} " +
                    $"has no equivalent in the current assembly. Component creation may fail.");
            }
        }

        return (IComponent)Activator.CreateInstance(effectiveType)!;
    }
}
