using Elasticsearch.API.Extensions;
using Elasticsearch.API.Services;
using Elasticsearch.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region Extension ElasticSearch.cs
////tek bir node
//var pool = new SingleNodeConnectionPool(new Uri(builder.Configuration.GetSection("Elastic")["url"]!));
//var settings = new ConnectionSettings(pool);
//var client = new ElasticClient(settings);
//builder.Services.AddSingleton(client);
#endregion

builder.Services.AddElastic(builder.Configuration);

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductRepository>();





var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
