using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Simple.TestWebApi
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
            // API stuff
            services.AddControllers();
            // Auth stuff
            var key = Encoding.ASCII.GetBytes(Auth.Token.Secret);
            services.AddAuthentication(x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(x =>
                    {
                        x.RequireHttpsMetadata = false;
                        x.SaveToken = true;
                        x.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                    });
            // Swagger stuff
            services.AddSwaggerGen(c =>
            {
                var schemeId = JwtBearerDefaults.AuthenticationScheme;

                // 1. Criamos o esquema SEM a propriedade Reference (que não existe mais)
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!"
                };

                // 2. Registramos a definição usando o ID
                c.AddSecurityDefinition(schemeId, jwtSecurityScheme);

                // 3. Criamos o requisito usando a nova classe de referência
                c.AddSecurityRequirement(document =>
                {
                    OpenApiSecuritySchemeReference schemeRef = new("Bearer");
                    OpenApiSecurityRequirement requirement = new()
                    {
                        [schemeRef] = []
                    };
                    return requirement;
                });

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Simple.TestWebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // aways use swagger, this is a test app....
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple.TestWebApi v1"));

            // normal stuff
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
