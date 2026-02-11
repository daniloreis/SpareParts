using SparePartsApp.Helpers;
using SparePartsApp.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;


namespace SparePartsApp
{
    public static class Helper
    {
        public static string Server { get; set; } = ""; 

        public static List<Tuple<string, string>> Servidores
        {
            get
            {
                var lista = new List<Tuple<string, string>>();

#if DEBUG
                lista.Add(new Tuple<string, string>("DEV", "10.1.12.108/api"));
#endif

                lista.Add(new Tuple<string, string>("QAT", "10.1.0.14/api"));
                lista.Add(new Tuple<string, string>("PRD", "10.1.0.119/api"));

                return lista;
            }
        }

        public static Recebimento ListaRecebimento { get; set; }

        public static bool HasWifi
        {
            get
            {
                IConnectionProvider connectionProvider = DependencyService.Get<IConnectionProvider>();

                return connectionProvider.CheckWifi();
            }
        }

        public static bool HasWebServerAvaiable(string checkUrl)
        {
            IConnectionProvider connectionProvider = DependencyService.Get<IConnectionProvider>();

            return connectionProvider.HasWebServerAvaiable(checkUrl);
        }

        public static ObservableRangeCollection<Material> MateriaisPendentes { get; set; } = new ObservableRangeCollection<Material>();

        public static string UsuarioAutenticado { get; internal set; }

        /// <summary>
        /// Pega a última mensagem de exceção.
        /// </summary>
        public static string GetInnerExceptionMessage(this Exception ex)
        {
            string msg = string.Empty;

            if (ex.InnerException != null)
            {
                msg = ex.InnerException.GetInnerExceptionMessage();
            }
            else
                return ex.Message;

            return msg;
        }

    }
}