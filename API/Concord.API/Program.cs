using Concord.Application.Services.Account.Providers;
using Concord.Application.Services.Categories;
using Concord.Application.Services.Mail;
using Concord.Application.Services.Providers;
using Concord.Application.Services.RefreshToken;
using Concord.Application.Services.Token;
using Concord.Domain.Context.Application;
using Concord.Domain.Context.Identity;
using Concord.Domain.Models.Identity;
using Concord.Domain.Repositories;
using Concord.Domain.Seed.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server Connection:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// for identity DbContext:
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<ApplicationUser>>();

// Token Settings
var jwt = builder.Configuration.GetSection("Token");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["key"])),
            ValidIssuer = jwt["Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
        };
    });

// Add authorization and authentication
builder.Services.AddAuthorization();

// add scopped for interfaces:
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddTransient<IGetPrincipleDataFromExpiredToken, GetPrincipleDataFromExpiredToken>();
builder.Services.AddTransient<IMailingService, MailingService>();
builder.Services.AddTransient<IProviderAccountService, ProviderAccountService>();
builder.Services.AddTransient<IProviderManagementService, ProviderManagementService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();


// For Add Auto mapper to our project
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Auth in swagger:
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Omayya Class API", Version = "v1" });
    c.ResolveConflictingActions(x => x.First());
    // Swagger 2.+ support
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                //Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new string[] {}
        }
    });
    
    // Include XML comments in Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    // Check if XML file exists before including it
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
        Console.WriteLine($"XML documentation file found and included: {xmlPath}");
    }
    else
    {
        Console.WriteLine($"XML documentation file NOT found at: {xmlPath}");
        Console.WriteLine("Make sure GenerateDocumentationFile is set to true in your .csproj");
    }

    // Optional: Enable annotations for additional features
    c.EnableAnnotations();
});

// Enable Cors For FrontEnd:
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.WithOrigins("http://localhost:4200")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// add signalR:
builder.Services.AddSignalR();



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// migrations and seed for identity:
using (var scope = app.Services.CreateScope())
{
    // Automatically apply pending migrations and update the database on startup
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    var IdentityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    IdentityContext.Database.Migrate();

    // Identity Seed:
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await IdentitySeeder.SeedRoles(serviceProvider, userManager);

    // seed main data:
    //var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    //await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger UI (at /swagger endpoint):
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Omayya Class API");
    });
}

app.UseCors("AllowOrigin");

app.UseHttpsRedirection();

// add this for save images in wwwroot:
app.UseStaticFiles();

// for return 401 when unauthorize:
app.UseMiddleware<AuthenticationMiddleware>();


app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();



app.MapControllers();

app.Run();
