using Mastonet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SakabaConsole
{
    class Boss
    {
        protected string Email { get; set; }
        protected string Password { get; set; }
        public MastodonClient MastodonClient { get; private set; }
        protected TimelineStreaming TimelineStreaming { get; set; }

        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int LifePoint { get; set; }
        public int EvadeRate { get; set; }
        public string VoiceAppear { get; set; }
        public string VoiceDamage { get; set; }
        public string VoiceCounter { get; set; }
        public string VoiceDead { get; set; }
        public string Weakness { get; set; }
        public string DropItem { get; set; }

        public Boss()
        {
            Email = Constant.BossEmail;
            Password = Constant.Password;
        }

        public async Task InitializeAsync()
        {
            var authenticationClient = new AuthenticationClient(Constant.Instance);
            var registration = await authenticationClient.CreateApp("SakabaConsole", Scope.Read | Scope.Write | Scope.Follow);
            var auth = await authenticationClient.ConnectWithPassword(Email, Password);

            MastodonClient = new MastodonClient(registration, auth);
        }
    }
}
