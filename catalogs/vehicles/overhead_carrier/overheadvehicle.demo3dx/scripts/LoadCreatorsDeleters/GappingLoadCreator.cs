using System;
using System.Collections.Generic;
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
    [Auto] public class GappingLoadCreator : NativeObject {
        public GappingLoadCreator(Visual sender) : base(sender) { Setup(sender); }

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
        [Auto, Category("-Control"), DefaultValue(0.1), Description("Gap Distance"), AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<DistanceProperty> GapDistance;
        [Auto, Category("-Data"), ReadOnly(true), Description("Loads Created")]
        SimplePropertyValue<int> LoadsCreated;
        #endregion

        #region Events
        [Auto] void OnVisualAdded(Visual sender) { UpdateCongestionZone(sender); }
        [Auto] void OnLoaded(Visual sender) { Setup(sender); }
        [Auto] void OnReset(Visual sender) { Setup(sender); }

        [Auto] void OnInitialize(Visual sender) {
            Setup(sender);
            // create 1st load
            CreateLoad(sender, null);
        }

        private void Setup(Visual sender) {
            // update version
            Version.Value = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            // reset custom properties
            LoadsCreated.Value = 0;
            // disable load creator's default auto-release behavior & enable its congestion zone
            var lc = sender as LoadCreatorVisual;
            lc.ReleaseEnabled = false;
            lc.CongestionZone = true;
            // setup listeners for change in congestion zone (i.e. onblocked & oncleared events)
            CongestionZone.OnBlocked.NativeListeners -= CongestionZone_OnBlocked;
            CongestionZone.OnBlocked.NativeListeners += CongestionZone_OnBlocked;
            CongestionZone.OnCleared.NativeListeners -= CongestionZone_OnCleared;
            CongestionZone.OnCleared.NativeListeners += CongestionZone_OnCleared;
            // update color
            UpdateColor(sender);
        }

        [Auto] void OnLoadCreatorEnabledUpdated(Visual sender, Boolean value, Boolean oldValue) {
            if (LoadCreatorEnabled.Value) {
                // create 1st load
                CreateLoad(sender, null);
            }
            // update color
            UpdateColor(Visual);
        }

        private void CongestionZone_OnBlocked(PhysicsObject congestionZone, Visual load) {
            // update color
            UpdateColor(Visual);
        }

        private void CongestionZone_OnCleared(PhysicsObject congestionZone, Visual load) {
            if (LoadCreatorEnabled.Value) {
                // create load
                CreateLoad(Visual, load);
            }
        }

        [Auto] void OnCongestionZoneVisibleUpdated(Visual sender, Boolean value, Boolean oldValue) { UpdateCongestionZone(sender); }
        [Auto] void OnCongestionZoneSizeUpdated(Visual sender, Distance3Property value, Distance3Property oldValue) { UpdateCongestionZone(sender); }
        [Auto] void OnCongestionZoneLocationUpdated(Visual sender, Distance3Property value, Distance3Property oldValue) { UpdateCongestionZone(sender); }
        [Auto] void OnCongestionZoneRotationUpdated(Visual sender, Angle3Property value, Angle3Property oldValue) { UpdateCongestionZone(sender); }

        private void UpdateCongestionZone(Visual sender) {
            var lc = sender as LoadCreatorVisual;
            lc.CongestionZone = true;
            // update congestion zone size
            CongestionZoneSize.Value.X = (float)GapDistance.Value;
            CongestionZone.Depth = CongestionZoneSize.Value.Z;
            CongestionZone.Height = CongestionZoneSize.Value.Y;
            CongestionZone.Width = GapDistance.Value;
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
            RefreshPropertiesGrid();
        }

        [Auto] void OnGapDistanceUpdated(Visual sender, DistanceProperty value, DistanceProperty oldValue) {
            // error check release distance (minimum value 0.1 mm)
            if (GapDistance.Value < 0.0001) {
                GapDistance.Value = 0.0001;
            }
            // update congestion zone's x size
            CongestionZoneSize.Value.X = (float)GapDistance.Value;
            RefreshPropertiesGrid();
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

        private void CreateLoad(Visual sender, Visual clearedLoad) {
            var lc = sender as LoadCreatorVisual;
            // only create a new load if load creator enabled & congestion zone not blocked
            if (LoadCreatorEnabled.Value && !CongestionZone.IsBlocked) {
                var load = lc.CreateLoadNow();
                if (load != null) {
                    LoadsCreated.Value++;
                    var conveyorVisual = sender.Parent as ConveyorVisual;
                    var straightConveyorAspect = sender.FindAspect<AttachToConveyorAspect>().AttachedAspect as StraightConveyorAspect;
                    if (conveyorVisual != null) {
                        // conveyor visual
                        if (clearedLoad != null) {
                            // move load backwards or forwards from cleared load along conveyor's x-axis with the specified gap
                            PositionLoad(lc, load, clearedLoad, conveyorVisual, GapDistance.Value);
                        }
                        else {
                            // move load to just before congestion zone sensor
                            PositionLoad(lc, load, CongestionZone, conveyorVisual, 0.0);
                        }
                    }
                    else if (straightConveyorAspect != null) {
                        // straight conveyor aspect
                        if (clearedLoad != null) {
                            // move load backwards or forwards from cleared load along conveyor's x-axis with the specified gap
                            PositionLoad(lc, load, clearedLoad, straightConveyorAspect, GapDistance.Value);
                        }
                        else {
                            // move load to just before congestion zone sensor
                            PositionLoad(lc, load, CongestionZone, straightConveyorAspect, 0.0);
                        }
                    }
                }
            }
            UpdateColor(sender);
        }

        private static void PositionLoad(LoadCreatorVisual lc, Visual load, Visual clearedLoad, ConveyorVisual conveyor, double gap) {
            // determine conveyor direction
            var conveyorDirection = (int)conveyor.Motor.Direction * (conveyor.Motor.CurrentSpeed >= 0.0 ? 1 : -1);
            // cleared load local location
            var clearedLoadLocalBBox = LocalBBox(lc, clearedLoad);
            var clearedLoadLocalLocation = lc.TransformFromWorld(clearedLoad.WorldLocation);
            var clearedLoadLocalTrailingPoint = clearedLoadLocalLocation.XD - (conveyorDirection * clearedLoadLocalBBox.LengthX / 2);
            // load local location
            var loadLocalBBox = LocalBBox(lc, load);
            var loadLocalLocation = lc.TransformFromWorld(load.WorldLocation);
            var loadLocalLeadingPoint = loadLocalLocation.XD + (conveyorDirection * loadLocalBBox.LengthX / 2);
            // current gap
            var currentGap = clearedLoadLocalTrailingPoint - loadLocalLeadingPoint;
            var diff = currentGap - (conveyorDirection * gap);
            // adjust load location
            var adjustedLocalLocation = loadLocalLocation + new Vector3(diff, 0, 0);
            load.WorldLocation = lc.TransformToWorld(adjustedLocalLocation);
        }

        private void PositionLoad(LoadCreatorVisual lc, Visual load, Visual clearedLoad, StraightConveyorAspect straightConveyorAspect, double separation) {
            // determine conveyor direction
            var conveyorDirection = (int)straightConveyorAspect.Motor.Direction * (straightConveyorAspect.Motor.CurrentSpeed >= 0.0 ? 1 : -1); ;
            // cleared load local location
            var clearedLoadLocalBBox = LocalBBox(lc, clearedLoad);
            var clearedLoadLocalLocation = lc.TransformFromWorld(clearedLoad.WorldLocation);
            var clearedLoadLocalTrailingPoint = clearedLoadLocalLocation.XD - (conveyorDirection * clearedLoadLocalBBox.LengthX / 2);
            // load local location
            var loadLocalBBox = LocalBBox(lc, load);
            var loadLocalLocation = lc.TransformFromWorld(load.WorldLocation);
            var loadLocalLeadingPoint = loadLocalLocation.XD + (conveyorDirection * loadLocalBBox.LengthX / 2);
            // current gap
            var currentGap = clearedLoadLocalTrailingPoint - loadLocalLeadingPoint;
            var diff = currentGap - (conveyorDirection * separation);
            // adjust load location (-0.1 m in load creator's y-axis because it's created 0.1 m too high) 
            var adjustedLocalLocation = loadLocalLocation + new Vector3(diff, -0.1, 0);
            load.WorldLocation = lc.TransformToWorld(adjustedLocalLocation);
            // update load velocity
            var conveyorAspectWorldXAxis = Vector3.Normalize(straightConveyorAspect.TrailingEdge.WorldMidPoint - straightConveyorAspect.LeadingEdge.WorldMidPoint);
            var conveyorDirectionVector = conveyorAspectWorldXAxis * (int)straightConveyorAspect.Motor.Direction;
            load.Velocity = conveyorDirectionVector * straightConveyorAspect.Motor.CurrentSpeed;
        }

        private static BBox LocalBBox(Visual referenceVisual, Visual visual) {
            var min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            var max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            foreach (var mo in visual.VisualAndDescendants.OfType<MeshObject>()) {
                var matrix = mo.WorldRotationMatrix;
                if (referenceVisual != null) {
                    matrix *= referenceVisual.WorldRotationMatrix.Inverted;
                }
                foreach (var pos in GetVertexPositions(mo.Mesh)) {
                    var transformed = matrix.TransformCoordinate(pos);
                    min = Vector3.Minimize(min, transformed);
                    max = Vector3.Maximize(max, transformed);
                }
            }
            var localBBox = new BBox(min, max);
            return localBBox;
        }

        private static IEnumerable<Vector3> GetVertexPositions(IMesh mesh) {
            var meshData = mesh.GetMeshData();
            using var it = meshData.VertexData.GetIterator();
            foreach (var vertex in it) {
                yield return vertex.Position;
            }
        }
        #endregion
    }
}
