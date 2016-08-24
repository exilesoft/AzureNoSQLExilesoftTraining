using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace NoSQLAPI.Controllers
{
    [RoutePrefix("api/tablestorage")]
    public class TableController : ApiController
    {
        private CloudTableClient _tableClient;

        public TableController()
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["TableStorageConnection"].ConnectionString);
            _tableClient = storageAccount.CreateCloudTableClient();
        }

        [HttpPost, Route("student/create")]
        public IHttpActionResult CreateMember(StudentDto studentDto)
        {
            var memberTable = _tableClient.GetTableReference("student");

            var student = new Student
            {
                RegistrationNumber = studentDto.RegistrationNumber,
                Email = studentDto.Email,
                Name = studentDto.Name
            };

            var insertOperation = TableOperation.Insert(student);

            memberTable.CreateIfNotExists();
            memberTable.Execute(insertOperation);

            return Ok();
        }

        [HttpGet, Route("member/{registrationNumber}")]
        public IHttpActionResult GetMember(string registrationNumber)
        {
            var memberTable = _tableClient.GetTableReference("student");

            var partionKey = registrationNumber.ToLower().Split('-')[0];

            var record = from student in memberTable.CreateQuery<Student>()
                         where student.PartitionKey == partionKey && student.RegistrationNumber == registrationNumber
                         select new { student };

            return Ok(record);
        }
    }
    
    public class Student : TableEntity
    {
        public Student()
        {
        }

        [IgnoreProperty]
        public string RegistrationNumber
        {
            get
            {
                return RowKey;
            }
            set
            {
                PartitionKey = value.ToLower().Split('-')[0];
                RowKey = value;
            }
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public Guid UniqueIdentifier { get; set; } = Guid.NewGuid();
    } 

    public class StudentDto
    {
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
