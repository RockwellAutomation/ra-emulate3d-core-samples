using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Demo3D.Native;
using Demo3D.Visuals;
using Microsoft.DirectX;

// Created by Mark Emeott
// Updated June 17, 2024

namespace Demo3D.Components.LoadCreatorsDeleters {
    [Auto] public class LoadCreator : NativeObject {
        public LoadCreator(Visual sender) : base(sender) { Setup(sender); }

        #region Visuals
        [Auto] BoxVisual CongestionZone;
        #endregion

        #region Custom Properties
        public enum LoadCreationModes { CreateNow, QueueOrder }
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
        }

        [Auto] void OnAfterPropertyUpdated(Visual sender, String name) {
            if (name == "CongestionZone") {
                UpdateCongestionZone(sender);
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

        [Auto] void OnLoadCreated(Visual sender, Visual load) {
            LoadsCreated.Value++;
            var lc = sender as LoadCreatorVisual;
            var conveyorAspect = sender.FindAspect<AttachToConveyorAspect>().AttachedAspect as ConveyorAspect;
            if (conveyorAspect != null) {
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
        #endregion
    }
}