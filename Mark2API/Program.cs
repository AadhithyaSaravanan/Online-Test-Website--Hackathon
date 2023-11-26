using Mark2API.Models;
using Mark2API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure the DbContext for OnlineDbContext with SQL Server connection

builder.Services.AddDbContext<OnlineDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("NewConn")));
//add scoped for repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
// Configure JSON serialization settings for controllers
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    //avoid looping while serialization
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddAuthorization(options =>
{
    // Define an "AdminPolicy" requiring the "Admin" role claim
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "Admin");
    });
    // Define a "UserPolicy" requiring the "User" role claim
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "User");
    });
});


//Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "JWTAuthenticationServer",//issuer
        ValidAudience = "JWTServicePostmanClient",//reciver
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Yh2k7QSu4l8CZg5p6X3Pna9L0Miy4D3Bvt0JVr87UcOj69Kqw5R2Nmf4FWs03Hdx")),//secret key
        RoleClaimType = ClaimTypes.Role // Ensure role claim is correctly configured
    };
});
builder.Services.AddSwaggerGen(c => {//adding details for swagger document
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement//adding and specifying
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
                             new string[] {}

                     }
        });
});
var app = builder.Build();//build webapplication instance using configuration setup

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())//check whether it is running in development environment
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();//secure connection during redirection
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();//control the request og http from controller and actions

app.Run();//entry point of project
