using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using coursework.Models;
using System.Collections.Generic;

namespace coursework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ShopContext _context;

        public OrdersController(ShopContext context)
        {
            _context = context;
        }

        // GET: api/Orders/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }


        // PUT: api/Orders/{orderId}/status
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound("Заказ не найден");

            if (order.Status == "выполнен" || order.Status == "отменен")
                return BadRequest("Нельзя изменить статус выполненного или отмененного заказа");

            if (newStatus == "отменен")
            {
                // Возвращаем товары на склад
                foreach (var orderItem in order.OrderItems)
                {
                    var item = await _context.Items.FindAsync(orderItem.ItemId);
                    if (item != null)
                    {
                        item.Stock += orderItem.Quantity;
                    }
                }
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус заказа обновлен", status = order.Status });
        }
    }
}
