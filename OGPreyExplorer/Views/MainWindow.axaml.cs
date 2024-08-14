using Avalonia.Controls;
using Avalonia.Interactivity;
using CgfConverter;
using CgfConverter.PackFileSystem;
using CgfConverter.Renderers;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using OGPreyExplorer.Services;
using OGPreyExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace OGPreyExplorer.Views;

public partial class MainWindow : Window
{
  private readonly ConfigService _configService;
  public MainWindow(ConfigService configService)
  {
    _configService = configService;
    InitializeComponent();
  }

  private void RunRenderersAndThrowAggregateExceptionIfAny(IEnumerable<IRenderer> renderers)
  {
    var exceptions = new List<Exception>();
    foreach (var renderer in renderers)
    {
      try
      {
        renderer.Render();
      }
      catch (Exception e)
      {
        exceptions.Add(e);
      }
    }

    if (exceptions.Any())
      throw new AggregateException(exceptions);
  }

  private void Pak_OnClick(object? sender, RoutedEventArgs e)
  {
    var pakPath = ((FileExplorerViewModel)DataContext!).HighlightedPakPath;
    if (pakPath != null)
    {
      if (File.Exists(pakPath))
      {
        if (Path.GetExtension(pakPath) != ".pak")
          return;

        var zipName = Path.GetFileNameWithoutExtension(pakPath) + ".zip";
        var tmpDir = Path.GetTempPath();
        var zipPath = Path.Combine(tmpDir, zipName);
        var preyPath = _configService.Config.PreyConvertExePath;
        // use PreyConvert.exe to convert .pak to .zip
        var process = new Process
        {
          StartInfo = new ProcessStartInfo
          {
            FileName = preyPath,
            Arguments = $"\"{pakPath}\" \"{zipPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
          }
        };
        process.Start();
        process.WaitForExit();
        // extract the .zip to to the path specified in the config
        var extPath = _configService.Config.ExportFolder;

        if (extPath != null) { 
          var extDir = Path.Combine(extPath, Path.GetFileNameWithoutExtension(pakPath));
          Directory.CreateDirectory(extDir);
          ZipFile.ExtractToDirectory(zipPath, extDir, overwriteFiles: true);
          // delete the .zip
          File.Delete(zipPath);
        }

      }
    }  
  }

  private void Cgf_OnClick(object? sender, RoutedEventArgs e)
  {
    //var test = ((FileExplorerViewModel)DataContext!).HighlightedOutputPath;
    var items = ((FileExplorerViewModel)DataContext!).HighlightedOutputItems;
    if (items != null)
    {
      foreach (var file in items)
      {
        if (File.Exists(file.Path))
        {
          var ext = Path.GetExtension(file.Path);
          string skinmPath = "";
          if (ext == ".skinm")
          {
            // copy file and rename to .cgf
            var cgfPath = Path.ChangeExtension(file.Path, ".cgf");
            File.Copy(file.Path, cgfPath, overwrite: true);
            skinmPath = cgfPath;
          }
          if (ext == ".cgf" || ext == ".skin" || ext == ".chr")
          {
            var argsHandler = new ArgsHandler();
            var args = new string[] { ext == ".skinm" ? skinmPath : file.Path, "-noconflict", "-gltf", "-objectDir", @"C:\Dev\Projects\Gaming\VGR\PC\Prey\MAIN" };
            var numErrorsOccurred = argsHandler.ProcessArgs(args);
            var data = new CryEngine(file.Path, argsHandler.PackFileSystem, materialFiles: argsHandler.MaterialFile);

            data.ProcessCryengineFiles();

            var renderers = new List<IRenderer>();
            renderers.Add(new GltfModelRenderer(argsHandler, data, argsHandler.OutputGLTF, argsHandler.OutputGLB));
            renderers.Add(new ColladaModelRenderer(argsHandler, data));

            RunRenderersAndThrowAggregateExceptionIfAny(renderers);
          }
        }
      }
    }
  }
}
