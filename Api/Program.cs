using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Infraestrutura.DB;
using minimal_api.Dominio.Infraestrutura.Interface;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
#region JWT
var key = builder.Configuration.GetSection("Jwt").ToString();

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        //ValidateIssuerSigningKey = true,
        //ValidIssuer = builder.Configuration["Jwt:Issuer"],
        //ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

#endregion

builder.Services.AddScoped<IAdminstradorServico, AdminstradorServico>();
builder.Services.AddScoped<IVeiculosServico, VeiculosServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o Token JWT desta Maneira: Bearer {token gerado}"
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
            new string[]{}
        }
    });

});

builder.Services.AddDbContext<DbContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ManagerAPISqlServer")));

var app = builder.Build();

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region ADMINISTRADOR

string GerarToken(Administrador administrador) {
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var segurityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
    var credenciais = new SigningCredentials(segurityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credenciais
        );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/login", ([FromBody]LoginDTO loginDto, IAdminstradorServico AdminstradorServico) =>{

    var adm = AdminstradorServico.Login(loginDto);

    if (adm != null)
    {
        string token = GerarToken(adm);
        return Results.Ok(new AdmLogado { 
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }

}).AllowAnonymous().WithTags("Adminstrador");

app.MapPost("/Adminstradores", ([FromBody] AdminstradorDto dto, IAdminstradorServico AdminstradorServico) => {

    var validacao = new ErrosDeValidacao { 
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(dto.Email))
        validacao.Mensagens.Add("O email � obrigat�rio.");
    if(string.IsNullOrEmpty(dto.Senha) || dto.Senha.Length < 6)
        validacao.Mensagens.Add("A senha � obrigat�ria e deve ter pelo menos 6 caracteres.");
    if (dto.Perfil == null)
        validacao.Mensagens.Add("O perfil � obrigat�rio");
    
    if (validacao.Mensagens.Count > 0)
            return Results.BadRequest(validacao);

    var administrador = new Administrador
    {
        Email = dto.Email,
        Senha = dto.Senha,
        Perfil = dto.Perfil.ToString()
    };
    AdminstradorServico.Incluir(administrador);

    return Results.Created($"/Adminstradores/{administrador.Id}", new AdminstradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Adminstrador");

app.MapGet("/Adminstradores", ([FromQuery] int? pagina, IAdminstradorServico adminstradorServico) =>
{
    var adms = new List<AdminstradorModelView>();
    var adm = adminstradorServico.Todos(pagina);
    foreach (var a in adm)
    {
        adms.Add(new AdminstradorModelView
        {
            Id = a.Id,
            Email = a.Email,
            Perfil = a.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization()
.RequireAuthorization( new AuthorizeAttribute { Roles = "Admin" })
.WithTags("Adminstrador");

app.MapGet("/Adminstradores/{id}", ([FromRoute] int id, IAdminstradorServico adminstradorServico) =>
{
    var adm = adminstradorServico.BuscaPorId(id);
   
    if (adm == null)
        return Results.NotFound("Ve�culo n�o encontrado.");
    return Results.Ok(new AdminstradorModelView
    {
        Id = adm.Id,
        Email = adm.Email,
        Perfil = adm.Perfil
    });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Adminstrador");

#endregion

#region Veiculos

ErrosDeValidacao validaDTO(VeiculoDTO dto)
{
    var validacao = new ErrosDeValidacao { 
        Mensagens = new List<string>()
    };
    if(string.IsNullOrEmpty(dto.Nome) || dto.Nome.Length < 3)
        validacao.Mensagens.Add("O nome do ve�culo � obrigat�rio e deve ter pelo menos 3 caracteres.");
    if(string.IsNullOrEmpty(dto.Marca) || dto.Marca.Length < 2) 
        validacao.Mensagens.Add("A marca do ve�culo � obrigat�ria e deve ter pelo menos 2 caracteres.");
    if(string.IsNullOrEmpty(dto.Modelo) || dto.Modelo.Length < 2)
        validacao.Mensagens.Add("O modelo do ve�culo � obrigat�rio e deve ter pelo menos 2 caracteres.");
    if(dto.Ano < 1950 || dto.Ano > DateTime.Now.Year + 1)
        validacao.Mensagens.Add($"O ano do ve�culo deve estar entre 1950 e {DateTime.Now.Year +1}.");
    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO dto, IVeiculosServico veiculosServico) => {

    var validacao = validaDTO(dto);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var veiculo = new Veiculo
    {
        Nome = dto.Nome,
        Marca = dto.Marca,
        Modelo = dto.Modelo,
        Ano = dto.Ano
    };
    veiculosServico.Incluir(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Editor" }).WithTags("Veiculo");

app.MapGet("/veiculos", ([FromQuery] int? pagina , IVeiculosServico veiculosServico) =>
    {
        var veiculos = veiculosServico.Todos(pagina);
        return Results.Ok(veiculos);
    }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Editor" }).WithTags("Veiculo");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculosServico veiculosServico) =>
{
    var veiculos = veiculosServico.BuscaPorId(id);
    if (veiculos == null)
        return Results.NotFound("Ve�culo n�o encontrado.");

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Editor" }).WithTags("Veiculo");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO dto, IVeiculosServico veiculosServico) =>
{
    var veiculos = veiculosServico.BuscaPorId(id);
    if (veiculos == null)
        return Results.NotFound("Ve�culo n�o encontrado.");

    var validacao = validaDTO(dto);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculos.Nome = dto.Nome;
    veiculos.Marca = dto.Marca;
    veiculos.Modelo = dto.Modelo;
    veiculos.Ano = dto.Ano;

    veiculosServico.Atualizar(veiculos);

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Veiculo");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculosServico veiculosServico) =>
{
    var veiculos = veiculosServico.BuscaPorId(id);
    if (veiculos == null)
        return Results.NotFound("Ve�culo n�o encontrado.");

    veiculosServico.Apagar(veiculos);

    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }).WithTags("Veiculo");

#endregion


app.UseSwagger();
app.UseSwaggerUI();

#region UseAuthorization
app.UseAuthentication();
app.UseAuthorization();
#endregion

app.Run();
