using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace MyApiAppCrud.Models
{
    public class Pricing{
        public string ProductCode { get; set; }
        
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        public DateTime PriceValidateFrom { get; set; }
        public DateTime PriceValidateTo { get; set; }
    }
}