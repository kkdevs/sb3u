using System;
using System.Configuration;
using System.IO;

namespace SB3Utility.Properties {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings {

		public const int MaxExternalTools = 30;

        public Settings() {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //

			for (int i = 0; i < MaxExternalTools; i++)
			{
				SettingsProperty newProp = new SettingsProperty(
					"ExternalTool" + i, typeof(ExternalTool),
					this.Providers[Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location)],
					false,
					null,
					SettingsSerializeAs.String,
					null,
					true, true
				);
				this.Properties.Add(newProp);
			}

			this.SettingsLoaded += new System.Configuration.SettingsLoadedEventHandler(Settings_SettingsLoaded);
        }

		void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
		{
			for (int i = 0; i < MaxExternalTools; i++)
			{
				string toolName = "ExternalTool" + i;
				if (this.PropertyValues[toolName].PropertyValue == null)
				{
					this.PropertyValues.Remove(toolName);
					this.Properties.Remove(toolName);
				}
			}
		}
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
