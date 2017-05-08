using Mastonet;
using System;
using System.Collections.Generic;
using System.IO;
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
        int Life;
        bool IsRunning;

        public Battle(Boss boss)
        {
            Boss = boss;
            MastodonClient = boss.MastodonClient;

            Task.Run(async () => {
                await Task.Delay(limmitTime);
                await End(false);
            });
        }

        public async Task Start()
        {
            Results = new List<BattleResult>();
            Life = Boss.LifePoint;
            IsRunning = true;

            string accoutName = "boss";

            int[] mediaIds = null; // アイコンのアップロード
            using (var stream = File.OpenRead(Boss.ImagePath))
            {
                var attach = await MastodonClient.UploadMedia(stream);
                mediaIds = new int[] { attach.Id };
            }
            Console.WriteLine("画像をアップロードしました");

            string appear = new StringBuilder()
                .AppendLine($"（{Boss.Name}が現れた！ LP: {Boss.LifePoint}）")
                .AppendLine(Boss.VoiceAppear)
                .ToString();
            await MastodonClient.PostStatus(appear, Visibility.Public, mediaIds: mediaIds);

            
            UserStreaming = MastodonClient.GetUserStreaming();
            UserStreaming.OnNotification += async (sender, e) =>
            {
                Console.WriteLine("通知を受信しました");

                if (!IsRunning) { return; }

                var status = e.Notification.Status;
                if (status == null || !status.Content.Contains($"@<span>{accoutName}</span>")) { return; }

                Console.WriteLine(status.Content);

                string content = DeleteTags(status.Content);
                if (Boss.Weakness != "")
                {
                    bool matchWeakness = false;
                    foreach (string weakness in Boss.Weakness.Replace("\r\n", "\n").Split('\n'))
                    {
                        if (weakness != "" && content.Contains(weakness))
                        {
                            matchWeakness = true;
                            break;
                        }
                    }
                    if (!matchWeakness)
                    {
                        string nodamage = new StringBuilder()
                            .AppendLine($"{Boss.Name} (LP: {Life}/{Boss.LifePoint})")
                            .AppendLine($"（{Boss.Name}にダメージを与えられない！）")
                            .AppendLine($"> {GetName(status.Account)}「{content}」")
                            .ToString();
                        await MastodonClient.PostStatus(nodamage, Visibility.Public);
                        return;
                    }
                }
                if (randomizer.Next(99) < Boss.EvadeRate)
                {
                    string evade = new StringBuilder()
                        .AppendLine($"{Boss.Name} (LP: {Life}/{Boss.LifePoint})")
                        .AppendLine($"（{Boss.Name}は攻撃を回避した！）")
                        .AppendLine($"> {GetName(status.Account)}「{content}」")
                        .ToString();
                    await MastodonClient.PostStatus(evade, Visibility.Public);
                    return;
                }

                
                Life--;
                if (Life < 0) { return; }

                Results.Add(new BattleResult
                {
                    PostId = status.Id,
                    Name = status.Account.AccountName,
                    Content = status.Content
                });
                if (Life == 0)
                {
                    string dead = new StringBuilder()
                        .AppendLine($"{Boss.Name} (LP: 0/{Boss.LifePoint})")
                        .AppendLine($"{Boss.VoiceDead}")
                        .AppendLine($"> {GetName(status.Account)}「{content}」")
                        .ToString();
                    await MastodonClient.PostStatus(dead, Visibility.Public);

                    await End(true);
                    return;
                }

                string damage = new StringBuilder()
                    .AppendLine($"{Boss.Name} (LP: {Life}/{Boss.LifePoint})")
                    .AppendLine($"{Boss.VoiceDamage}")
                    .AppendLine($"> {GetName(status.Account)}「{content}」")
                    .ToString();
                await MastodonClient.PostStatus(damage, Visibility.Public);

                if (Life % 2 == 1)
                {
                    string counter = $"@{status.Account.AccountName} {Boss.VoiceCounter}";
                    await MastodonClient.PostStatus(counter, Visibility.Public);
                }
            };

            Console.WriteLine("Battleを開始します");
            await UserStreaming.Start();
        }

        public async Task End(bool success)
        {
            if (IsRunning)
            {
                IsRunning = false;

                if (success)
                {
                    var record = new Record();
                    await record.InitializeAsync();

                    int num = Results.Select(x => x.Name).Distinct().Count();
                    string users = string.Join(", ", Results.Select(x => x.Name).Distinct());
                    string lastUser = Results.Last().Name;
                    string lastContent = DeleteTags(Results.Last().Content);

                    var builder = new StringBuilder()
                        .AppendLine($"【{Boss.Name}を倒した！】");
                    if (Boss.DropItem != "")
                    {
                        builder.AppendLine($"「{Boss.DropItem}」を手に入れた");
                    }
                    builder.AppendLine($"参加人数: {num}人 ({users})")
                        .AppendLine($"最後の一撃: @{lastUser} 「{lastContent}」");

                    string result = builder.ToString();
                    await record.MastodonClient.PostStatus(result, Visibility.Public);
                }
                else
                {
                    await MastodonClient.PostStatus($"（{Boss.Name}は去って行った...）", Visibility.Public);
                }

                if (UserStreaming != null)
                {
                    UserStreaming.Stop();
                }
            }
        }

        private string DeleteTags(string content)
        {
            content = Regex.Replace(content, "<span.*</span>", "");
            content = Regex.Replace(content, "<.*?>", "").Trim();
            return content;
        }
        private string GetName(Mastonet.Entities.Account account)
        {
            if (account.DisplayName != "")
            {
                return account.DisplayName;
            }
            else
            {
                return account.UserName;
            }
        }
    }
}
