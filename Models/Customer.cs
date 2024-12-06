using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApiAppCrud.Models
{
    public class Customer{
        public string CustId {get; set;}
        public string CustName {get; set;}
    }
}