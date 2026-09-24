using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (Serilog) ---
// Configure sinks (console, file) and enrichers here before anything else logs.
builder.Host.UseSerilog();

// --- Controllers ---
builder.Services.AddControllers();

// --- OpenAPI / Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// --- Database (EF Core + PostgreSQL) ---
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity (ASP.NET Core Identity) ---
// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();

// --- Authentication (JWT Bearer) ---
// Configure ValidIssuer, ValidAudience, and IssuerSigningKey from configuration.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// --- Repositories ---
// builder.Services.AddScoped<IRouteRepository, RouteRepository>();
// builder.Services.AddScoped<IRatingRepository, RatingRepository>();
// builder.Services.AddScoped<IRegionRepository, RegionRepository>();

// --- Validation (FluentValidation) ---
// builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// --- Error Handling (ProblemDetails / RFC 7807) ---
builder.Services.AddProblemDetails();

var app = builder.Build();

// --- Global Exception Middleware ---
// app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
