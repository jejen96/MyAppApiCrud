using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace MyApiAppCrud.Models
{
    public class SalesOrder{

        public DateTime OrderDate { get; set; }
        [Key]
        public string SalesOrderNo { get; set; }
        public string CustCode { get; set; }
    }
}