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

        public async Task InitializeAsync()
        {
            var authenticationClient = new AuthenticationClient(Constant.Instance);
            var registration = await authenticationClient.CreateApp("SakabaConsole", Scope.Read | Scope.Write | Scope.Follow);
            var auth = await authenticationClient.ConnectWithPassword(Email, Password);

            MastodonClient = new MastodonClient(registration, auth);
        }

        public async Task PostResultAsync(string name, IEnumerable<BattleResult> results)
        {
            if (!results.Any()) { return; }

            int num = results.Select(x => x.Name).Distinct().Count();
            string users = string.Join(", ", results.Select(x => x.Name).Distinct());
            string lastUser = results.Last().Name;
            string lastContent = Regex.Replace(results.Last().Content, "<span.*</span>", "");
            lastContent = Regex.Replace(lastContent, "<.*?>", "").Trim();

            string result = new StringBuilder()
                .AppendLine($"【{name} を倒した！】")
                .AppendLine($"参加人数: {num}人 ({users})")
                .AppendLine($"最後の一撃: @{lastUser} 「{lastContent}」")
                .ToString();

            await MastodonClient.PostStatus(result, Visibility.Public);
        }
    }
}
