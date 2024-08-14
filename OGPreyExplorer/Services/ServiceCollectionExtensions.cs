using Microsoft.Extensions.DependencyInjection;
using OGPreyExplorer.Interfaces;
using OGPreyExplorer.ViewModels;

namespace OGPreyExplorer.Services
{
  public static class ServiceCollectionExtensions
  {
    public static void AddPreyExplorerServices(this IServiceCollection collection)
    {
      collection.AddSingleton<ConfigService>();
      collection.AddTransient<FileExplorerViewModel>();
    }
  }
}
