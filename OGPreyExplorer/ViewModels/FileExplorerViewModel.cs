using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OGPreyExplorer.Models;
using OGPreyExplorer.Services;
using ReactiveUI;

namespace OGPreyExplorer.ViewModels
{
  public partial class FileExplorerViewModel : ReactiveObject
  {
    private static IconConverter? s_iconConverter;
    private bool _cellSelection;
    private FileTreeNodeModel? _gameSdkRoot;
    private FileTreeNodeModel? _gameFilesRoot;

    private string _selectedGameSDKPath;
    private string _extractedFilesPath;
    private string? _highlightedPakPath;
    private string? _highlightedOutputPath;
    private List<FileTreeNodeModel> _highlightedOutputItems;

    private readonly ConfigService _configService;

    public FileExplorerViewModel(ConfigService configService)
    {
      _configService = configService;
      Drives = DriveInfo.GetDrives().Select(x => x.Name).ToList();

      _selectedGameSDKPath = _configService.Config.PreyPakFolder ?? (Drives.FirstOrDefault() ?? "/");
      _extractedFilesPath = _configService.Config.ExportFolder ?? (Drives.FirstOrDefault() ?? "/");

      PakFilesSource = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(Array.Empty<FileTreeNodeModel>())
      {
        Columns =
                {
                    new HierarchicalExpanderColumn<FileTreeNodeModel>(
                        new TemplateColumn<FileTreeNodeModel>(
                            "Name",
                            "FileNameCell",
                            "FileNameEditCell",
                            new GridLength(1, GridUnitType.Star),
                            new()
                            {
                                CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                                CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                                IsTextSearchEnabled = true,
                                TextSearchValueSelector = x => x.Name
                            }),
                        x => x.Children,
                        x => x.HasChildren,
                        x => x.IsExpanded),
                    new TextColumn<FileTreeNodeModel, string?>(
                        "Size",
                        x => x.Size,
                        options: new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => long.Parse(x.Size)),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => long.Parse(x.Size)),
                        }),
                }
      };
      GameFilesSource = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(Array.Empty<FileTreeNodeModel>())
      {
        Columns =
                {
                    new HierarchicalExpanderColumn<FileTreeNodeModel>(
                        new TemplateColumn<FileTreeNodeModel>(
                            "Name",
                            "FileNameCell",
                            "FileNameEditCell",
                            new GridLength(1, GridUnitType.Star),
                            new()
                            {
                                CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                                CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                                IsTextSearchEnabled = true,
                                TextSearchValueSelector = x => x.Name
                            }),
                        x => x.Children,
                        x => x.HasChildren,
                        x => x.IsExpanded),
                    new TextColumn<FileTreeNodeModel, string?>(
                        "Size",
                        x => x.Size,
                        options: new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => long.Parse(x.Size)),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => long.Parse(x.Size)),
                        }),
                }
      };
      PakFilesSource.RowSelection!.SingleSelect = false;
      PakFilesSource.RowSelection.SelectionChanged += SelectionChanged;

      GameFilesSource.RowSelection!.SingleSelect = false;
      GameFilesSource.RowSelection.SelectionChanged += GameFileSelectionChanged;

      this.WhenAnyValue(x => x.SelectedGameSDKPath)
          .Subscribe(x =>
          {
            _gameSdkRoot = new FileTreeNodeModel(_selectedGameSDKPath, isDirectory: true, isRoot: true);
            PakFilesSource.Items = new[] { _gameSdkRoot };
          });
      this.WhenAnyValue(x => x.ExtractedFilesPath)
        .Subscribe(x =>
        {
          _gameFilesRoot = new FileTreeNodeModel(_extractedFilesPath, isDirectory: true, isRoot: true);
          GameFilesSource.Items = new[] { _gameFilesRoot };
        });
    }

    public bool CellSelection
    {
      get => _cellSelection;
      set
      {
        if (_cellSelection != value)
        {
          _cellSelection = value;
          if (_cellSelection)
            PakFilesSource.Selection = new TreeDataGridCellSelectionModel<FileTreeNodeModel>(PakFilesSource) { SingleSelect = false };
          else
            PakFilesSource.Selection = new TreeDataGridRowSelectionModel<FileTreeNodeModel>(PakFilesSource) { SingleSelect = false };
          this.RaisePropertyChanged();
        }
      }
    }

    public IList<string> Drives { get; }

    public string SelectedGameSDKPath
    {
      get => _selectedGameSDKPath;
      set => this.RaiseAndSetIfChanged(ref _selectedGameSDKPath, value);
    }

    public string ExtractedFilesPath
    {
      get => _extractedFilesPath;
      set => this.RaiseAndSetIfChanged(ref _extractedFilesPath, value);
    }

    public string? HighlightedPakPath
    {
      get => _highlightedPakPath;
      set => SetHighlightedPath(value, true);
    }

    public string? HighlightedOutputPath
    {
      get => _highlightedOutputPath;
      set => this.SetHighlightedPath(value, false);
    }

    public List<FileTreeNodeModel> HighlightedOutputItems
    {
      get => _highlightedOutputItems;
      set => this.RaiseAndSetIfChanged(ref _highlightedOutputItems, value);
    }

    public HierarchicalTreeDataGridSource<FileTreeNodeModel> PakFilesSource { get; }
    public HierarchicalTreeDataGridSource<FileTreeNodeModel> GameFilesSource { get; }

    public static IMultiValueConverter FileIconConverter
    {
      get
      {
        if (s_iconConverter is null)
        {
          using (var fileStream = AssetLoader.Open(new Uri("avares://OGPreyExplorer/Assets/file.png")))
          using (var folderStream = AssetLoader.Open(new Uri("avares://OGPreyExplorer/Assets/folder.png")))
          using (var folderOpenStream = AssetLoader.Open(new Uri("avares://OGPreyExplorer/Assets/folder-open.png")))
          {
            var fileIcon = new Bitmap(fileStream);
            var folderIcon = new Bitmap(folderStream);
            var folderOpenIcon = new Bitmap(folderOpenStream);

            s_iconConverter = new IconConverter(fileIcon, folderOpenIcon, folderIcon);
          }
        }

        return s_iconConverter;
      }
    }

    private void SetHighlightedPath(string? value, bool isPakSource)
    {
      if (string.IsNullOrEmpty(value))
      {
        if (isPakSource)
        {
          PakFilesSource.RowSelection!.Clear();
        }
        else
        {
          GameFilesSource.RowSelection!.Clear();
        }
        return;
      }

      var path = value;
      var components = new Stack<string>();
      DirectoryInfo? d = null;

      if (File.Exists(path))
      {
        var f = new FileInfo(path);
        components.Push(f.Name);
        d = f.Directory;
      }
      else if (Directory.Exists(path))
      {
        d = new DirectoryInfo(path);
      }

      while (d is not null)
      {
        components.Push(d.Name);
        d = d.Parent;
      }

      var index = IndexPath.Unselected;

      if (components.Count > 0)
      {
        var drive = components.Pop();
        var driveIndex = Drives.ToList().FindIndex(x => string.Equals(x, drive, StringComparison.OrdinalIgnoreCase));

        if (driveIndex >= 0)
        {
          if (isPakSource) SelectedGameSDKPath = Drives[driveIndex];
          else ExtractedFilesPath = Drives[driveIndex];
        }


        FileTreeNodeModel? node = isPakSource ? _gameSdkRoot : _gameFilesRoot;
        index = new IndexPath(0);

        while (node is not null && components.Count > 0)
        {
          node.IsExpanded = true;

          var component = components.Pop();
          var i = node.Children.ToList().FindIndex(x => string.Equals(x.Name, component, StringComparison.OrdinalIgnoreCase));
          node = i >= 0 ? node.Children[i] : null;
          index = i >= 0 ? index.Append(i) : default;
        }
      }

      if (isPakSource)
      {
        PakFilesSource.RowSelection!.Select(index);
      }
      else
      {
        GameFilesSource.RowSelection!.Select(index);
      }
    }

    private void GameFileSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<FileTreeNodeModel> e)
    {
      var highlightedPath = GameFilesSource.RowSelection?.SelectedItem?.Path;
      var highlightedItems = GameFilesSource.RowSelection?.SelectedItems?.ToList();
      this.RaiseAndSetIfChanged(ref _highlightedOutputPath, highlightedPath, nameof(HighlightedOutputPath));
      this.RaiseAndSetIfChanged(ref _highlightedOutputItems, highlightedItems, nameof(HighlightedOutputItems));
    }

    private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<FileTreeNodeModel> e)
    {
      var highlightedPath = PakFilesSource.RowSelection?.SelectedItem?.Path;
      this.RaiseAndSetIfChanged(ref _highlightedPakPath, highlightedPath, nameof(HighlightedPakPath));

      //foreach (var i in e.DeselectedItems)
      //  System.Diagnostics.Trace.WriteLine($"Deselected '{i?.Path}'");
      //foreach (var i in e.SelectedItems)
      //  System.Diagnostics.Trace.WriteLine($"Selected '{i?.Path}'");
    }

    private class IconConverter : IMultiValueConverter
    {
      private readonly Bitmap _file;
      private readonly Bitmap _folderExpanded;
      private readonly Bitmap _folderCollapsed;

      public IconConverter(Bitmap file, Bitmap folderExpanded, Bitmap folderCollapsed)
      {
        _file = file;
        _folderExpanded = folderExpanded;
        _folderCollapsed = folderCollapsed;
      }

      public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
      {
        if (values.Count == 2 &&
            values[0] is bool isDirectory &&
            values[1] is bool isExpanded)
        {
          if (!isDirectory)
            return _file;
          else
            return isExpanded ? _folderExpanded : _folderCollapsed;
        }

        return null;
      }
    }
  }
}
