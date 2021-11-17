using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCore31Test.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
/*
 //CLIENTES CONTROLLER FUNCIONAL VAS POR 3 Crear CRUD para tabla Póliza
 https://www.syncfusion.com/blogs/post/how-to-build-crud-rest-apis-with-asp-net-core-3-1-and-entity-framework-core-create-jwt-tokens-and-secure-apis.aspx
Create an empty API controller called TokenController.!!!
    
 alola x auqi vas
Create an empty API controller called TokenController.


que simple crera todo auto con ejecutando en la terminal
Scaffold-DbContext "Server=DESKTOP-1ULHC37;
Database=Inventory;Integrated Security=True" 
Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
En un alinea=
Scaffold-DbContext "Server=DESKTOP-1ULHC37;Database=Inventory;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
 */
namespace NetCore31Test
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //alola
            var connection = Configuration.GetConnectionString("InventoryDatabase");            
            services.AddDbContextPool<InventoryContext>(options => options.UseSqlServer(connection));
            services.AddControllers();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience=Configuration["Jwt:Audience"],
                    ValidIssuer=Configuration["Jwt:Issuer"],
                    IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //añadido
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
