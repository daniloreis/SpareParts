using System;
using System.Configuration;
using System.Net.Http;
using System.ServiceProcess;
using System.Timers;

namespace WindowsServiceAtendimento
{
    public partial class Service1 : ServiceBase
    {
        string domain = Convert.ToString(ConfigurationManager.AppSettings["domain"]);
        Timer timer = new Timer(TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["intervaloMin"])).TotalMilliseconds);

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(Execute);
            timer.Start();
        }

        public void Execute(object sender, ElapsedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient() { BaseAddress = new Uri($"http://{domain}/") };

                HttpResponseMessage response = client.GetAsync($"/api/Atendimento/AtenderPendentes").Result;
                if (response.IsSuccessStatusCode)
                {
                    var dto = response.Content.ReadAsStringAsync().Result;
                }

                HttpResponseMessage response2 = client.GetAsync($"/api/Email/ConsumirFilaEmail").Result;
                if (response2.IsSuccessStatusCode)
                {
                    var dto = response2.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                //File.AppendAllText("log.txt", Environment.NewLine + ex.Message);
            }
        }

        protected override void OnStop()
        {
            timer.Stop();
        }
    }
}
