using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace coursework.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("idorders")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "обрабатывается"; // Статусы: обрабатывается, выполнен, отменен

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}