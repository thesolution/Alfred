using System;
using System.Configuration;
using Bot;
using Bot.Tasks;
using Topshelf;
using System.Threading.Tasks;

namespace BotConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(hf =>
            {
                hf.Service<BotService>();
                hf.RunAsLocalSystem();

            });
        }
    }

    public class BotService : ServiceControl
    {
               
        private IrcBot bot;
        
        public BotService(){
            var config = CreateConfiguration();
            bot = new IrcBot(config);   
            var uriString = ConfigurationManager.AppSettings["feed"];
            var defaultElb = ConfigurationManager.AppSettings["defaultElb"];
            if (!string.IsNullOrWhiteSpace(uriString))
                bot.AddTask(new IrcTeamCityBuildStatusTask(new Uri(uriString)));
            if (!string.IsNullOrWhiteSpace(defaultElb))
                bot.AddTask(new IrcElbStatusTask(defaultElb));

            
        }
        
        private IrcBotConfiguration CreateConfiguration()
        {
            return new IrcBotConfiguration
            {
                NickName = ConfigurationManager.AppSettings["nick"],
                HostName = ConfigurationManager.AppSettings["host"],
                Port = int.Parse(ConfigurationManager.AppSettings["port"]),
                Channel = ConfigurationManager.AppSettings["channel"]
            };
        }



        public bool Start(HostControl hostControl)
        {
            Task.Run(() => {
                bot.Run().Wait();
                hostControl.Stop();
            });
            
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            bot.Stop();
            return true;
        }
    }
}
