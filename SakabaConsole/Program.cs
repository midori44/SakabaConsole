using System;
using System.IO;
using System.Threading.Tasks;

namespace SakabaConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args).Wait();
        }

        private static async Task Run(string[] args)
        {
            Console.WriteLine(string.Join(" ", args));

            string Name = args[0];
            string ImagePath = args[1];
            int LifePoint = int.Parse(args[2]);
            int EvadeRate = int.Parse(args[3]);
            string VoiceAppear = args[4];
            string VoiceDamage = args[5];
            string VoiceCounter  = args[6];
            string VoiceDead = args[7];
            string DropItem = args[8];
            string Weakness = args[9];

            string base64String;
            using (var inFile = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
            {
                var bs = new byte[inFile.Length];
                int readBytes = inFile.Read(bs, 0, (int)inFile.Length);
                base64String = Convert.ToBase64String(bs);
                Console.WriteLine("アイコンをBase64に変換しました");
            }

            var boss = new Boss();
            await boss.InitializeAsync();
            Console.WriteLine("Mastodonに接続しました");

            string ext = Path.GetExtension(ImagePath).Replace(".", "");
            await boss.MastodonClient.UpdateCredentials(avatar: $"data:image/{ext};base64,{base64String}");
            Console.WriteLine("プロフィールを更新しました");

            boss.Name = Name;
            boss.ImagePath = ImagePath;
            boss.LifePoint = LifePoint;
            boss.EvadeRate = EvadeRate;
            boss.VoiceAppear = VoiceAppear;
            boss.VoiceDamage = VoiceDamage;
            boss.VoiceCounter = VoiceCounter;
            boss.VoiceDead = VoiceDead;
            boss.Weakness = Weakness;
            boss.DropItem = DropItem;

            var battle = new Battle(boss);
            await battle.Start();
        }
    }
}
