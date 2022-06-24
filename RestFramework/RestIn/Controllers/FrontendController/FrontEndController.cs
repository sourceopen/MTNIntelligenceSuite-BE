﻿using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.Extensions.Logging;
using DBFramework;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RestIn.Controllers
{
    [ApiController]
    [Route("frontend")]
    public class FrontEndController : Controller
    {
        private readonly ILogger<FrontEndController> _logger;

        public FrontEndController(ILogger<FrontEndController> inLogger)
        {
            _logger = inLogger;
        }

        [Route("getOccupancyValues")]
        [HttpGet]
        public IActionResult GetOccupancyValues()
        {
            DBConnection db = new DBConnection();
           // string j = "values:{22:10,23:12,24:12}";
            //string j = "[{\"userId\": 1,\"id\": 1,\"title\": \"delectus aut autem\",\"completed\": false},{\"userId\": 1,\"id\": 2,\"title\": \"quisutnam\",\"completed\": false}]";
           // string json = JsonConvert.SerializeObject(j);

            //return Ok(json);


            var jsonString = "{\"values\":[1,12,24]}";
        //    Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
          //  Response.WriteAsync(jsonString, Encoding.UTF8);

            //return CreatedAtRoute("Result", json);
            // return Ok(json);

            //var data = new { Name = "kevin", Age = 40 };
            //return Json(jsonString);
            return Ok(jsonString);
        }
    }

}