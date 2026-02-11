using Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Application
{
    public class ImpressaoService
    {
        /// <summary>
        /// Enviar etiquetas em formato ZPL
        /// </summary>
        public void Imprimir(IEnumerable<string> etiquetas)
        {
            try
            {
                string ipAddress = ConfigurationManager.AppSettings["PRINTER"].ToString();
                int port = 9100;

                TcpClient client = new TcpClient();
                client.Connect(ipAddress, port);

                StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.Default);

                foreach (var etiqueta in etiquetas)
                {
                    writer.Write(etiqueta);
                    writer.Flush();
                };

                writer.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na impressão: " + ex.GetInnerExceptionMessage());
            }

        }
    }
}
