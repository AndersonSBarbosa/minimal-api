using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enums;
using minimal_api.Dominio.Infraestrutura.DB;
using minimal_api.Dominio.Infraestrutura.Interface;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using System.Runtime.Intrinsics.Arm;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminstradorServico, AdminstradorServico>();
builder.Services.AddScoped<IVeiculosServico, VeiculosServico>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<DbContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ManagerAPISqlServer")));


var app = builder.Build();



#region Home
app.MapGet("/", () => Results.Json(new Home()));
#endregion

#region ADMINISTRADOR

app.MapPost("/login", ([FromBody]LoginDTO loginDto, IAdminstradorServico AdminstradorServico) =>{

    if (AdminstradorServico.Login(loginDto) != null )
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();

}).WithTags("Adminstrador");

app.MapPost("/Adminstradores", ([FromBody] AdminstradorDto dto, IAdminstradorServico AdminstradorServico) => {

    var validacao = new ErrosDeValidacao { 
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(dto.Email))
        validacao.Mensagens.Add("O email é obrigatório.");
    if(string.IsNullOrEmpty(dto.Senha) || dto.Senha.Length < 6)
        validacao.Mensagens.Add("A senha é obrigatória e deve ter pelo menos 6 caracteres.");
    if (dto.Perfil == null)
        validacao.Mensagens.Add("O perfil é obrigatório");
    
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

}).WithTags("Adminstrador");

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
}).WithTags("Adminstrador");

app.MapGet("/Adminstradores/{id}", ([FromRoute] int id, IAdminstradorServico adminstradorServico) =>
{
    var adm = adminstradorServico.BuscaPorId(id);
   
    if (adm == null)
        return Results.NotFound("Veículo não encontrado.");
    return Results.Ok(new AdminstradorModelView
    {
        Id = adm.Id,
        Email = adm.Email,
        Perfil = adm.Perfil
    });
}).WithTags("Adminstrador");

#endregion

#region Veiculos

ErrosDeValidacao validaDTO(VeiculoDTO dto)
{
    var validacao = new ErrosDeValidacao { 
        Mensagens = new List<string>()
    };
    if(string.IsNullOrEmpty(dto.Nome) || dto.Nome.Length < 3)
        validacao.Mensagens.Add("O nome do veículo é obrigatório e deve ter pelo menos 3 caracteres.");
    if(string.IsNullOrEmpty(dto.Marca) || dto.Marca.Length < 2) 
        validacao.Mensagens.Add("A marca do veículo é obrigatória e deve ter pelo menos 2 caracteres.");
    if(string.IsNullOrEmpty(dto.Modelo) || dto.Modelo.Length < 2)
        validacao.Mensagens.Add("O modelo do veículo é obrigatório e deve ter pelo menos 2 caracteres.");
    if(dto.Ano < 1950 || dto.Ano > DateTime.Now.Year + 1)
        validacao.Mensagens.Add($"O ano do veículo deve estar entre 1950 e {DateTime.Now.Year +1}.");
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

}).WithTags("Veiculo");

app.MapGet("/veiculos", ([FromQuery] int? pagina , IVeiculosServico veiculosServico) =>
    {
        var veiculos = veiculosServico.Todos(pagina);
        return Results.Ok(veiculos);
    }).WithTags("Veiculo");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculosServico veiculosServico) =>
{
    var veiculos = veiculosServico.BuscaPorId(id);
    if (veiculos == null)
        return Results.NotFound("Veículo não encontrado.");

    return Results.Ok(veiculos);
}).WithTags("Veiculo");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO dto, IVeiculosServico veiculosServico) =>
{
    var veiculos = veiculosServico.BuscaPorId(id);
    if (veiculos == null)
        return Results.NotFound("Veículo não encontrado.");

    var validacao = validaDTO(dto);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculos.Nome = dto.Nome;
    veiculos.Marca = dto.Marca;
    veiculos.Modelo = dto.Modelo;
    veiculos.Ano = dto.Ano;

    veiculosServico.Atualizar(veiculos);

    return Results.Ok(veiculos);
}).WithTags("Veiculo");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculosServico veiculosServico) =>
{
    var veiculos = veiculosServico.BuscaPorId(id);
    if (veiculos == null)
        return Results.NotFound("Veículo não encontrado.");

    veiculosServico.Apagar(veiculos);

    return Results.NoContent();
}).WithTags("Veiculo");

#endregion


app.UseSwagger();
app.UseSwaggerUI();
app.Run();
