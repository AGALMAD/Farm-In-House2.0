using Server.DTOs;
using Server.Models;

namespace Server.Mappers;

public class ProductMapper
{

    public ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Average = product.Average,
            Category = product.Category,
            CategoryId = product.CategoryId,
            Description = product.Description,
            Images = product.Images,
            Price = product.Price,
            Reviews = product.Reviews,
            Stock = product.Stock
        };
    }

    public IEnumerable<ProductDto> ToDto(IEnumerable<Product> products)
    {
        return products.Select(ToDto);
    }

    public Product ToEntity(ProductDto productDto)
    {
        return new Product
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Average = productDto.Average,
            Category = productDto.Category,
            CategoryId = productDto.CategoryId,
            Description = productDto.Description,
            Images = productDto.Images,
            Price = productDto.Price,
            Reviews = productDto.Reviews,
            Stock = productDto.Stock
        };
    }

    public Product ToEntity(ProductToInsert productDto)
    {
        return new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = Int64.Parse(productDto.Price),
            Stock = Int32.Parse(productDto.Stock)
        };
    }

    public IEnumerable<Product> ToEntity(IEnumerable<ProductDto> productsDto)
    {
        return productsDto.Select(ToEntity);
    }
}
