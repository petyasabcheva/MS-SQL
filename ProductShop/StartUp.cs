using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private static string ResultDirectoryPath = "../../../Datasets/Results";
        public static void Main(string[] args)
        {
            var db = new ProductShopContext();
            //var inputJson = File.ReadAllText("../../../Datasets/categories-products.json");
            //string result = ImportCategoryProducts(db, inputJson);
            //Console.WriteLine(result);
            var json = GetSoldProducts(db);
            if (!Directory.Exists(ResultDirectoryPath))
            {
                Directory.CreateDirectory(ResultDirectoryPath);
            }
            File.WriteAllText(ResultDirectoryPath + "/users-sold-products.json", json);
        }

        private static void ResetDatabase(ProductShopContext db)
        {
            db.Database.EnsureDeleted();
            Console.WriteLine("Database was successfully deleted");
            db.Database.EnsureCreated();
            Console.WriteLine("Database was successfully created");

        }
        //Problem 02 Import Users
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<User[]>(inputJson);
            context.Users.AddRange(users);
            context.SaveChanges();
            return $"Successfully imported {users.Length}";
        }
        //Problem 03 Import Products
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);
            context.Products.AddRange(products);
            context.SaveChanges();
            return $"Successfully imported {products.Length}";
        }
        //Problem 04 Import Categories
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<Category[]>(inputJson).Where(c => c.Name != null)
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();
            return $"Successfully imported {categories.Length}";
        }

        //Problem 05 Import Categories and Products
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoryProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);
            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();
            return $"Successfully imported {categoryProducts.Length}";

        }
        //Problem 06 Export Products in Range
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = p.Seller.FirstName + " " + p.Seller.LastName
                })
                .ToArray();
            var json = JsonConvert.SerializeObject(products, Formatting.Indented);
            return json;
        }
        //Problem 06 Query 6. Export Successfully Sold Products
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u=>u.ProductsSold.Any(p=>p.Buyer!=null))
                .OrderBy(u=>u.LastName)
                .ThenBy(u=>u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName=u.LastName,
                    soldProducts= u.ProductsSold.Where(p=>p.Buyer!=null)
                        .Select(p=>new
                        {
                            name=p.Name,
                            price=p.Price,
                            buyerFirstName=p.Buyer.FirstName,
                            buyerLastName=p.Buyer.LastName,
                        }).ToArray()
                })
                .ToArray();
            var json = JsonConvert.SerializeObject(users, Formatting.Indented);
            return json;
        }

    }
}