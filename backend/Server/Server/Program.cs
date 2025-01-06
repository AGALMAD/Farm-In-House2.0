using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Mappers;
using Server.Models;
using Server.Services;
using Server.Services.Blockchain;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configuramos cultura invariante para que al pasar los decimales a texto no tengan comas
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var builder = WebApplication.CreateBuilder(args);

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<Settings>>().Value);

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value);

            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // CONFIGURANDO JWT
            builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,

                        // INDICAMOS LA CLAVE
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Para el modelo de IA
            builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
                .FromFile("IAFarminhouse.mlnet");

            builder.Services.AddScoped<FarminhouseContext>();
            builder.Services.AddScoped<UnitOfWork>();

            /*builder.Services.AddScoped<FarminhouseContext>();
            builder.Services.AddScoped<UnitOfWork>();*/
            builder.Services.AddScoped<UserMapper>();

            builder.Services.AddScoped<ProductMapper>();
            builder.Services.AddScoped<SmartSearchService>();
            builder.Services.AddScoped<ShoppingCartMapper>();
            builder.Services.AddScoped<ShoppingCartService>();
            builder.Services.AddScoped<ReviewService>();
            builder.Services.AddScoped<TemporalOrderMapper>();
            builder.Services.AddScoped<TemporalOrderService>();
            builder.Services.AddScoped<CartContentMapper>();
            builder.Services.AddScoped<BlockchainService>();
            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<ProductsToBuyMapper>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<OrderMapper>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<ImageService>();

            //builder.Services.AddHostedService<CleanTemporalOrdersService>();
            // Aqui esta la clave privada
            Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:Key"];

            builder.Services.AddHostedService<CleanTemporalOrdersService>();


            // Permite CORS
            builder.Services.AddCors(
                options =>
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                        ;
                    })
                );


            var app = builder.Build();

            //PA QUE FUNCIONE EL WWWROOT NO LO TOQUEIS HIJOS DE PUTA
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
            });

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            // Permite CORS
            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (IServiceScope scope = app.Services.CreateScope())
            {
                FarminhouseContext dbContext = scope.ServiceProvider.GetService<FarminhouseContext>();
                if (dbContext.Database.EnsureCreated())
                {
                    // Categorías
                    Category fruitsCategory = new Category { Name = "fruits" };
                    Category vegetablesCategory = new Category { Name = "vegetables" };
                    Category meatCategory = new Category { Name = "meat" };

                    // Frutas
                    var fruits = new List<Product>
                    {
                        new Product
                        {
                            Name = "Naranja",
                            Description = "Naranja ecológica, rica en vitamina C, perfecta para zumos y postres.",
                            Price = 300,
                            Stock = 0,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/naranja1.jpg",
                                "/images/naranja2.jpg",
                                "/images/naranja3.jpg",
                                "/images/naranja4.jpg",
                                "/images/naranja5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Mandarina",
                            Description = "Mandarina ecológica, dulce y fácil de pelar, ideal para snacks.",
                            Price = 250,
                            Stock = 200,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/mandarina1.jpg",
                                "/images/mandarina2.jpg",
                                "/images/mandarina3.jpg",
                                "/images/mandarina4.jpg",
                                "/images/mandarina5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Limón",
                            Description = "Limón ecológico, versátil y refrescante, excelente para bebidas y aderezos.",
                            Price = 200,
                            Stock = 180,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/limon1.jpg",
                                "/images/limon2.jpg",
                                "/images/limon3.jpg",
                                "/images/limon4.jpg",
                                "/images/limon5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Fresa",
                            Description = "Fresa ecológica, jugosa y dulce, perfecta para postres y batidos.",
                            Price = 400,
                            Stock = 0,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/fresa1.jpg",
                                "/images/fresa2.jpg",
                                "/images/fresa3.jpg",
                                "/images/fresa4.jpg",
                                "/images/fresa5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Melón",
                            Description = "Melón ecológico, refrescante y dulce, ideal para el verano.",
                            Price = 450,
                            Stock = 90,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/melon1.jpg",
                                "/images/melon2.jpg",
                                "/images/melon3.jpg",
                                "/images/melon4.jpg",
                                "/images/melon5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Sandía",
                            Description = "Sandía ecológica, hidratante y deliciosa, perfecta para días calurosos.",
                            Price = 600,
                            Stock = 75,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/sandia1.jpg",
                                "/images/sandia2.jpg",
                                "/images/sandia3.jpg",
                                "/images/sandia4.jpg",
                                "/images/sandia5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Manzana",
                            Description = "Manzana ecológica, crujiente y nutritiva, excelente para comer cruda o en postres.",
                            Price = 250,
                            Stock = 160,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/manzana1.jpg",
                                "/images/manzana2.jpg",
                                "/images/manzana3.jpg",
                                "/images/manzana4.jpg",
                                "/images/manzana5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Pera",
                            Description = "Pera ecológica, dulce y jugosa, ideal para snacks y postres.",
                            Price = 280,
                            Stock = 140,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/pera1.jpg",
                                "/images/pera2.jpg",
                                "/images/pera3.jpg",
                                "/images/pera4.jpg",
                                "/images/pera5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Uva",
                            Description = "Uva ecológica, dulce y versátil, perfecta para comer sola o en ensaladas.",
                            Price = 500,
                            Stock = 130,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/uva1.jpg",
                                "/images/uva2.jpg",
                                "/images/uva3.jpg",
                                "/images/uva4.jpg",
                                "/images/uva5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        },
                        new Product
                        {
                            Name = "Granada",
                            Description = "Granada ecológica, rica en antioxidantes, excelente para ensaladas y zumos.",
                            Price = 320,
                            Stock = 110,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/granada1.jpg",
                                "/images/granada2.jpg",
                                "/images/granada3.jpg",
                                "/images/granada4.jpg",
                                "/images/granada5.jpg"
                            },
                            CategoryId = 1,
                            Category = fruitsCategory
                        }
                    };


                    // Verduras
                    var vegetables = new List<Product>
                    {
                        new Product
                        {
                            Name = "Tomate",
                            Description = "Tomate ecológico, jugoso y lleno de sabor, ideal para ensaladas y salsas.",
                            Price = 200,
                            Stock = 150,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/tomate1.jpg",
                                "/images/tomate2.jpg",
                                "/images/tomate3.jpg",
                                "/images/tomate4.jpg",
                                "/images/tomate5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Pimiento verde",
                            Description = "Pimiento verde ecológico, perfecto para ensaladas y guisos.",
                            Price = 180,
                            Stock = 180,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/pimiento_verde1.jpg",
                                "/images/pimiento_verde2.jpg",
                                "/images/pimiento_verde3.jpg",
                                "/images/pimiento_verde4.jpg",
                                "/images/pimiento_verde5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Zanahoria",
                            Description = "Zanahoria ecológica, dulce y crujiente, rica en vitamina A.",
                            Price = 150,
                            Stock = 200,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/zanahoria1.jpg",
                                "/images/zanahoria2.jpg",
                                "/images/zanahoria3.jpg",
                                "/images/zanahoria4.jpg",
                                "/images/zanahoria5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Brócoli",
                            Description = "Brócoli ecológico, lleno de nutrientes, ideal para cocinar al vapor.",
                            Price = 220,
                            Stock = 170,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/brocoli1.jpg",
                                "/images/brocoli2.jpg",
                                "/images/brocoli3.jpg",
                                "/images/brocoli4.jpg",
                                "/images/brocoli5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Judías verdes",
                            Description = "Judías verdes ecológicas, altas en fibra y bajas en calorías.",
                            Price = 190,
                            Stock = 160,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/judias_verdes1.jpg",
                                "/images/judias_verdes2.jpg",
                                "/images/judias_verdes3.jpg",
                                "/images/judias_verdes4.jpg",
                                "/images/judias_verdes5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Acelga",
                            Description = "Acelga ecológica, rica en vitaminas y minerales, ideal para guisos.",
                            Price = 170,
                            Stock = 140,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/acelga1.jpg",
                                "/images/acelga2.jpg",
                                "/images/acelga3.jpg",
                                "/images/acelga4.jpg",
                                "/images/acelga5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Nabo",
                            Description = "Nabo ecológico, bajo en calorías y alto en fibra, perfecto para sopas.",
                            Price = 160,
                            Stock = 130,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/nabo1.jpg",
                                "/images/nabo2.jpg",
                                "/images/nabo3.jpg",
                                "/images/nabo4.jpg",
                                "/images/nabo5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Coles de Bruselas",
                            Description = "Coles de Bruselas ecológicas, ricas en antioxidantes.",
                            Price = 210,
                            Stock = 120,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/coles_bruselas1.jpg",
                                "/images/coles_bruselas2.jpg",
                                "/images/coles_bruselas3.jpg",
                                "/images/coles_bruselas4.jpg",
                                "/images/coles_bruselas5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Pepino",
                            Description = "Pepino ecológico, fresco y crujiente, ideal para ensaladas.",
                            Price = 140,
                            Stock = 110,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/pepino1.jpg",
                                "/images/pepino2.jpg",
                                "/images/pepino3.jpg",
                                "/images/pepino4.jpg",
                                "/images/pepino5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        },
                        new Product
                        {
                            Name = "Cebolla",
                            Description = "Cebolla ecológica, rica en antioxidantes, fundamental en la cocina mediterránea.",
                            Price = 160,
                            Stock = 100,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/cebolla1.jpg",
                                "/images/cebolla2.jpg",
                                "/images/cebolla3.jpg",
                                "/images/cebolla4.jpg",
                                "/images/cebolla5.jpg"
                            },
                            CategoryId = 2,
                            Category = vegetablesCategory
                        }
                    };


                    // Carnes
                    var meats = new List<Product>
                    {
                        new Product
                        {
                            Name = "Pollo",
                            Description = "Pollo ecológico, criado sin antibióticos, ideal para asar o guisar.",
                            Price = 500,
                            Stock = 100,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/pollo1.jpg",
                                "/images/pollo2.jpg",
                                "/images/pollo3.jpg",
                                "/images/pollo4.jpg",
                                "/images/pollo5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Chuletón De Ternera",
                            Description = "Ternera ecológica, rica en proteínas, perfecta para parrillas y guisos.",
                            Price = 800,
                            Stock = 80,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/chuleton_ternera1.jpg",
                                "/images/ternera2.jpg",
                                "/images/ternera3.jpg",
                                "/images/ternera4.jpg",
                                "/images/ternera5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Cinta De Lomo",
                            Description = "Cerdo ecológico, jugoso y sabroso, ideal para asar o freír.",
                            Price = 600,
                            Stock = 90,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/cinta_lomo1.jpg",
                                "/images/cerdo2.jpg",
                                "/images/cerdo3.jpg",
                                "/images/cerdo4.jpg",
                                "/images/cerdo5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Chuletas De Cordero",
                            Description = "Cordero ecológico, tierno y delicioso, perfecto para asados.",
                            Price = 900,
                            Stock = 70,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/chuleta_cordero1.jpg",
                                "/images/cordero2.jpg",
                                "/images/cordero3.jpg",
                                "/images/cordero4.jpg",
                                "/images/cordero5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Pavo",
                            Description = "Pavo ecológico, bajo en grasa y alto en proteínas, ideal para dietas saludables.",
                            Price = 550,
                            Stock = 110,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/pavo1.jpg",
                                "/images/pavo2.jpg",
                                "/images/pavo3.jpg",
                                "/images/pavo4.jpg",
                                "/images/pavo5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Conejo",
                            Description = "Conejo ecológico, carne magra y nutritiva, perfecta para guisos.",
                            Price = 700,
                            Stock = 60,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/conejo1.jpg",
                                "/images/conejo2.jpg",
                                "/images/conejo3.jpg",
                                "/images/conejo4.jpg",
                                "/images/conejo5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Entrecot De Buey",
                            Description = "Buey ecológico, carne tierna y sabrosa, ideal para parrillas.",
                            Price = 1000,
                            Stock = 50,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/entrecot-buey1.jpg",
                                "/images/buey2.jpg",
                                "/images/buey3.jpg",
                                "/images/buey4.jpg",
                                "/images/buey5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Jabalí",
                            Description = "Jabalí ecológico, carne exótica y rica en sabor, perfecta para estofados.",
                            Price = 1200,
                            Stock = 40,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/jabali1.jpg",
                                "/images/jabali2.jpg",
                                "/images/jabali3.jpg",
                                "/images/jabali4.jpg",
                                "/images/jabali5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Pato",
                            Description = "Pato ecológico, carne jugosa y rica en sabor, ideal para asados.",
                            Price = 750,
                            Stock = 55,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/pato1.jpg",
                                "/images/pato2.jpg",
                                "/images/pato3.jpg",
                                "/images/pato4.jpg",
                                "/images/pato5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        },
                        new Product
                        {
                            Name = "Codorniz",
                            Description = "Codorniz ecológica, carne tierna y delicada, perfecta para platos gourmet.",
                            Price = 650,
                            Stock = 65,
                            Average = 0,
                            Images = new List<string>
                            {
                                "/images/codorniz1.jpg",
                                "/images/codorniz2.jpg",
                                "/images/codorniz3.jpg",
                                "/images/codorniz4.jpg",
                                "/images/codorniz5.jpg"
                            },
                            CategoryId = 3,
                            Category = meatCategory
                        }
                    };




                    // Añadir categorías y productos al contexto de la base de datos
                    dbContext.Categories.Add(fruitsCategory);
                    dbContext.Categories.Add(vegetablesCategory);
                    dbContext.Categories.Add(meatCategory);
                    dbContext.Products.AddRange(fruits);
                    dbContext.Products.AddRange(vegetables);
                    dbContext.Products.AddRange(meats);

                    PasswordService passwordService = new PasswordService();
                    // Crear usuarios de ejemplo
                    var user = new User { Name = builder.Configuration["AdminUser:Name"], Email = builder.Configuration["AdminUser:Email"], Password = passwordService.Hash(builder.Configuration["AdminUser:Password"]), Role = builder.Configuration["AdminUser:Role"], Address = builder.Configuration["AdminUser:Address"] };
                    // Asegurarse de que los usuarios están añadidos al contexto
                    dbContext.Users.Add(user);

                    dbContext.SaveChanges();

                    dbContext.ShoppingCart.Add(new ShoppingCart() { UserId = 1 });

                    dbContext.SaveChanges();
                }
            }


            app.Run();
        }
    }
}
