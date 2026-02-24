using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// API stuff
builder.Services.AddControllers();
// Auth stuff
var key = Encoding.ASCII.GetBytes(Simple.TestWebApi.Auth.Token.Secret);
builder.Services.AddAuthentication(x =>
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
builder.Services.AddSwaggerGen(c =>
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

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

// normal stuff
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Run();


