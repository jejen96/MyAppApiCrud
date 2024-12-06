using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApiAppCrud.Data;
using MyApiAppCrud.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApiAppCrud.Controllers{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/products/insert
        [HttpPost("insert")]
        public async Task<IActionResult> InsertSalesOrder([FromBody] SalesOrderRequest request)
        {
            // 1. Validasi Customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustCode == request.CustId);

            if (customer == null)
            {
                return BadRequest(new { Status = "failed", Message = $"Customer dengan code {request.CustId} tidak ditemukan" });
            }

            // 2. Membuat SalesOrder
            var salesOrder = new SalesOrder
            {
                SalesOrderNo = "SO" + DateTime.Now.ToString("yyyyMMddHHmmss"), // Generate Sales Order No
                CustCode = request.CustId,
                OrderDate = DateTime.Now
            };

            // Mengambil harga yang sesuai dengan tanggal
            var price = await _context.Prices
                .Where(p => p.PriceValidationFrom <= DateTime.Now && p.PriceValidationTo >= DateTime.Now)
                .FirstOrDefaultAsync();

            if (price != null)
            {
                salesOrder.Price = price.PriceValue;
            }
            else
            {
                return BadRequest(new { Status = "failed", Message = "Harga tidak ditemukan untuk tanggal ini" });
            }

            // Menyimpan SalesOrder
            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();

            // 3. Menyimpan SalesOrderDetail
            foreach (var detail in request.OrderDetail)
            {
                // Validasi ProductCode
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductCode == detail.ProductCode);

                if (product == null)
                {
                    return BadRequest(new { Status = "failed", Message = $"Product dengan code {detail.ProductCode} tidak ditemukan" });
                }

                var salesOrderDetail = new SalesOrderDetail
                {
                    SalesOrderNo = salesOrder.SalesOrderNo,
                    ProductCode = detail.ProductCode,
                    Qty = detail.Qty,
                    Price = price.PriceValue
                };

                _context.SalesOrderDetails.Add(salesOrderDetail);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status = "success",
                SalesOrderNo = salesOrder.SalesOrderNo,
                Message = "Data has been inserted successfully"
            });
        }
    }


        
[HttpGet("view")]
        public async Task<IActionResult> GetSalesOrderDetails([FromQuery] string SalesOrderNo)
        {
            // Mencari SalesOrder berdasarkan SalesOrderNo
            var salesOrder = await _context.SalesOrders
                .Where(so => so.SalesOrderNo == SalesOrderNo)
                .FirstOrDefaultAsync();

            if (salesOrder == null)
            {
                return NotFound(new { Status = "failed", Message = $"No Order {SalesOrderNo} tidak ditemukan" });
            }

            // Mencari SalesOrderDetail berdasarkan SalesOrderNo
            var orderDetails = await _context.SalesOrderDetails
                .Where(so => so.SalesOrderNo == SalesOrderNo)
                .Select(so => new 
                {
                    so.ProductCode,
                    so.Qty
                })
                .ToListAsync();

            // Menyusun response sesuai dengan format yang diinginkan
            var response = new
            {
                SalesOrderNo = salesOrder.SalesOrderNo,
                CustId = salesOrder.CustCode,
                OrderDetail = orderDetails
            };

            return Ok(response);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateSalesOrder([FromBody] SalesOrderRequest request)
        {
            // 1. Mencari SalesOrder berdasarkan SalesOrderNo
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(so => so.SalesOrderNo == request.SalesOrderNo);

            if (salesOrder == null)
            {
                return NotFound(new { Status = "failed", Message = $"No Order {request.SalesOrderNo} tidak ditemukan" });
            }

            // 2. Validasi CustId - pastikan Customer ada di database
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustCode == request.CustId);

            if (customer == null)
            {
                return BadRequest(new { Status = "failed", Message = $"Customer dengan code {request.CustId} tidak ditemukan" });
            }

            // 3. Update SalesOrder (CustId, OrderDate, dll)
            salesOrder.CustCode = request.CustId;
            _context.SalesOrders.Update(salesOrder);
            await _context.SaveChangesAsync();

            // 4. Menghapus semua detail lama
            var existingDetails = await _context.SalesOrderDetails
                .Where(so => so.SalesOrderNo == request.SalesOrderNo)
                .ToListAsync();
            _context.SalesOrderDetails.RemoveRange(existingDetails);

            // 5. Menambahkan detail baru
            foreach (var detail in request.OrderDetail)
            {
                // Validasi ProductCode - pastikan product ada di database
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductCode == detail.ProductCode);

                if (product == null)
                {
                    return BadRequest(new { Status = "failed", Message = $"Product dengan code {detail.ProductCode} tidak ditemukan" });
                }

                var salesOrderDetail = new SalesOrderDetail
                {
                    SalesOrderNo = request.SalesOrderNo,
                    ProductCode = detail.ProductCode,
                    Qty = detail.Qty
                };

                _context.SalesOrderDetails.Add(salesOrderDetail);
            }

            // 6. Simpan perubahan pada SalesOrderDetails
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status = "success",
                Message = "Data has been Update successfully"
            });
        }
    
         [HttpDelete("delete")]
        public async Task<IActionResult> DeleteSalesOrder([FromBody] DeleteSalesOrderRequest request)
        {
            // 1. Mencari SalesOrder berdasarkan SalesOrderNo
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(so => so.SalesOrderNo == request.SalesOrderNo);

            if (salesOrder == null)
            {
                return NotFound(new { Status = "failed", Message = $"No Order {request.SalesOrderNo} tidak ditemukan" });
            }

            // 2. Menghapus SalesOrderDetails yang terkait dengan SalesOrderNo
            var salesOrderDetails = await _context.SalesOrderDetails
                .Where(so => so.SalesOrderNo == request.SalesOrderNo)
                .ToListAsync();

            _context.SalesOrderDetails.RemoveRange(salesOrderDetails);

            // 3. Menghapus SalesOrder itu sendiri
            _context.SalesOrders.Remove(salesOrder);

            // 4. Menyimpan perubahan ke database
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status = "success",
                Message = "Data has been delete successfully"
            });
        }
    
    }
}