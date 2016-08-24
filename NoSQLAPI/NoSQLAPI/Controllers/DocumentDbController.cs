using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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
    [RoutePrefix("api/documentdb")]
    public class DocumentDbController : ApiController
    {
        private readonly string _dataBase = "exile";
        private readonly string _imageCollection = "images";

        private readonly DocumentClient _client;

        public DocumentDbController()
        {
            var url = ConfigurationManager.AppSettings["documentdb:url"].ToString();
            var key = ConfigurationManager.AppSettings["documentdb:key"].ToString();

            _client = new DocumentClient(new Uri(url), key);
        }

        [HttpPost, Route("upload")]
        public async Task<IHttpActionResult> UploadImage(ProfileDto imageDto)
        {
            var image = new ProfileImage
            {
                Id = Guid.NewGuid().ToString().ToLower(),
                CreatedOn = DateTime.UtcNow,
                Epoch = DateTime.UtcNow.Ticks,
                Description = imageDto.Description,
                Tags = imageDto.Tags
            };

            var response = await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_dataBase, _imageCollection), image);
            return Ok(image.Id);
        }

        [HttpGet, Route("images/recent/{search}")]
        public async Task<IHttpActionResult> GetImages(string search)
        {
            var profileImageQueryBase = _client.CreateDocumentQuery<ProfileImage>(UriFactory.CreateDocumentCollectionUri(_dataBase, _imageCollection));
            var imageResult = profileImageQueryBase.Where(i => i.Tags.Contains(search)).AsDocumentQuery();

            var result = new List<ProfileImage>();

            while (imageResult.HasMoreResults)
            {
                result.AddRange(await imageResult.ExecuteNextAsync<ProfileImage>());
            }

            return Ok(result);   
        }
    }

    public class ProfileImage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString().ToLower();
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Url { get; set; } = "https://s-media-cache-ak0.pinimg.com/564x/61/22/cb/6122cb371a319afa82c5d4e8077ebbdc.jpg";
        public DateTime CreatedOn { get; set; }
        public long Epoch { get; set; }
    }

    public class ProfileDto
    {
        public string Description { get; set; }
        public List<string> Tags { get; set; }
    }
}
