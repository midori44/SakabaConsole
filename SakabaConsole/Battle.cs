using Mastonet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SakabaConsole
{
    class Battle
    {
        static int limmitTime = 1800_000;
        Random randomizer = new Random();

        TimelineStreaming UserStreaming;
        MastodonClient MastodonClient;
        Boss Boss;
        List<BattleResult> Results;
        int AttackCount;
        bool IsRunning;

        public Battle(Boss boss)
        {
            Boss = boss;
            MastodonClient = boss.MastodonClient;

            Task.Run(async () =>
            {
                await Task.Delay(limmitTime);
                await End(false);
            });
        }

        public async Task Start()
        {
            Results = new List<BattleResult>();
            AttackCount = 0;
            IsRunning = true;

            //var postStatus = await MastodonClient.PostStatus(Boss.VoiceAppear, Visibility.Public);
            string accoutName = "boss";


            UserStreaming = MastodonClient.GetUserStreaming();
            UserStreaming.OnNotification += async (sender, e) =>
            {
                var status = e.Notification.Status;
                if (status == null || !status.Content.Contains($"@<span>{accoutName}</span>")) { return; }

                if (Boss.Weakness != "")
                {
                    bool matchWeakness = false;
                    foreach (string weakness in Boss.Weakness.Split('\n'))
                    {
                        if (status.Content.Contains(weakness))
                        {
                            matchWeakness = true;
                            break;
                        }
                    }
                    if (!matchWeakness)
                    {
                        string nodamage = new StringBuilder()
                            .AppendLine($"（{Boss.Name}にダメージを与えられない！）")
                            .AppendLine("")
                            .AppendLine($"{status.Account.AccountName}「{DeleteTags(status.Content)}」")
                            .ToString();
                        await MastodonClient.PostStatus(nodamage, Visibility.Public);
                        return;
                    }
                }
                if (randomizer.Next(99) < Boss.EvadeRate)
                {
                    string evade = new StringBuilder()
                        .AppendLine($"（{Boss.Name}は攻撃を回避した！）")
                        .AppendLine("")
                        .AppendLine($"{status.Account.AccountName}「{DeleteTags(status.Content)}」")
                        .ToString();
                    await MastodonClient.PostStatus(evade, Visibility.Public);
                    return;
                }

                
                AttackCount++;
                if (AttackCount > Boss.LifePoint) { return; }

                Results.Add(new BattleResult
                {
                    PostId = status.Id,
                    Name = status.Account.AccountName,
                    Content = status.Content
                });
                if (AttackCount == Boss.LifePoint)
                {
                    string dead = new StringBuilder()
                        .AppendLine(Boss.VoiceDead)
                        .AppendLine("")
                        .AppendLine($"{status.Account.AccountName}「{DeleteTags(status.Content)}」")
                        .ToString();
                    await MastodonClient.PostStatus(dead, Visibility.Public);

                    await End(true);
                    return;
                }

                string damage = new StringBuilder()
                    .AppendLine(Boss.VoiceDamage)
                    .AppendLine("")
                    .AppendLine($"{status.Account.AccountName}「{DeleteTags(status.Content)}」")
                    .ToString();
                await MastodonClient.PostStatus(damage, Visibility.Public);

                string counter = $"@{status.Account.AccountName} {Boss.VoiceCounter}";
                await MastodonClient.PostStatus(counter, Visibility.Public);
            };

            await UserStreaming.Start();
        }

        public async Task End(bool success)
        {
            if (IsRunning)
            {
                IsRunning = false;
                if (UserStreaming != null)
                {
                    UserStreaming.Stop();
                }

                if (success)
                {
                    var record = new Record();
                    await record.InitializeAsync();

                    await record.PostResultAsync(Boss.Name, Results);
                    if (Boss.DropItem != "")
                    {
                        await MastodonClient.PostStatus($"【{Boss.Name}は {Boss.DropItem} を落とした！】", Visibility.Public);
                    }
                }
                else
                {
                    await MastodonClient.PostStatus($"（{Boss.Name}は去って行った...）", Visibility.Public);
                }

            }
        }

        private string DeleteTags(string content)
        {
            content = Regex.Replace(content, "<span.*</span>", "");
            content = Regex.Replace(content, "<.*?>", "").Trim();
            return content;
        }
    }
}
