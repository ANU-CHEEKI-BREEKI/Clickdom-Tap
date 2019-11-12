using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ASavable : MonoBehaviour
{
    [SerializeField] private int id = -1;
    public int Id { get { return id; } }

    [ContextMenu("SetUniqueId")]
    private void SetUniqueId()
    {
        var savables = FindObjectsOfType<ASavable>();

        var s_ids = savables
                   .Select(s => s.id)
                   .ToList();

        try
        {
            if (!s_ids.Contains(id))
                return;

            var max = s_ids.OrderBy(id => id).Last() + 1;
            for (int i = 1; i <= max; i++)
            {
                if (!s_ids.Contains(i))
                {
                    id = i;
                    s_ids.Add(id);
                    return;
                }
            }
        }
        finally
        {
            var sb = new StringBuilder();
            foreach (var item in s_ids.OrderBy(s=>s))
                sb.Append(item).Append(", ");
            Debug.Log(sb.ToString());
        }
    }
}
