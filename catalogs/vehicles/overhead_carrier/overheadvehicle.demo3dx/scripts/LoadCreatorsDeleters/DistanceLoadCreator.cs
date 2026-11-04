using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Demo3D.Native;
using Demo3D.PLC.Comms;
using Demo3D.Utilities;
using Demo3D.Visuals;
using Microsoft.DirectX;

// Created by Mark Emeott
// Updated June 17, 2025

namespace Demo3D.Components.LoadCreatorsDeleters {
    [Auto] public class DistanceLoadCreator : NativeObject {
        public DistanceLoadCreator(Visual sender) : base(sender) { Setup(sender); }

        #region Globals
        private Boolean started = false;
        #endregion

        #region Visuals
        [Auto] BoxVisual CongestionZone;
        #endregion

        #region Custom Properties
        [Auto, Category("-About"), ReadOnly(true), Description("Version")]
        SimplePropertyValue<String> Version;
        [Auto, Category("-Congestion"), DefaultValue(true), Description("Congestion Zone Visible")]
        SimplePropertyValue<Boolean> CongestionZoneVisible;
        [Auto, Category("-Congestion"), Description("Congestion Zone Size")]
        SimplePropertyValue<Distance3Property> CongestionZoneSize;
        [Auto, Category("-Congestion"), Description("Congestion Zone Relative Location")]
        SimplePropertyValue<Distance3Property> CongestionZoneLocation;
        [Auto, Category("-Congestion"), Description("Congestion Zone Relative Rotation")]
        SimplePropertyValue<Angle3Property> CongestionZoneRotation;
        [Auto, Category("-Control"), DefaultValue(true), Description("Load Creator Enabled"), AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<Boolean> LoadCreatorEnabled;
        [Auto, Category("-Control"), DefaultValue(0.5), Description("Release Distance"), AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<DistanceProperty> ReleaseDistance;
        [Auto, Category("-Data"), ReadOnly(true), Description("Count of failed load creation attempts"), AccessRights(AccessRights.WriteToPLC)]
        SimplePropertyValue<Int32> FailedCreations;
        [Auto, Category("-Data"), ReadOnly(true), Description("Loads Created")]
        SimplePropertyValue<int> LoadsCreated;
        #endregion

        #region Events
        [Auto] void OnVisualAdded(Visual sender) { UpdateCongestionZone(sender); }
        [Auto] void OnLoaded(Visual sender) { Setup(sender); }
        [Auto] void OnReset(Visual sender) { Setup(sender); }

        [Auto] void OnInitialize(Visual sender) {
            Setup(sender);
            // initialize values
            started = false;
            // start release logic
            StartReleaseLogic(sender);
        }

        private void Setup(Visual sender) {
            // update version
            Version.Value = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            // reset custom properties
            LoadsCreated.Value = 0;
            // disable load creator's default auto-release behavior
            (sender as LoadCreatorVisual).ReleaseEnabled = false;
            // reset custom properties
            FailedCreations.Value = 0;
            // setup listeners for change in congestion zone's state (i.e. onblocked & oncleared events)
            var lc = sender as LoadCreatorVisual;
            if (lc.CongestionZone) {
                // setup congestion zone listeners
                CongestionZone.OnBlocked.NativeListeners -= CongestionZone_State;
                CongestionZone.OnBlocked.NativeListeners += CongestionZone_State;
                CongestionZone.OnCleared.NativeListeners -= CongestionZone_State;
                CongestionZone.OnCleared.NativeListeners += CongestionZone_State;
            }
            else {
                // remove congestion zone listeners
                CongestionZone.OnBlocked.NativeListeners -= CongestionZone_State;
                CongestionZone.OnCleared.NativeListeners -= CongestionZone_State;
            }
            // update color
            UpdateColor(sender);
        }

        [Auto] void OnLoadCreatorEnabledUpdated(Visual sender, Boolean value, Boolean oldValue) { StartReleaseLogic(sender); }
        [Auto] void OnParentUpdated(Visual sender, Visual oldParent, Visual newParent) { StartReleaseLogic(sender);  }

        private void StartReleaseLogic(Visual sender) {
            // determine if load creator enabled & not already started
            var lc = sender as LoadCreatorVisual;
            if (LoadCreatorEnabled.Value && !started) {
                // start release logic
                document.Run(() => ReleaseLogic(sender));
            }
            UpdateColor(sender);
        }

        [Auto] void OnAfterPropertyUpdated(Visual sender, String name) {
            if (name == "CongestionZone") {
                Setup(sender);
                UpdateCongestionZone(sender);
            }
        }

        private void CongestionZone_State(PhysicsObject congestionZone, Visual load) {
            // only execute if CongestionZone is enabled
            var lc = Visual as LoadCreatorVisual;
            //print("congestion zone state changed");
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
            // update congestion location
            var location = new Vector3();
            location.X = CongestionZoneLocation.Value.X;
            location.Y = -0.1f + (float)CongestionZone.Height / 2 + CongestionZoneLocation.Value.Y;
            location.Z = CongestionZoneLocation.Value.Z;
            CongestionZone.Location = location;
            // update congestion rotation
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

        [Auto] void OnReleaseDistanceUpdated(Visual sender, DistanceProperty value, DistanceProperty oldValue) {
            // error check release distance (minimum value 1 mm)
            if (ReleaseDistance.Value < 0.001) {
                ReleaseDistance.Value = 0.001;
            }
        }

        private IEnumerable ReleaseLogic(Visual sender) {
            // latch started
            started = true;
            // create loads while load creator enabled
            while (LoadCreatorEnabled.Value && (sender.Parent is ConveyorVisual || sender.Parent.HasAspect<ConveyorAspect>())) {
                var coreConveyor = sender.Parent as ConveyorVisual;
                var motorAspect = sender.Parent.FindAspect<Visuals.Motor.MotorAspect>();
                var conveyorMotorAspect = sender.Parent.FindAspect<ConveyorMotorAspect>();
                if (coreConveyor != null && (motorAspect == null || (motorAspect != null && !motorAspect.OverrideCoreConveyorMotor))) {
                    var directionSign = coreConveyor.IsMotorForwards ? 1 : -1;
                    var speedSign = coreConveyor.MotorSpeed >= 0 ? 1 : -1;
                    CreateLoad(sender, directionSign, speedSign);
                    // wait for conveyor to travel specified release distance
                    var distance = coreConveyor.Motor.DistanceTravelled + ReleaseDistance.Value * directionSign * speedSign;
                    yield return coreConveyor.AddDistanceNotifier(distance);
                }
                else if (motorAspect != null) {
                    var directionSign = motorAspect.Direction == MotorDirection.Forwards ? 1 : -1;
                    var speedSign = motorAspect.CurrentSpeed >= 0 ? 1 : -1;
                    CreateLoad(sender, directionSign, speedSign);
                    // wait for motorAspect to travel specified release distance
                    var distance = motorAspect.DistanceTravelled + ReleaseDistance.Value * directionSign * speedSign;
                    yield return motorAspect.AddDistanceNotifier(distance, sender.Parent);
                }
                else if (conveyorMotorAspect != null) {
                    var directionSign = conveyorMotorAspect.Direction == MotorDirection.Forwards ? 1 : -1;
                    var speedSign = conveyorMotorAspect.CurrentSpeed >= 0 ? 1 : -1;
                    CreateLoad(sender, directionSign, speedSign);
                    // wait for conveyorMotorAspect to travel specified release distance
                    var distance = conveyorMotorAspect.DistanceTravelled + ReleaseDistance.Value * directionSign * speedSign;
                    yield return conveyorMotorAspect.AddDistanceNotifier(distance, sender.Parent);
                }
            }
            // unlatch started
            started = false;
        }

        private void CreateLoad(Visual sender, int directionSign, int speedSign) {
            var lc = sender as LoadCreatorVisual;
            // attempt to create load now (will fail if congestion zone enabled & blocked)
            var load = lc.CreateLoadNow();
            if (load != null) {
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

        private void UpdateLoad(Visual sender, Visual load) {
            var lc = sender as LoadCreatorVisual;
            var conveyorVisual = sender.Parent as ConveyorVisual;
            var conveyorAspect = sender.FindAspect<AttachToConveyorAspect>().AttachedAspect as ConveyorAspect;
            if (conveyorVisual != null) {
                var motorAspect = sender.Parent.FindAspect<Visuals.Motor.MotorAspect>();
                if (motorAspect == null || (motorAspect != null && !motorAspect.OverrideCoreConveyorMotor)) {
                    if (lc.InitialConveyorVelocity) {
                        // accelerate load to conveyor velocity (conveyor visual)
                        var directionSign = conveyorVisual.IsMotorForwards ? 1 : -1;
                        var speedSign = conveyorVisual.MotorSpeed >= 0 ? 1 : -1;
                        load.Velocity = conveyorVisual.TransformToWorldNormal(vector(conveyorVisual.Motor.CurrentSpeed * directionSign * speedSign, 0, 0));
                        return;
                    }
                }
                else {
                    if (lc.InitialConveyorVelocity) {
                        // accelerate load to conveyor velocity (motor aspect)
                        var directionSign = motorAspect.Direction == MotorDirection.Forwards ? 1 : -1;
                        var speedSign = motorAspect.CurrentSpeed >= 0 ? 1 : -1;
                        load.Velocity = conveyorVisual.TransformToWorldNormal(vector(motorAspect.CurrentSpeed * directionSign * speedSign, 0, 0));
                        return;
                    }
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
            if (!LoadCreatorEnabled.Value) {
                // load creator disabled
                lc.FadeToColor(Color.DimGray, 0.0);
                CongestionZone.FadeToColor(Color.DimGray, 0.0);
            }
            else if (CongestionZone.IsBlocked) {
                // congestion zone blocked
                lc.FadeToColor(Color.Orange, 0.0);
                CongestionZone.FadeToColor(Color.Orange, 0.0);
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
