using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Product.Base.Models
{
    /// <summary>
    /// Represents a product in the system. A product could be a Bank Account, Credit Card, or others.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    [Table("Products")]
    public class Product
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        /// <value>The unique identifier of the product.</value>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the product, e.g., Bank Account or Credit Card.
        /// </summary>
        /// <value>The type of the product.</value>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        [Required]
        public Type ProductType { get; set; }

        /// <summary>
        /// Gets or sets the date when the product was created.
        /// </summary>
        /// <value>The creation date of the product.</value>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date when the product was last updated.
        /// </summary>
        /// <value>The last updated date of the product.</value>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets a value indicating whether the product is deleted.
        /// </summary>
        /// <value>A boolean value indicating if the product is deleted.</value>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        [Required]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the Product class.
        /// </summary>
        /// <param name="name">The name of the product.</param>
        /// <param name="productType">The type of the product.</param>
        /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
        public Product(string name, Type productType)
        {
            Id = GuuidGenerator.GenerateHash(); // Assuming this method generates a unique ID
            Name = name;
            ProductType = productType;
        }

        /// <summary>
        /// Enum representing the types of products available in the system.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Represents a bank account product.
            /// </summary>
            BankAccount,

            /// <summary>
            /// Represents a credit card product.
            /// </summary>
            CreditCard,
        }
    }
}