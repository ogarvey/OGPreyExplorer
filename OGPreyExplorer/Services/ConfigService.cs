using Newtonsoft.Json;
using OGPreyExplorer.Interfaces;
using OGPreyExplorer.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPreyExplorer.Services
{
  public class ConfigService : ReactiveObject
  {
    private const string ConfigFilePath = "appsettings.json";
    private PreyExplorerConfig? _config;
    public ConfigService() {
      LoadConfig();
    }
    public PreyExplorerConfig Config
    {
      get => _config;
      set => this.RaiseAndSetIfChanged(ref _config, value);
    }

    public void LoadConfig()
    {
      if (File.Exists(ConfigFilePath))
      {
        var json = File.ReadAllText(ConfigFilePath);
        Config = JsonConvert.DeserializeObject<PreyExplorerConfig>(json);
      }
      else
      {
        Config = new PreyExplorerConfig(); // Load default values if config doesn't exist
      }
    }

    public void SaveConfig(PreyExplorerConfig config)
    {
      var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
      File.WriteAllText(ConfigFilePath, json);
    }
  }
}
