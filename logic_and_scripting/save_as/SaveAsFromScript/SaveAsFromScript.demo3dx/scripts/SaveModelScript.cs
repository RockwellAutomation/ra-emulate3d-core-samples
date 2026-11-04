using Demo3D.Native;
using Demo3D.Visuals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

[Auto] public class SaveModelScript : NativeObject {
    public SaveModelScript(Visual sender) : base(sender) 
    {
        FileName.Category = "Save As";
        FilePath.Category = "Save As";
        SaveNow.Category = "Save As";
    }
    
    [Auto] public SimplePropertyValue<string> FileName;
    [Auto] public SimplePropertyValue<string> FilePath;
    [Auto] public SimplePropertyValue<bool> SaveNow;
    
    
    [Context("Save As")]
    public void Save(Visual sender) {
        
        // Check a file name has been inputted
        // If no name has been input, exit with error
        if(String.IsNullOrEmpty(FileName.Value))
        {
            app.LogMessage("Error", "Cannot Save - No File Name provided.", sender);
            return;
        }
        
        // Get the existing extension of the file
        var ext = Path.GetExtension(document.Path);

        // Check if a valid file path has been input
        if(!String.IsNullOrEmpty(FilePath.Value) && Directory.Exists(Path.TrimEndingDirectorySeparator(FilePath.Value)))
        {           
            // Confirm with the User if the file already exists, if it should be overwritten
            if(File.Exists(document.Path))
            {
                string message = "The file already exists in the directory. Continue with saving and overwrite the existing file?";
                string title = "File Already Exists";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, title, buttons);
                if (result == DialogResult.No)
                {
                    app.LogMessage("Alert", "Save aborted.", sender);
                    return;
                }
            }
            
            document.Path = FilePath.Value + "\\" + FileName.Value + ext;
        }
        // If a valid file path has not been input, use the current directory
        else
        {
            // Notify with an alert this is being done
            app.LogMessage("Alert", "File Path does not exist. The current file directory will be used instead.", sender);
            
            var path = document.Path;
            var dir = Path.GetDirectoryName(path);
            
            // If the model has not previously been saved, the current directory will be null
            // Therefore, the save exits with error
            if (String.IsNullOrEmpty(dir)) 
            {
                app.LogMessage("Error", "No valid save directory found.", sender);
                return;
            }
            document.Path = dir + "\\" + FileName.Value + ext;
        }
       
        // Save the document
        document.Save();
    }

    [Auto] void OnSaveNowUpdated(Visual sender, Boolean value, Boolean oldValue) {
        if(value)
        {
            Save(sender);
            SaveNow.Value = false;
        }
        else
        {
            return;
        }
    }    
}