using System.Collections.Generic;

namespace SimpleUI.Models
{
    public class UserInfo
    {
        public string Name { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }   
}
