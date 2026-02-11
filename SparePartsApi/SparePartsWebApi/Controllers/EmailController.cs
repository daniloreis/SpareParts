using Application;
using Domain;
using Infrastructure;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace SparePartsWebApi.Controllers
{
    public class EmailController : ApiController
    {
        private EmailService emailService;

        public EmailController(EmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpGet]
        public async Task<IHttpActionResult> ConsumirFilaEmail()
        {
            try
            {
                await Task.Run(() => emailService.ConsumirFilaEmail());

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetInnerExceptionMessage());
            }
        }

    }
}