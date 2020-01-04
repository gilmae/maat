using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace StrangeVanilla.Maat
{
    public class HomeModule : Nancy.NancyModule
    {
         public HomeModule()
            {
                Get("/", p => {

                    return "Hello World!"; });
            }
        
    }
}
