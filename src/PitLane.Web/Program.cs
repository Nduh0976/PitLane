var app = WebApplication.CreateBuilder(args).Build();
app.MapGet("/", () => "PitLane is under construction");
app.Run();