using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OGPreyExplorer.Services;
using OGPreyExplorer.ViewModels;
using OGPreyExplorer.Views;

namespace OGPreyExplorer;

public partial class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    var collection = new ServiceCollection();
    collection.AddPreyExplorerServices();
    var serviceProvider = collection.BuildServiceProvider();
    var vm = serviceProvider.GetRequiredService<FileExplorerViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindow(configService: serviceProvider.GetRequiredService<ConfigService>())
      {
        DataContext = vm,
      };
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
    {
      singleViewPlatform.MainView = new FileExplorer
      {
        DataContext = vm
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}
