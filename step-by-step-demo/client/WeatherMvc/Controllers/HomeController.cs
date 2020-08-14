using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WeatherMvc.Models;
using WeatherMVC.Models;
using WeatherMVC.Services;

namespace WeatherMVC.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly ITokenService _tokenService;

    public HomeController(ILogger<HomeController> logger, ITokenService tokenService)
    {
      _logger = logger;
      _tokenService = tokenService;
    }

    public IActionResult Index()
    {
      return View();
    }


    [Authorize]
    public async Task<IActionResult> Weather()
    {
      var data = new List<WeatherData>();
      
            
      using (var client = new HttpClient())
      {
                
        var tokenResponse = await _tokenService.GetToken("weatherapi.read");
                
        client
          .SetBearerToken(tokenResponse.AccessToken);
      
        var result = client
          .GetAsync("https://localhost:5445/weatherforecast")
          .Result;
      
        if (result.IsSuccessStatusCode)
        {
          var model = result.Content.ReadAsStringAsync().Result;
      
          data = JsonConvert.DeserializeObject<List<WeatherData>>(model);
      
          return View(data);
        }
        else
        {
          throw new Exception("Unable to get content");
        }
      }
    }

    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
  }
}