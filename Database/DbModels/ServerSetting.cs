using System;

namespace CustomPageFramework.Database.DbModels
{
    public class ServerSetting
    {
        public string ServerType { get; set; }

        public string ServerUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public DateTime LastSavedOn { get; set; }
    }
}
