using Aki.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;

namespace HideoutArchitect
{
    public enum ENeededDefinition
    {
        NextLevel,
        NextLevelReady
    }
    public class ModConfiguration
    {
        private string _tooltipHeaderColor = "#56C427";

        [JsonConverter(typeof(Utils.StringEnumCommentConverter), "\nNeededForHideoutDefinition - Defines the logic used to determine which items are marked. See mod page for details. \nOptions: \n\tNextLevel: Generally marks items required for the facility's next upgrade\n\tNextLevelReady: Marks items required for the facility's next upgrade only when its other facility pre-requisites are fulfilled. (e.g. Items for Lavatory 2 won't be marked until Water Collector 1 is finished.)")]
        public ENeededDefinition NeededForHideoutDefinition = ENeededDefinition.NextLevelReady;

        [JsonConverter(typeof(Utils.JsonCommentConverter), "\nTooltipHeaderColor - Color of the tooltip 'Needed for hideout' header.")]
        public string TooltipHeaderColor
        {
            get => _tooltipHeaderColor;
            set
            {
                if (value.IsValidHexColor())
                    _tooltipHeaderColor = value;
            }
        }

        public ModConfiguration()
        {
        }

        public static ModConfiguration Load(ModInformation ModInfo)
        {
            string path = VFS.Combine(ModInfo.path, "config.jsonc");
            Debug.LogError($"Loading config from '{path}'");

            ModConfiguration ModConfig;

            string configJson = null;
            try
            {
                if (!VFS.Exists(path))
                    throw new Exception($"No config on path {path} exists!");

                configJson = VFS.ReadFile(path, null);                    
                ModConfig = JsonConvert.DeserializeObject<ModConfiguration>(configJson);
            }
            catch (Exception configReadingException)
            {
                string loadErrorMsg = $"[{ModInfo.name}] Could not load config!";
                Debug.LogError(loadErrorMsg);
                Debug.LogError(configReadingException);

                // Recreate/fill config file
                JObject defaultConfig = JObject.Parse(JsonConvert.SerializeObject(new ModConfiguration()));
                if (configJson != null)
                {
                    void LogBadJsonFormatting(Exception exception = null)
                    {
                        string mergeLoadedConfigMsg = $"[{ModInfo.name}] There was a {(exception != null ? "fatal " : string.Empty)}problem with loading config as JSON, there's likely a bad typo.";
                        Debug.LogError(mergeLoadedConfigMsg);
                        Debug.LogError(exception);
                        Debug.LogError("Restoring config defaults completely.");
                    }

                    if (configJson.IsValidJson())
                    {
                        try
                        {
                            Debug.LogError($"[{ModInfo.name}] Merging existing config data with defaults.");
                            // If the file loaded at least partially, overwrite the defaults with it
                            JObject loadedConfigPart = JObject.Parse(configJson);
                            defaultConfig.Merge(loadedConfigPart, new JsonMergeSettings
                            {
                                // union array values together to avoid duplicates
                                MergeArrayHandling = MergeArrayHandling.Union
                            });
                        }
                        catch (Exception mergeLoadedConfigException)
                        {
                            LogBadJsonFormatting(mergeLoadedConfigException);
                        }
                    }
                    else
                    {
                        LogBadJsonFormatting();
                    }
                }

                string fixedConfigJson = defaultConfig.ToString();
                try
                {
                    Debug.LogError($"[{ModInfo.name}] Parsing default config to JSON\nJson: {fixedConfigJson}");
                    ModConfig = JsonConvert.DeserializeObject<ModConfiguration>(fixedConfigJson);
                }
                catch (Exception configReconstructionException)
                {
                    string fillErrorMsg = $"[{ModInfo.name}] Could not restore config values!";
                    Debug.LogError(fillErrorMsg);
                    Debug.LogError("Yell at Faupi with the logs.");

                    throw configReconstructionException;   // Throw because at this point we can't really continue
                }

                string completeConfigJson = JsonConvert.SerializeObject(ModConfig, Formatting.Indented);
                if (completeConfigJson != configJson)    // There's a difference between the config file and actual config
                {
                    try
                    {
                        Debug.LogError($"[{ModInfo.name}] Writing fixed config...");
                        VFS.WriteFile(path, completeConfigJson.ToString(), false, Encoding.UTF8);
                    }
                    catch
                    {
                        Debug.LogError($"[{ModInfo.name}] There was a problem with writing the config.");
                        throw;
                    }
                }
            }
            
            return ModConfig;
        }
    }
}
