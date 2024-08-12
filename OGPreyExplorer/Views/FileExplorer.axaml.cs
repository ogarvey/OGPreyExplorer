using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OGPreyExplorer.ViewModels;
using System.Diagnostics;

namespace OGPreyExplorer.Views;

public partial class FileExplorer : ReactiveUserControl<FileExplorerViewModel>
{
  public FileExplorer()
  {
    InitializeComponent();
  }
}