﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetAllUserRes
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; } 
        public string PhoneNumber { get; set; }
        public string Role { get; set; } 
        public DateTimeOffset? LockoutEnd { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
    }
}
