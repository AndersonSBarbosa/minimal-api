using minimal_api.Dominio.Dtos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");


app.MapPost("/login", (LoginDTO loginDto) =>{

    if (loginDto.Usuario == "Admin  Teste" && loginDto.Senha == "123456")
        return Results.Ok("Login realizado com sucesso!");
    else
        return Results.Unauthorized();


});
app.Run();
