﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticationDemo.DataAccess
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public ICollection<User> Users { get; set; }
    }
}