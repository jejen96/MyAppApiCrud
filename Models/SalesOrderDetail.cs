using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiAppCrud.Models
{
    public class SalesOrderDetail{
        [Key]
        public string SalesOrderNo { get; set; }
        [Key]
        public int Qty { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }
    }
}