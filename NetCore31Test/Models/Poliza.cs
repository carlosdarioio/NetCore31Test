using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NetCore31Test.Models
{
    public partial class Poliza
    {        
        public int Id { get; set; }
        //[Required]
        public int ClienteId { get; set; }
        //[Required]
        public string Mondeda { get; set; }
        //[Required]
        public decimal SumaAsegurada { get; set; }
    }
}
