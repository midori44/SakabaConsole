using Mastonet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SakabaConsole
{
    class Record
    {
        protected string Email { get; set; }
        protected string Password { get; set; }
        public MastodonClient MastodonClient { get; private set; }

        public Record()
        {
            Email = Constant.RecordEmail;
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
