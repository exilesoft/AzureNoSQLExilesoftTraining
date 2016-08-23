using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
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
        private readonly string _dataBase = "imageDatabase";
        private readonly string _imageCollection = "images";
        private readonly string _commentCollection = "comments";

        private readonly DocumentClient _client;

        public DocumentDbController()
        {
            _client = new DocumentClient(new Uri("uri"), "key");

        }

        [HttpPost, Route("upload")]
        public async Task<IHttpActionResult> UploadImage(ProfileImage image)
        {
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
        public string Url { get; set; } = "https://";
        public DateTime CreatedOn
        {
            get
            {
                return CreatedOn;
            }
            set
            {
                CreatedOn = value;
                Epoch = (int)DateTime.UtcNow.Ticks;
            }
        }
        public int Epoch { get; set; }
    }
}
