using System.Collections.Generic;

namespace com.koerber
{
public class Config
    {
        public int Port { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}