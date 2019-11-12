using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISaveLoader
{
    event Action<string> OnSavedAsync;
    event Action<string> OnLoadedAsync;

    void Save(string id, string data);
    string Load(string id);
    
    Task SaveAsync(string id, string data);
    Task<string> LoadAsync(string id);
}
