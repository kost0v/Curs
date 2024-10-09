using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace coursework.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [Column("idorderitems")]
        public int Id { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        [JsonIgnore]
        public Order Order { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item Item { get; set; }  // Навигационное свойство для связи с Item

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; } // Цена за единицу товара на момент покупки
    }
}