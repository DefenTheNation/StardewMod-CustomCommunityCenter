﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCommunityCenter
{
    public class IngredientSaveData
    {
        public int ItemId { get; set; }       
        public int ItemQuality { get; set; }
        public int RequiredStack { get; set; }
        public bool Completed { get; set; }
    }
}
