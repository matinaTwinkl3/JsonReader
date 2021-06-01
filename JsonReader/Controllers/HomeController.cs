using JsonReader.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonReader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            string inputFile = _config.GetSection("InputFile").Value.ToString();
            List<JsonData> obj = new List<JsonData>();
            List<ResponseJsonData> responseObj = new List<ResponseJsonData>();
            try
            {
                string data = System.IO.File.ReadAllText(inputFile, Encoding.UTF8);
                if (string.IsNullOrEmpty(data) == false)
                {
                    obj = JsonConvert.DeserializeObject<List<JsonData>>(data);
                    var count = 0;
                    var distinctAddressCount = 0;
                    string[] address = new string[obj.Count];
                    foreach (var item in obj)
                    {
                        string[] jsonAddress = item.Address.Split(',');
                        address[count] = jsonAddress[1];
                        count++;
                        if (address.Contains(jsonAddress[1]))
                        {
                            if (distinctAddressCount == 0)
                            {
                                distinctAddressCount++;
                            }
                            else
                            {
                                int value = responseObj.Where(x => x.Address == jsonAddress[1]).ToList().Select(x => x.Count).FirstOrDefault();
                                distinctAddressCount = value + 1;
                            }
                            var resObj = new ResponseJsonData
                            {
                                Count = distinctAddressCount,
                                Address = jsonAddress[1]
                            };
                            if (resObj.Count == 1)
                            {
                                responseObj.Add(resObj);
                            }
                            else
                            {
                                var res = responseObj.FirstOrDefault(x => x.Address == jsonAddress[1]);
                                res.Count = distinctAddressCount;
                            }
                        }                   
                       
                    }
                    ViewBag.Response = responseObj;
                }
                var jsonResponse = JsonConvert.SerializeObject(responseObj);
                string outFile = _config.GetValue<string>("OutputFile");
                try
                {
                    System.IO.File.WriteAllText(outFile, jsonResponse);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(responseObj);
        }
    }
}
