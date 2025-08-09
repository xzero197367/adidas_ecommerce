using Microsoft.AspNetCore.Mvc.Razor;

namespace Adidas.AdminDashboardMVC.Helpers;

public class ViewLocationExpander: IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        
        // Custom locations for partials under /Areas/Admin/Views/Partial/**
        var customLocations = new[]
        {
            "/Views/Operation/{1}/{0}.cshtml", 
            "/Views/Operation/{1}/Sections/{0}.cshtml",

        };

        return customLocations.Concat(viewLocations);
    }
}