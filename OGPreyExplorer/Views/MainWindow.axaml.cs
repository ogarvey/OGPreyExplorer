using Avalonia.Controls;
using Avalonia.Interactivity;
using CgfConverter;
using CgfConverter.PackFileSystem;
using CgfConverter.Renderers;
using CgfConverter.Renderers.Gltf;
using OGPreyExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OGPreyExplorer.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
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

  private void Cgf_OnClick(object? sender, RoutedEventArgs e)
  {
    var test = ((FileExplorerViewModel)DataContext!).HighlightedPath;
    if (test != null)
    {
      if (File.Exists(test))
      {
        var ext = Path.GetExtension(test);
        if (ext == ".cgf" || ext == ".skin" || ext == ".skinm")
        {
          var argsHandler = new ArgsHandler();
          var args = new string[] { test, "-gltf", "-objectDir", Path.GetDirectoryName(test) };
          var numErrorsOccurred = argsHandler.ProcessArgs(args);
          var data = new CryEngine(test, argsHandler.PackFileSystem, materialFiles: argsHandler.MaterialFile);

          data.ProcessCryengineFiles();

          var renderers = new List<IRenderer>();
          renderers.Add(new GltfModelRenderer(argsHandler, data, argsHandler.OutputGLTF, argsHandler.OutputGLB));

          RunRenderersAndThrowAggregateExceptionIfAny(renderers);
        }
      }
    }
  }
}
