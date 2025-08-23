using Adidas.Application.Contracts.ServicesContracts;
using Microsoft.AspNetCore.Hosting;

public class PathProvider : IPathProvider
{
    private readonly IWebHostEnvironment _env;

    public PathProvider(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string GetRootPath()
    {
        return _env.WebRootPath;
    }
}
