using Api1.DocsServices.BLL;
using Api1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests
{
    public class Tests
    {
        DocContext docContext = new DocContext(new DbContextOptionsBuilder<DocContext>().UseInMemoryDatabase("DocsList").Options);
        private DocsService docsService; 
        IServiceProvider ServiceProvider;


      

        [SetUp]
        public void Setup()
        {
            docsService = new DocsService(docContext,
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build());

            ServiceProvider = Api1.Program.CreateHostBuilder(new string[0]).Build().Services;

            var doc = new Doc
            {
                Category = Category.APPLICATION,
                Name = "TestFile.txt",
            };

            var ver = new Api1.Models.Version
            {
                Doc = doc,
                Release = 1,
                Size = 1234,
                UploadDateTime = DateTime.Now,
                Path = @"C:\\Files\\Tests",
            };

            docContext.Add(doc);// ��� ���������� � ���������, ������� ��������� � ������, in memory?
            docContext.Versions.Add(ver);
            int res = docContext.SaveChanges();
        }

        [Test]
        public void GetDocs_Get1Doc_NotNull()
        // [����������� �����]_[��������]_[��������� ���������].
        {
            DocsService ApiService = ServiceProvider.GetService<DocsService>();
            IList <Doc> result = ApiService.GetDocs(null);//������ �� ��������� ���������� � �������� ����?

            Assert.NotNull(result);
        }
    }
}