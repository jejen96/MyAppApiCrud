using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiAppCrud.Models
{
    public class Product{
        [Key]
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}