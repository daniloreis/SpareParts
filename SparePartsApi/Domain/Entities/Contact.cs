    
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Contact : EntityBase<Contact>
    {
		private IContactRepository repo;

		public Contact(IContactRepository repo) : base(repo)
		{
			this.repo = repo;
		}

		public Contact()
		{
			 
		}

        [Key]
			public Int32? ContactId { get; set; }

        			public String Email { get; set; }

        			public String FirstName { get; set; }

        			public String LastName { get; set; }

        			public String Phone { get; set; }

        		
    }              
}
