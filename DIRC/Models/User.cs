using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIRC.Models
{
    public class User
    {
        public string Nickname { get; set; }

        public User(string desired_nickname)
        {
            Nickname = desired_nickname;
        }
    }
}
