using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Demo3D.Common;
using Demo3D.Native;
using Demo3D.PLC.Comms;
using Demo3D.Visuals;
using Microsoft.DirectX;

// Created by Mark Emeott
// Updated June 17, 2025

namespace Demo3D.Components.LoadCreatorsDeleters {
    [Auto] public class ControlledLoadDeleter : NativeObject {
        public ControlledLoadDeleter(BoxVisual sender) : base(sender) { Setup(sender); }

        #region Visuals
        [Auto] BoxVisual DeletionZone;
        #endregion

        #region Custom Properties
        [Auto, Category("-About"), ReadOnly(true), Description("Version")]
        SimplePropertyValue<String> Version;
        [Auto, Category("-Config"), Description("Accepted Loads for Deletion")]
        SimplePropertyValue<LoadFilter> AcceptedLoads;
        [Auto, Category("-Config"), Description("Ignored Loads for Deletion")]
        SimplePropertyValue<LoadFilter> IgnoredLoads;
        [Auto, Category("-Control"), Description("Delete Loads when value changes to True (using Deletion Zone if Enabled)"), AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<Boolean> DeleteLoads;
        [Auto, Category("-Control"), DefaultValue(true), Description("Deletion Enabled"), AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<Boolean> LoadDeleterEnabled;
        [Auto, Category("-Control"), DefaultValue(false), Description("Enable Deletion Zone"), AccessRights(AccessRights.ReadFromPLC)]
        SimplePropertyValue<Boolean> DeletionZoneEnabled;
        [Auto, Category("-Deletion Zone"), DefaultValue(true), Description("Deletion Zone Visible")]
        SimplePropertyValue<Boolean> DeletionZoneVisible;
        [Auto, Category("-Deletion Zone"), Description("Deletion Zone Size")]
        SimplePropertyValue<Distance3Property> DeletionZoneSize;
        [Auto, Category("-Deletion Zone"), Description("Deletion Zone Relative Location")]
        SimplePropertyValue<Distance3Property> DeletionZoneLocation;
        [Auto, Category("-Deletion Zone"), Description("Deletion Zone Relative Rotation")]
        SimplePropertyValue<Angle3Property> DeletionZoneRotation;
        [Auto, Category("-Data"), ReadOnly(true), Description("Loads Deleted")]
        SimplePropertyValue<int> LoadsDeleted;
        #endregion

        #region Events
        [Auto] void OnVisualAdded(Visual sender) { UpdateDeletionZone(sender); }
        [Auto] void OnLoaded(Visual sender) { Setup(sender); }
        [Auto] void OnReset(Visual sender) { Setup(sender); }
        [Auto] void OnInitialize(Visual sender) { Setup(sender); }

        private void Setup(Visual sender) {
            // update version
            Version.Value = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            // reset custom properties
            LoadsDeleted.Value = 0;
        }

        [Auto] void OnLoadDeleterEnabledUpdated(Visual sender, Boolean value, Boolean oldValue) {
            // update load deleter color
            (sender as BoxVisual).Material.Color = LoadDeleterEnabled.Value ? Color.OrangeRed : Color.DimGray;
            DeletionZone.Material.Color = LoadDeleterEnabled.Value ? Color.OrangeRed : Color.DimGray;
        }

        [Auto] void OnAfterPropertyUpdated(Visual sender, String name) {
            if (name == "Height" || name == "Depth" || name == "Width") {
                var locationY = (sender as BoxVisual).Height / 2;
                // update trashcan image location
                var trashcan = sender.FindFirstOfType(typeof(ImportedImageVisual)) as ImportedImageVisual;
                trashcan.LocationY = locationY + 0.001;
                var box = sender as BoxVisual;
                var minDimension = Math.Min(box.Depth, box.Width);
                var imageSize = Math.Min(0.2, minDimension);
                trashcan.Height = imageSize;
                trashcan.PropertiesUpdated("Height");
                // update deleter location & attachment height to be flush with conveyor surface
                if (sender.Parent is ConveyorVisual || sender.Parent.HasAspect<ConveyorAspect>()) {
                    sender.LocationY = locationY;
                }
                var attachToConveyorAspect = sender.FindAspect<AttachToConveyorAspect>();
                if (attachToConveyorAspect != null) {
                    attachToConveyorAspect.AttachDistanceOffset.Y = (float)locationY;
                }
                // update deletion zone location
                UpdateDeletionZone(sender);
            }
        }

        [Auto] void OnDeletionZoneEnabledUpdated(Visual sender, Boolean value, Boolean oldValue) { UpdateDeletionZone(sender); }
        [Auto] void OnDeletionZoneVisibleUpdated(Visual sender, Boolean value, Boolean oldValue) { UpdateDeletionZone(sender); }
        [Auto] void OnDeletionZoneSizeUpdated(Visual sender, Distance3Property value, Distance3Property oldValue) { UpdateDeletionZone(sender); }
        [Auto] void OnDeletionZoneLocationUpdated(Visual sender, Distance3Property value, Distance3Property oldValue) { UpdateDeletionZone(sender); }
        [Auto] void OnDeletionZoneRotationUpdated(Visual sender, Angle3Property value, Angle3Property oldValue) { UpdateDeletionZone(sender); }

        private void UpdateDeletionZone(Visual sender) {
            // update deletion zone size
            DeletionZone.Depth = DeletionZoneSize.Value.Z;
            DeletionZone.Height = DeletionZoneSize.Value.Y;
            DeletionZone.Width = DeletionZoneSize.Value.X;
            // update deletion zone location
            var location = new Vector3();
            location.X = DeletionZoneLocation.Value.X;
            location.Y = -(float)(sender as BoxVisual).Height / 2 + (float)DeletionZone.Height / 2 + DeletionZoneLocation.Value.Y;
            location.Z = DeletionZoneLocation.Value.Z;
            DeletionZone.Location = location;
            // update deletion zone rotation
            DeletionZone.RotationDegrees = DeletionZoneRotation.Value;
            // update deletion zone visibility
            DeletionZone.Visible = DeletionZoneEnabled.Value ? DeletionZoneVisible.Value : false;
            // show/hide deletion zone properties
            DeletionZoneSize.Hidden = !DeletionZoneEnabled.Value;
            DeletionZoneLocation.Hidden = !DeletionZoneEnabled.Value;
            DeletionZoneRotation.Hidden = !DeletionZoneEnabled.Value;
            DeletionZoneVisible.Hidden = !DeletionZoneEnabled.Value;
            RefreshPropertiesGrid();
        }

        [Auto] void OnDeleteLoadsUpdated(Visual sender, Boolean value, Boolean oldValue) {
            if (DeleteLoads.Value && LoadDeleterEnabled.Value) {
                // delete loads
                var loadsToDelete = StoreLoadsToDelete((sender as BoxVisual).BlockingLoad);
                DeleteLoadsInList(loadsToDelete);
            }
        }

        private List<VisualReference> StoreLoadsToDelete(Visual load) {
            // return loads to delete list
            var loadsToDelete = new List<VisualReference>();
            loadsToDelete.AddUniqueReference(load);
            if (DeletionZoneEnabled.Value) {
                foreach (Visual blockingLoad in DeletionZone.BlockingLoads) {
                    loadsToDelete.AddUniqueReference(blockingLoad);
                }
            }
            return loadsToDelete;
        }

        private void DeleteLoadsInList(List<VisualReference> loadsToDelete) {
            // delete loads in the list (if accepted & not ignored by load filters)
            foreach (VisualReference loadToDelete in loadsToDelete) {
                if (loadToDelete != null) {
                    if (AcceptedLoads.Value.Accept(loadToDelete) && !IgnoredLoads.Value.Accept(loadToDelete)) {
                        loadToDelete.Visual.Delete();
                        ++LoadsDeleted.Value;
                    }
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