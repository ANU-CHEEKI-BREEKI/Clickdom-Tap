﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public interface ISimpleEntityConverter
{
    void ConvertToEntity(Entity entity, EntityManager manager);
}