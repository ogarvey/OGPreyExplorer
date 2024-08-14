using OGPreyExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPreyExplorer.Interfaces
{
  public interface IConfigService
  {
    PreyExplorerConfig GetConfig();
    void SaveConfig(PreyExplorerConfig config);
  }
}
