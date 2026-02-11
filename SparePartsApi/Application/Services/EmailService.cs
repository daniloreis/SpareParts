using Domain;
using Infrastructure;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
//using Microsoft.Extensions.Configuration;

namespace Application
{
    public class EmailService : ServiceBase<EmailEnviado>
    {
        private readonly EmailEnviadoRepository repository;

        //private IConfiguration configuration;

        public EmailService(EmailEnviadoRepository repository) : base(repository)
        {
            this.repository = repository;
        }

        public void EnviarEmailParaFila(string destinatario, string assunto, string mensagem, string cc)
        {
            try
            {
                var email = new EmailEnviado() { Assunto = assunto, Destinatario = destinatario, Mensagem = mensagem, DataHoraCriado = DateTime.Now, EmailsCC = cc };

                repository.Add(email);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConsumirFilaEmail()
        {
            try
            {
                var pendentes = repository.FindByCriteria(x => !x.DataHoraEnviado.HasValue).ToList();

                foreach (var email in pendentes)
                {
                    try
                    {
                        EnviarEmail(email.Destinatario, email.Assunto, email.Mensagem, email.EmailsCC.Split(';'));

                        email.DataHoraEnviado = DateTime.Now;
                        email.Erro = "";
                    }
                    catch (Exception ex)
                    {
                        email.Erro = DateTime.Now + ": " + ex.GetInnerExceptionMessage();
                    }
                    finally
                    {
                        repository.Update(email);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EnviarEmail(string destinatario, string assunto, string mensagem, string[] cc)
        {
            try
            {
                string emailSender = ConfigurationManager.AppSettings["EmailSender"].ToString();
                string emailName = ConfigurationManager.AppSettings["EmailName"].ToString();
                string emailServer = ConfigurationManager.AppSettings["EmailServer"].ToString();


                var email = new MailMessage();
                email.From = new MailAddress(emailSender, emailName);
                email.To.Add(destinatario);

                foreach (var item in cc)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        email.CC.Add(item);
                }

                email.Priority = MailPriority.Normal;
                email.IsBodyHtml = true;
                email.Subject = assunto;
                email.Body = mensagem;
                email.SubjectEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                email.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");

                var smtp = new SmtpClient(emailServer, 25);

                smtp.Send(email);

                email.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
