using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Data.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        // public void Update(Product product) => dbContext.Update(product);

        // product update - explicit
        public void Update(Product obj)
        {
            var product = dbContext.Products.FirstOrDefault(p => p.Id == obj.Id);
            if (product != null)
            {
                product.Title = obj.Title;
                product.ISBN = obj.ISBN;
                product.Price = obj.Price;
                product.Price50 = obj.Price50;
                product.ListPrice = obj.ListPrice;
                product.Price100 = obj.Price100;
                product.Description = obj.Description;
                product.CategoryId = obj.CategoryId;
                product.Author = obj.Author;

                if (obj.ImageUrl != null)
                {
                    product.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
