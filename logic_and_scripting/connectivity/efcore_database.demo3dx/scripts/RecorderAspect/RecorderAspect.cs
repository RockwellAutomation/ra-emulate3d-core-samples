#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using Demo3D.Gui.AspectViewer;
using Demo3D.Utilities;
using Demo3D.Visuals;
using Microsoft.DirectX;
using Microsoft.EntityFrameworkCore;

namespace Demo3D.Components {
    public enum RecordingStatus {
        Stopped, Recording, Playback
    }

    [Category("Database")]
    public class RecorderAspect : VisualAspect {
        private RecordingStatus recordingStatus;

        private float recordInterval = 0.1f;
        private Visual? recordTarget;
        private ObservableCollection<Visual> playbackTargets = new();

        private DateTime recordStartTime;
        private Timer? recordTimer;
        private Timer? playbackTimer;

        private int playbackIndex;
        private List<Record> recordingRecords = new();
        private List<Record> playbackRecords = new();

        private string? DatabasePath { get; set; }

        public string DatabaseName { get; set; } = "recorder.db";

        /// <summary>
        /// Capture a record every interval, <see cref="RecordInterval"/>.
        /// </summary>
        [Time] public float           RecordInterval { get => recordInterval; set => SetProperty(ref recordInterval, value, nameof(RecordInterval));}

        [ReadOnly(true)]
        public RecordingStatus Status { get => recordingStatus; private set => SetProperty(ref recordingStatus, value, nameof(Status)); }
        public Visual?         RecordTarget {  get => recordTarget; set => SetProperty(ref recordTarget, value, nameof(RecordTarget)); }
        public ObservableCollection<Visual> PlaybackTargets { get => playbackTargets; set => SetProperty(ref playbackTargets, value, nameof(PlaybackTargets)); }

        protected override void OnAdded() {
            base.OnAdded();
            var dir = Path.Combine(app.DocumentsDirectory, "Data");
            DatabasePath = Path.Combine(dir, DatabaseName);
            if(!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            print($"Database Path: {DatabasePath}");
            using var db = new DatabaseContext(DatabasePath);
        }

        protected override void OnRemoved() {
            base.OnRemoved();
            recordTimer?.Dispose();
            playbackTimer?.Dispose();
        }

        [AspectMethod]
        public void StartRecording() {
            if(RecordTarget == null) {
                app.LogMessage("Error", "No visual set to record", null);
                return;
            }
            recordingRecords.Clear();
            using var db = new DatabaseContext(DatabasePath);
            //Clear the database, will be slow on large tables. (EFCore 7 has a better way to do this)
            db.Records.RemoveRange(db.Records);
            db.SaveChanges();

            Status = RecordingStatus.Recording;
            recordTimer = new Timer(RecordInterval * 1000) {
                AutoReset = true
            };
            recordTimer.Elapsed += RecordTimer_Elapsed;
            recordStartTime = DateTime.Now;
            recordTimer.Start();
        }

        [AspectMethod]
        public void StopRecording() {
            print($"Writing {recordingRecords.Count} number of records to database");
            using var db = new DatabaseContext(DatabasePath);
            db.Records.AddRange(recordingRecords);
            db.SaveChanges();
            recordTimer?.Stop();
            recordTimer?.Dispose();
            recordTimer = null;
            Status = RecordingStatus.Stopped;
        }

        [AspectMethod]
        public void PlayRecording() {
            if (RecordTarget == null) {
                app.LogMessage("Error", "No visual set to record", null);
                return;
            }

            using var db = new DatabaseContext(DatabasePath);
            playbackRecords = db.Records.ToList();
            print($"Playing back {playbackRecords.Count} number of records to database");
            Status = RecordingStatus.Playback;
            playbackTimer = new Timer(RecordInterval * 1000) {
                AutoReset = true
            };
            playbackTimer.Elapsed += PlaybackTimer_Elapsed;
            playbackIndex = 0;
            playbackTimer.Start();
        }

        [AspectMethod]
        public void StopPlayback() {
            print("Playback stopped");
            playbackTimer?.Stop();
            playbackTimer?.Dispose();
            playbackTimer = null;
            Status = RecordingStatus.Stopped;
        }

        [AspectMethod]
        public void RecreateDatabase() {
            var dir = Path.Combine(app.DocumentsDirectory, "Data");
            DatabasePath = Path.Combine(dir, DatabaseName);
            File.Delete(DatabasePath);
            print($"Database Path: {DatabasePath}");
            using var db = new DatabaseContext(DatabasePath);
        }

        private void RecordTimer_Elapsed(object? sender, ElapsedEventArgs e) {
            if (RecordTarget == null) {
                if (sender is Timer timer) {
                    timer.Stop();
                    timer.Dispose();
                }
                app.LogMessage("Error", "No visual set to record", null);
                return;
            }

            var locDiff = RecordTarget.Location - RecordTarget.InitialLocation;
            var rotDiff = RecordTarget.RotationDegrees - RecordTarget.InitialRotationDegrees;

            recordingRecords.Add(new Record {
                Time = DateTime.Now - recordStartTime,
                LocationX = locDiff.X,
                LocationY = locDiff.Y,
                LocationZ = locDiff.Z,
                RotationX = rotDiff.X,
                RotationY = rotDiff.Y,
                RotationZ = rotDiff.Z
            });
        }

        private void PlaybackTimer_Elapsed(object? sender, ElapsedEventArgs e) {
            playbackIndex++;
            if (playbackIndex < playbackRecords.Count) {
                var record = playbackRecords[playbackIndex];
                app.Invoke(() => {
                    var locOffset = new Vector3(record.LocationX, record.LocationY, record.LocationZ);
                    var rotOffset = new Vector3(record.RotationX, record.RotationY, record.RotationZ);
                    print(locOffset);
                    foreach (var t in PlaybackTargets) {
                        t.Location        = t.InitialLocation        + locOffset;
                        t.RotationDegrees = t.InitialRotationDegrees + rotOffset;
                    }
                    app.Redraw();
                });
            }
            else {
                print("Playback finished");
                playbackTimer?.Stop();
                playbackTimer?.Dispose();
                playbackTimer = null;
                Status = RecordingStatus.Stopped;
            }
        }
    }
}