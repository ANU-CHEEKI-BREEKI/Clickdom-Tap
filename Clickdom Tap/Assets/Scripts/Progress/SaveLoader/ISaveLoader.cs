using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISaveLoader
{
    void Save(string id, string data, Action<bool> onSaved = null);
    void Load(string id, Action<bool, string> onLoaded = null);
}
