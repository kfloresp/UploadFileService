var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var principalFolderPath = configuration.GetValue<string>("FileStorage:UploadsFolderPath");
var keyEncrypt = configuration.GetValue<string>("Encryption:Key");
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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


app.MapGet("/api/version", () =>
{
    return Results.Ok("Bienvienido a Pran-UFS-1.0.0");
});

app.MapPost("/api/upload/photo", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Content type must be 'multipart/form-data'.");
    }

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    var description = form["description"];
    var userId = form["userId"];

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file selected.");
    }

    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads", userId);
    Directory.CreateDirectory(uploads);
    var filePath = Path.Combine(uploads, description+""+file.FileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    var requestUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/uploads/{userId}/{description+""+file.FileName}";


    return Results.Ok(new { url = requestUrl });
});

app.MapPost("/api/upload/document", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Content type must be 'multipart/form-data'.");
    }

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    var description = form["description"];
    var userId = form["userId"];

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }

    var uploadsFolderPath = Path.Combine(principalFolderPath, "documents", userId);
    Directory.CreateDirectory(uploadsFolderPath);

    var filePath = Path.Combine(uploadsFolderPath, file.FileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    var requestUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/uploads/documents/{userId}/{file.FileName}";


    return Results.Ok(new { Url = requestUrl });
});


app.Run();