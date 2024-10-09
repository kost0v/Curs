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
    public class CartController : ControllerBase
    {
        private readonly ShopContext _context;

        public CartController(ShopContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCartItems(int userId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return Ok(new List<object>());
            }

            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            var result = new List<object>();

            foreach (var ci in cartItems)
            {
                var item = await _context.Items.FindAsync(ci.ItemId);
                result.Add(new
                {
                    ci.Id,
                    ci.CartId,
                    ci.ItemId,
                    ci.Quantity,
                    item
                });
            }

            return Ok(result);
        }

        [HttpPost("{itemId}")]
        public async Task<IActionResult> AddToCart(int itemId, [FromHeader] string Authorization)
        {
            if (string.IsNullOrEmpty(Authorization))
                return Unauthorized();

            var userId = int.Parse(Authorization.Replace("Bearer ", ""));
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ItemId == itemId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem { CartId = cart.Id, ItemId = itemId, Quantity = 1 };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok("Товар добавлен в корзину");
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveFromCart(int itemId, [FromHeader] string Authorization)
        {
            if (string.IsNullOrEmpty(Authorization))
                return Unauthorized();

            var userId = int.Parse(Authorization.Replace("Bearer ", ""));
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Корзина не найдена");

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ItemId == itemId);

            if (cartItem == null)
                return NotFound("Товар не найден в корзине");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok("Товар удален из корзины");
        }

        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Корзина не найдена");

            var cartItems = _context.CartItems.Where(ci => ci.CartId == cart.Id);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok("Корзина очищена");
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromHeader] string Authorization)
        {
            if (string.IsNullOrEmpty(Authorization))
                return Unauthorized();

            var userId = int.Parse(Authorization.Replace("Bearer ", ""));
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return Ok(new { success = false, message = "Корзина не найдена" });
            }

            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return Ok(new { success = false, message = "Корзина пуста" });
            }

            decimal totalPrice = 0;
            foreach (var cartItem in cartItems)
            {
                var item = await _context.Items.FindAsync(cartItem.ItemId);
                if (item == null || item.Stock < cartItem.Quantity)
                {
                    return Ok(new { success = false, message = $"Недостаточно товара {item?.Name ?? "Неизвестный товар"} на складе" });
                }

                totalPrice += item.Price * cartItem.Quantity;
            }

            // Создание заказа
            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                Status = "обрабатывается",
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cartItems)
            {
                var item = await _context.Items.FindAsync(cartItem.ItemId);
                if (item != null)
                {
                    // Уменьшаем количество товара на складе
                    item.Stock -= cartItem.Quantity;

                    // Добавляем товар в заказ
                    var orderItem = new OrderItem
                    {
                        ItemId = item.Id,
                        Quantity = cartItem.Quantity,
                        Price = item.Price
                    };
                    order.OrderItems.Add(orderItem);
                }
            }

            _context.Orders.Add(order);

            // Очищаем корзину
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Заказ успешно оформлен", orderId = order.Id });
        }
    }
}