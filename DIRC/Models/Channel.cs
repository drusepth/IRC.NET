using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIRC.Models
{
    public class Channel
    {
        public string Name { get; set; }

        public Channel(string channel_name)
        {
            Name = channel_name;
        }
    }
}
