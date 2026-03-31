using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Demo3D.Native;
using Demo3D.Raw;
using Demo3D.Visuals;

[Auto]
public class CardboardBoxWithComponentsScript : NativeObject
{
    public CardboardBoxWithComponentsScript(Visual sender) : base(sender) { }


    [Auto]
    void OnHasC1Updated(Visual sender, bool newValue, bool oldValue)
    {
        UpdateComponent("C1", newValue);
    }

    [Auto]
    void OnHasC2Updated(Visual sender, bool newValue, bool oldValue)
    {
        UpdateComponent("C2", newValue);
    }

    private void UpdateComponent(string componentName, bool isVisible)
    {
        if (Visual.FindChild(componentName) is MeshObject component)
        {
            component.Visible = isVisible;
        }
        else
        {
            app.LogMessage("Error", $"Could not find {componentName} as a child.", Visual);
        }
    }
}