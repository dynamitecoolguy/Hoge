using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using HogeLib.Models;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

namespace IchibanWebAPI.Controllers {
    [Route("blog")]
    [ApiController]
    public class BlogController : ControllerBase {
        IDynamoDBContext DDBContext {
            get; set;
        }

        /// <summary>
        /// Default constructor that Controller will invoke.
        /// </summary>
        public BlogController() {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = "Hoge_Blog";
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] = new Amazon.Util.TypeMapping(typeof(Blog), tableName);

            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig { ServiceURL = "http://localdynamodb:8000" };
            AmazonDynamoDBClient client = new AmazonDynamoDBClient("dummy", "dummy", clientConfig);

            try {
                // DynamoDB内にテーブルが無ければ新規作成する。もちろん本番用Controllerでこんなことをしてはいけない
                Task<CreateTableResponse> response = client.CreateTableAsync(
                    tableName,
                    new List<KeySchemaElement> {
                        new KeySchemaElement("Id", KeyType.HASH)
                    },
                     new List<AttributeDefinition> {
                        new AttributeDefinition("Id", ScalarAttributeType.S)
                    },
                    new ProvisionedThroughput {
                        ReadCapacityUnits = 1,
                        WriteCapacityUnits = 1
                    },
                    new System.Threading.CancellationToken()
                );
                response.Wait();
            } catch (Exception _) {
                // すでにあるならなにもしない
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(client, config);
        }

        // GET: blog
        [HttpGet]
        public async Task<IEnumerable<Blog>> GetAsync() {
            var search = this.DDBContext.ScanAsync<Blog>(null);
            var blogs = await search.GetNextSetAsync();

            return blogs;
        }

        // GET: blog/blogId
        [HttpGet("{blogId}", Name = "Get")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Blog>> GetAsync(string blogId) {
            if (string.IsNullOrEmpty(blogId)) {
                return BadRequest();
            }

            var blog = await DDBContext.LoadAsync<Blog>(blogId);
            if (blog == null) {
                return NotFound();
            }

            return blog;
        }

        // PUT: blog
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult> PutAsync([FromBody] Blog blog) {
            blog.Id = Guid.NewGuid().ToString();
            blog.CreatedTimestamp = DateTime.Now;

            await DDBContext.SaveAsync<Blog>(blog);

            return Accepted();
        }

        // DELETE: blog/blogId
        [HttpDelete("{blogId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult> DeleteAsync(string blogId) {
            if (string.IsNullOrEmpty(blogId)) {
                return BadRequest();
            }

            await this.DDBContext.DeleteAsync<Blog>(blogId);

            return Accepted();
        }
    }
}
