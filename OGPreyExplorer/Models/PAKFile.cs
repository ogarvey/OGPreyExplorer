using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPreyExplorer.Models
{
  public class PAKFile
  {
    public string Path { get; set; }
    public string Name { get; set; }
    public long Size { get; set; }
    public bool IsConverted { get; set; }
    public bool IsExtracted { get; set; }
    public string? ExtractedPath { get; set; }

    public PAKFile(string path, string name)
    {
      Path = path;
      Name = name;
      Size = File.GetAttributes(path).HasFlag(FileAttributes.Directory) ? 0 : new FileInfo(path).Length;
      IsConverted = false;
      IsExtracted = false;
      ExtractedPath = null;
    }
  }
}
