
using FamilyPillsAPI.Data;
using FamilyPillsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace FamilyPillsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Configure DbContext with MySQL
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<FamilyPillsDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                )
            );

            var jwtSecret = builder.Configuration["Jwt:Secret"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException("Jwt:Secret is not configured.");
            }

            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Bearer token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();
            EnsureDatabaseSchema(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }

        private static void EnsureDatabaseSchema(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FamilyPillsDbContext>();

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS users (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    email VARCHAR(255) UNIQUE NOT NULL,
                    password VARCHAR(255) NOT NULL,
                    full_name VARCHAR(255),
                    last_login TIMESTAMP NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS refresh_tokens (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    token_hash VARCHAR(255) NOT NULL,
                    expires_at TIMESTAMP NOT NULL,
                    revoked_at TIMESTAMP NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT fk_refresh_tokens_user
                        FOREIGN KEY (user_id) REFERENCES users(id)
                        ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS medicines (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NULL,
                    name VARCHAR(255) NOT NULL,
                    barcode VARCHAR(100),
                    total_quantity INT NOT NULL DEFAULT 0,
                    unit VARCHAR(50),
                    expiry_date VARCHAR(50),
                    image_path VARCHAR(500),
                    quantity VARCHAR(100),
                    last_updated VARCHAR(50),
                    is_running_low BOOLEAN DEFAULT false,
                    is_expired BOOLEAN DEFAULT false,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    CONSTRAINT fk_medicines_user
                        FOREIGN KEY (user_id) REFERENCES users(id)
                        ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS medicine_images (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NULL,
                    medicine_id INT NULL,
                    image_path VARCHAR(500) NOT NULL,
                    file_name VARCHAR(255) NOT NULL,
                    file_size BIGINT NOT NULL DEFAULT 0,
                    content_type VARCHAR(100),
                    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT fk_medicine_images_user
                        FOREIGN KEY (user_id) REFERENCES users(id)
                        ON DELETE SET NULL,
                    CONSTRAINT fk_medicine_images_medicine
                        FOREIGN KEY (medicine_id) REFERENCES medicines(id)
                        ON DELETE SET NULL
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS medicine_inventory_logs (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NULL,
                    medicine_id INT NOT NULL,
                    change_amount INT NOT NULL,
                    quantity_before INT NULL,
                    quantity_after INT NULL,
                    reason VARCHAR(255),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT fk_inventory_logs_user
                        FOREIGN KEY (user_id) REFERENCES users(id)
                        ON DELETE SET NULL,
                    CONSTRAINT fk_inventory_logs_medicine
                        FOREIGN KEY (medicine_id) REFERENCES medicines(id)
                        ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS user_settings (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL UNIQUE,
                    low_stock_threshold INT NOT NULL DEFAULT 10,
                    expiry_warning_days INT NOT NULL DEFAULT 30,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    CONSTRAINT fk_user_settings_user
                        FOREIGN KEY (user_id) REFERENCES users(id)
                        ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);
        }
    }
}
