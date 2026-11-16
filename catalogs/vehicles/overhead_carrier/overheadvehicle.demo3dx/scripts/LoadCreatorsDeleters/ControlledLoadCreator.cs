using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Demo3D.Native;
using Demo3D.Visuals;
using Demo3D.PLC.Comms;
using System.Linq;
using Microsoft.DirectX;
using System.Reflection;

// Created by Mark Emeott
// Updated June 17, 2025

namespace Demo3D.Components.LoadCreatorsDeleters {
    [Auto] public class ControlledLoadCreator : NativeObject {
        public ControlledLoadCreator(Visual sender) : base(sender) { Setup(sender); }

        #region Visuals
        [Auto] BoxVisual CongestionZone;
        #endregion

        #region Custom Properties
        public enum LoadCreationModes { CreateNow, QueueOrder }
        [Auto, Category("-About"), ReadOnly(true), Description("Version")]
        SimplePropertyValue<String> Version;
        [Auto, Category("-Congestion"), DefaultValue(true), Description("Congestion Zone Visible")]
        SimplePropertyValue<Boolean> CongestionZoneVisible;
        [Auto, Category("-Config"), DefaultValue(LoadCreationModes.QueueOrder), Description("Creation Mode")]
        SimplePropertyValue<LoadCreationModes> LoadCreationMode;
        [Auto, Category("-Congestion"), Description("Congestion Zone Size")]
        SimplePropertyValue<Distance3Property> CongestionZoneSize;
        [Auto, Category("-Congestion"), Description("Congestion Zone Relative Location")]
        SimplePropertyValue<Distance3Property> CongestionZoneLocation;
        [Auto, Category("-Congestion"), Description("Congestion Zone Relative Rotation")]
        SimplePropertyValue<Angle3Property> CongestionZoneRotation;
        [Auto, Category("-Control"), Description("Create Load when value changes to True"), IsBindingInterface, AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<Boolean> CreateLoad;
        [Auto, Category("-Data"), ReadOnly(true), Description("Count of failed load creation attempts"), AccessRights(AccessRights.WriteToPLC)]
        SimplePropertyValue<Int32> FailedCreations;
        [Auto, Category("-Data"), ReadOnly(true), Description("Count of queued load creation orders"), AccessRights(AccessRights.WriteToPLC)]
        SimplePropertyValue<Int32> QueuedOrders;
        [Auto, Category("-Data"), ReadOnly(true), Description("Loads Created")]
        SimplePropertyValue<int> LoadsCreated;
        #endregion

        #region Events
        [Auto] void OnVisualAdded(Visual sender) { UpdateCongestionZone(sender); }
        [Auto] void OnLoaded(Visual sender) { Setup(sender); }
        [Auto] void OnReset(Visual sender) { Setup(sender); }
        [Auto] void OnInitialize(Visual sender) { Setup(sender); }

        private void Setup(Visual sender) {
            // update version
            Version.Value = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            // reset custom properties
            LoadsCreated.Value = 0;
            // disable load creator's default auto-release behavior
            (sender as LoadCreatorVisual).ReleaseEnabled = false;
            // reset custom properties
            FailedCreations.Value = 0;
            QueuedOrders.Value = 0;
            // setup listeners for change in congestion zone's state (i.e. onblocked & oncleared events)
            CongestionZone.OnBlocked.NativeListeners -= CongestionZone_State;
            CongestionZone.OnBlocked.NativeListeners += CongestionZone_State;
            CongestionZone.OnCleared.NativeListeners -= CongestionZone_State;
            CongestionZone.OnCleared.NativeListeners += CongestionZone_State;
            // update color
            UpdateColor(sender);
        }

        [Auto] void OnLoadCreationModeUpdated(Visual sender, LoadCreationModes value, LoadCreationModes oldValue) {
            // show appropriate data properties
            if (LoadCreationMode.Value == LoadCreationModes.CreateNow) {
                FailedCreations.Hidden = false;
                QueuedOrders.Hidden = true;
            }
            else if (LoadCreationMode.Value == LoadCreationModes.QueueOrder) {
                FailedCreations.Hidden = true;
                QueuedOrders.Hidden = false;
            }
            RefreshPropertiesGrid();
        }

        [Auto] void OnAfterPropertyUpdated(Visual sender, String name) {
            if (name == "CongestionZone") {
                UpdateCongestionZone(sender);
            }
        }

        private void CongestionZone_State(PhysicsObject congestionZone, Visual load) {
            // only execute if CongestionZone is enabled
            var lc = Visual as LoadCreatorVisual;
            if (lc.CongestionZone) {
                UpdateColor(Visual);
            }
        }

        [Auto] void OnCongestionZoneVisibleUpdated(Visual sender, Boolean value, Boolean oldValue) { UpdateCongestionZone(sender); }
        [Auto] void OnCongestionZoneSizeUpdated(Visual sender, Distance3Property value, Distance3Property oldValue) { UpdateCongestionZone(sender); }
        [Auto] void OnCongestionZoneLocationUpdated(Visual sender, Distance3Property value, Distance3Property oldValue) { UpdateCongestionZone(sender); }
        [Auto] void OnCongestionZoneRotationUpdated(Visual sender, Angle3Property value, Angle3Property oldValue) { UpdateCongestionZone(sender); }

        private void UpdateCongestionZone(Visual sender) {
            var lc = sender as LoadCreatorVisual;
            // update congestion zone size
            CongestionZone.Depth = CongestionZoneSize.Value.Z;
            CongestionZone.Height = CongestionZoneSize.Value.Y;
            CongestionZone.Width = CongestionZoneSize.Value.X;
            // update congestion zone location
            var location = new Vector3();
            location.X = CongestionZoneLocation.Value.X;
            location.Y = -0.1f + (float)CongestionZone.Height / 2 + CongestionZoneLocation.Value.Y;
            location.Z = CongestionZoneLocation.Value.Z;
            CongestionZone.Location = location;
            // update congestion zone rotation
            CongestionZone.RotationDegrees = CongestionZoneRotation.Value;
            // update congestion zone visibility
            CongestionZone.Visible = lc.CongestionZone ? CongestionZoneVisible.Value : false;
            // show/hide congestion zone properties
            CongestionZoneSize.Hidden = !lc.CongestionZone;
            CongestionZoneLocation.Hidden = !lc.CongestionZone;
            CongestionZoneRotation.Hidden = !lc.CongestionZone;
            CongestionZoneVisible.Hidden = !lc.CongestionZone;
            RefreshPropertiesGrid();
        }

        [Auto] IEnumerable OnCreateLoadUpdated(Visual sender, Boolean value, Boolean oldValue) {
            var lc = sender as LoadCreatorVisual;
            if (CreateLoad.Value) {
                Visual load = null;
                if (LoadCreationMode.Value == LoadCreationModes.QueueOrder) {
                    // order load (will queue if congestion zone blocked, release when congestion zone unblocked)
                    var order = lc.OrderLoad();
                    // queue order
                    QueuedOrders.Value++;
                    UpdateColor(sender);
                    yield return order;
                    // order fulfilled
                    LoadsCreated.Value++;
                    load = order.Value;
                    load.Unstick();
                    UpdateLoad(sender, load);
                    // decrement queued orders
                    QueuedOrders.Value--;
                    UpdateColor(sender);
                }
                else if (LoadCreationMode.Value == LoadCreationModes.CreateNow) {
                    // attempt to create load now (will fail if congestion zone enabled & blocked)
                    load = lc.CreateLoadNow();
                    if (load != null) {
                        // creation succeeded
                        LoadsCreated.Value++;
                        UpdateLoad(sender, load);
                    }
                    else {
                        // increment failed orders
                        FailedCreations.Value++;
                        lc.FadeToColor(Color.Red, 0.0);
                        CongestionZone.FadeToColor(Color.Red, 0.0);
                    }
                }
            }
        }

        private void UpdateLoad(Visual sender, Visual load) {
            var lc = sender as LoadCreatorVisual;
            var conveyorVisual = sender.Parent as ConveyorVisual;
            var conveyorAspect = sender.FindAspect<AttachToConveyorAspect>().AttachedAspect as ConveyorAspect;
            if (conveyorVisual != null) {
                if (lc.InitialConveyorVelocity) {
                    // accelerate load to conveyor velocity
                    var directionSign = conveyorVisual.IsMotorForwards ? 1 : -1;
                    var speedSign = conveyorVisual.MotorSpeed >= 0 ? 1 : -1;
                    load.Velocity = conveyorVisual.TransformToWorldNormal(vector(conveyorVisual.Motor.CurrentSpeed * directionSign * speedSign, 0, 0));
                    return;
                }
            }
            else if (conveyorAspect != null) {
                // Move load -0.1 m in load creator's y-axis (because it's created 0.1 m too high)
                var creationLocation = lc.TransformFromWorld(load.WorldLocation) - new Vector3(0.0, 0.1, 0.0);
                load.WorldLocation = lc.TransformToWorld(creationLocation);
                if (lc.InitialConveyorVelocity) {
                    // accelerate load to conveyor velocity (if enabled)
                    var directionSign = conveyorAspect.Motor.Direction == MotorDirection.Forwards ? 1 : -1;
                    var speedSign = conveyorAspect.Motor.CurrentSpeed >= 0 ? 1 : -1;
                    load.Velocity = conveyorAspect.Visual.TransformToWorldNormal(vector(conveyorAspect.Motor.CurrentSpeed * directionSign * speedSign, 0, 0));

                }
            }
        }

        private void RefreshPropertiesGrid() {
            // refresh properties grid (if visual is selected)
            if (app.Selection.Contains(Visual)) {
                app.RefreshPropertiesGrid();
            }
        }

        private void UpdateColor(Visual sender) {
            // update load creator & congestion zone color
            var lc = Visual as LoadCreatorVisual;
            if (CongestionZone.IsBlocked) {
                // congestion zone blocked
                if (LoadCreationMode.Value == LoadCreationModes.QueueOrder && QueuedOrders.Value > 0) {
                    // queuing orders
                    lc.FadeToColor(Color.Purple, 0.0);
                    CongestionZone.FadeToColor(Color.Purple, 0.0);
                }
                else {
                    // not queuing orders
                    lc.FadeToColor(Color.Orange, 0.0);
                    CongestionZone.FadeToColor(Color.Orange, 0.0);
                }
            }
            else {
                // congestion zone clear
                lc.FadeToColor(Color.Green, 0.0);
                CongestionZone.FadeToColor(Color.Green, 0.0);
            }
        }
        #endregion
    }
}