﻿using Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public Enums.RoleType Name { get; set; }
    }
}
