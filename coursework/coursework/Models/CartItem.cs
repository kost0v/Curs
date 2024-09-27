using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace coursework.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        [Column("idcartitems")]
        public int Id { get; set; }

        [Required]
        [Column("cart_id")]
        public int CartId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }
    }
}
