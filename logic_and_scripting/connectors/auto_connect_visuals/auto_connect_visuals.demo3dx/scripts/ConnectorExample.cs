using Demo3D.Native;
using Demo3D.Visuals;
using Demo3D.Visuals.Renderers.Mesh;
using System.ComponentModel;
using System.Drawing;

[Auto] public class ConnectorExample : NativeObject {
    public ConnectorExample(Visual sender) : base(sender) {}

    [Auto, Category("-Custom"), Description("Toggle snapping behavior.")]
    SimplePropertyValue<bool> ConnectorEnabled;

    [Auto] void OnConnectorEnabledUpdated(Visual sender, bool value, bool oldValue) { UpdateConnector(sender); }
    [Auto] void OnReset(Visual sender) { UpdateConnector(sender); }

    void UpdateConnector(Visual sender) {
        Connector c = sender.FindCreateConnector("ConnectorExample");
        c.Start = c.End = Visual.LeftFace.Point;
        c.Normal = Visual.LeftFace.Normal;
        c.ReparentOnConnect = true;
        c.ControlPointEnabled = true;
        c.ControlPointSize = 0.10;
        c.AlignmentStyle = ConnectorAlignmentStyle.Horizontal;
        c.AutoConnect = true;

        // Use below command to remove the connector.
        // sender.RemoveConnector("InsertConnectorNameHere");
    }

    [Auto] void OnChildAdded(Visual sender, Visual child) {
        UpdateChildColor(child, Color.LightGreen);
        print($"Connection made between {child} and {sender}!");

    }

    [Auto] void OnChildRemoved(Visual sender, Visual child) {
        UpdateChildColor(child, Color.White);
        print($"Connection removed.");
    }

    private void UpdateChildColor(Visual child, Color color) {
        if (child is CoreVisual) {
            // fade to color deep sets the color of the child and all its children
            child.FadeToColorDeep(color, 0.0);
        }
        else {
            // look for mesh renderer aspects (since imported CAD is exclusively made up of these)
            var meshRenderers = child.FindAspects<MeshRendererAspect>();
            foreach (var meshRenderer in meshRenderers) {
                foreach (var material in meshRenderer.Materials) {
                    material.Color = color;
                }
            }
        }
    }
}