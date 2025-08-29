#nullable enable
using System;
using System.IO;
using Demo3D.Gui.AspectViewer;
using Demo3D.Gui.AspectViewer.Editors;
using Demo3D.Visuals;
using Emulate3D.SourceGenerators;

namespace Demo3D.Components
{
    public partial class VideoRecorderAspect : VisualAspect
    {
        private RunMode initialRunMode = RunMode.Normal;

        [GenerateProperty, FolderBrowserEditor]
        private string outputDirectory = string.Empty;

        [GenerateProperty]
        private string fileName = string.Empty;

        [GenerateProperty]
        private double startTime = 1;

        [GenerateProperty]
        private double endTime = 5;

        private string FullPath => Path.Combine(OutputDirectory, FileName + ".mp4");

        protected override void OnAdded()
        {
            base.OnAdded();
            // If the output directory isn't defined, set it to a default location
            if (string.IsNullOrEmpty(OutputDirectory))
            {
                OutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Emulate3D\\VideoRecordings";
            }

            // If the file name isn't defined, set it to a default value
            if (string.IsNullOrWhiteSpace(FileName))
            {
                FileName = "Recording";
            }
        }

        [Auto]
        void OnInitialize(Visual sender)
        {
            if (!Directory.Exists(OutputDirectory) == false)
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            if (File.Exists(FullPath))
            {
                File.Delete(FullPath);
            }

            // Video recording can't be started whilst the model is running
            // so add a stop event at the StartTime of the recording and start the 
            // recording once the model has stopped.
            initialRunMode = app.RunMode;
            app.SceneAnimation.Settings.StopAtTime = StartTime;
            app.Document.Scene.OnStop.NativeListeners += BeginMovieCapture;
        }

        private void BeginMovieCapture(Visual sender)
        {
            app.Document.Scene.OnStop.NativeListeners -= BeginMovieCapture;
            // Video recording can't be stopped whilst the model is running
            // so add a stop event at the StopTime of the recording and stop the 
            // recording once the model has stopped.
            app.SceneAnimation.Settings.StopAtTime = EndTime;
            app.Document.Scene.OnStop.NativeListeners += EndMovieCapture;

            app.BeginInvoke(new Action(() =>
            {
                print("Starting Movie Capture");
                app.StartVideoRecording(FullPath);
                // Restore the original run mode now that the recording has started
                if (initialRunMode == RunMode.Normal)
                {
                    app.SceneAnimation.Start();
                }
                else
                {
                    app.SceneAnimation.FastForward();
                }
            }));

        }

        private void EndMovieCapture(Visual sender)
        {
            app.Document.Scene.OnStop.NativeListeners -= EndMovieCapture;
            // End the recording
            app.StopVideoRecording();
        }

        [AspectMethod]
        public void OpenDirectory()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{FullPath}\"");
        }
    }
}