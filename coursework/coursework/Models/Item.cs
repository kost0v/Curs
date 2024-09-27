using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace coursework.Models
{
    [Table("Items")]
    public class Item
    {
        [Key]
        [Column("iditems")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("stock_quantity")]
        public int Stock { get; set; }

        [Column("image_id")]
        public string ImagePath { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }
    }
}
