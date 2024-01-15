using Microsoft.EntityFrameworkCore;

#region Configuração
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseInMemoryDatabase("TarefasDB"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
#endregion Configuração

#region Gets
app.MapGet("/", () => "Hello World");

app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound("Tarefa não encontrada!")
);

app.MapGet("/tarefas/concluidas", async (AppDbContext db) =>
                                  await db.Tarefas.Where(t => t.Concluida).ToListAsync());
#endregion Gets

#region Post
app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});
#endregion Post

#region Put
app.MapPut("/tarefas/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if (tarefa is null) return Results.NotFound("Erro: essa tarefa não existe!");

    tarefa.Nome = inputTarefa.Nome;
    tarefa.Concluida = inputTarefa.Concluida;
    await db.SaveChangesAsync();

    return Results.NoContent();

});
#endregion Put

#region Delete
app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) return Results.NotFound("Erro: essa tarefa não existe!");
    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();

    return Results.Ok(tarefa);
});
#endregion Delete

app.Run();

#region Model Tarefa
class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool Concluida { get; set; }
}
#endregion Model Tarefa

#region Context
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}
#endregion Context