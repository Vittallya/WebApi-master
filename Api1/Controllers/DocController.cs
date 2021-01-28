using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Api1.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

using Api1.DocsServices.BLL;
using Api1.Resources;

namespace Api1.Controllers
{

    [Route("Files")]
    [ApiController]
    public class DocController : Controller
    {
        DocContext _context;
        IWebHostEnvironment _appEnvironment;
        private readonly IDocsService _services;
        //cистема внедрения зависимостей использует конструкторы классов для передачи всех зависимостей.
        //Соответственно в конструкторе контроллера мы можем получить зависимость. 
        //Конструкторы являются наиболее предпочтительным вариантом для получения зависимостей

        public DocController(DocContext context, IWebHostEnvironment appEnvironment, IDocsService services)
        {
            _context = context;
            _appEnvironment = appEnvironment;
            _services = services;
        }
        [Route("Upload")]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile uploadeDoc, CategoryDTO category)
        {
            if ((int)category < 1 || (int)category > 3)
                return BadRequest("Upload was not successful.\r\n Invalid category.\r\n Please, select correct category");
            if (uploadeDoc == null)
                return BadRequest("Upload was not successful.\r\n You didn't choose a file.\r\n Please, select the file you want to upload");
           
            try
            {
                await _services.UploadFileAsync(uploadeDoc, category);//обращение
                return CreatedAtAction("Success! File was upload", uploadeDoc);
            }
            catch (Exception exc)
            {
                return BadRequest($"Error: {exc.Message}");
            }
        }

        [Route("All")]
        //[Route("All or category")] поверить как отображается 
        [HttpGet]
        public IActionResult GetDocs(Category? category)
        {
            if ((category == null) || ((int)category >= 1 && (int)category <= 3))
            {
                var docs = _services.GetDocs(category);

                return Ok(Map(docs));// что будет с параметром, если будет null
            }
            //if (category != null)
            //    return Ok(_context.Docs.Include(p => p.Versions).ToList());
            else
                return BadRequest("Сategory was selected incorrectly.\r\n Please select one of the available options:\r\n Presentation (category = 1),\r\n Application(2),\r\n Other(3)");
            //если категории поменяются комментарий будет некорректный, как сделать, чтобы подтягивалось автоматически???
        }
     

        [HttpGet("Name")]
        public IActionResult GetDoc(string name)
        {
            var doc = _services.GetDoc(name);
            if (doc == null)
            {
                return NotFound("The file with the specified name was not found");
            }
            return Ok(Map(doc));
        }


        private ICollection<DocResuorce> Map(ICollection<Doc> docs)
        {
            var docsR = new List<DocResuorce>(docs.Count);

            foreach (var doc in docs)
            {
                docsR.Add(new DocResuorce
                {
                    Id = doc.Id,
                    Category = doc.Category,
                    Name = doc.Name,
                    Versions = doc.Versions.Select(x => new VersionResource
                    {
                        Id = x.Id,
                        DocId = x.DocId,
                        Path = x.Path,
                        Release = x.Release,
                        Size = x.Size,
                        UploadDateTime = x.UploadDateTime,
                    }).ToList(),
                });
            }
            return docsR;
        }


        [Route("Download")]
        [HttpGet]
        public async Task<IActionResult> DownloadDocAsync(string name, int category, int? release)
        {
            byte[] Mass = null;
            Mass = await _services.DownloadDocAsync(name, category, release);
            if (Mass != null)
                try
                {

                    return File(Mass, "application/octet-stream", name); //Основной подтип 'Application/Octet-Stream'
                    // Используется для обозначения того, что тело содержит бинарные данные.
                    // MIME тип состоит из типа и подтипа — двух строк разделённых наклонной чертой (/), без использования пробелов.
                    //application Список IANA
                    //Любой вид бинарных данных, явно не попадающих ни в одну другу группу типов. Данные, которые будут выполняться или как 
                    //- либо интерпретироваться, или данные для выполнения, которых необходимо отдельное приложение
                    //Для указания базового типа бинарных данных(данных без определённого типа) используют тип application / octet - stream
                    //Этот тип является базовым для бинарных данных.В связи с тем, что он подразумевает неопределённые бинарные данные, 
                    //браузеры, как правило, не будут пытаться его обработать каком - либо образом, а вызовут для него диалоговое окно «Сохранить Как»,
                }
                catch (Exception e)
                {
                    return BadRequest($"File does not read: {e.Message}");
                }
            return BadRequest("File was not found");
        }

        [Route("Category")]
        [HttpPatch]
        public ActionResult<Doc> ChangeCategory(string name, Category oldCategory, Category newCategory)
        {
            var doc = _services.ChangeCategory(name, oldCategory, newCategory);
            if (doc == null)
            {
                return BadRequest();
            }
            return doc;
        }

        [HttpDelete("Name")]
        public ActionResult<Doc> DeleteDoc(string name, Category category)
        {
            var doc = _services.DeleteDoc(name, category);
            if (doc == null)
            {
                return NotFound("This file was not found");
            }
            return doc;
        }
    }
}