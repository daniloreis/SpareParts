    
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class  ContactService : ServiceBase<Contact> , IContactService
    {
        private readonly  Contact  contact;

        public ContactService(Contact contact) : base(contact)
        {
            this.contact = contact;
        }
 
    }
}


