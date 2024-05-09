using System.Net;
using Azure;
using Functions.Shared.Entities;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Functions.REST.API
{
    public class Function1
    {
        private readonly ILogger _logger;
        
        private readonly IMongoClient _mongoClient;

        public Function1(ILoggerFactory loggerFactory, IMongoClient mongoClient)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            _mongoClient = mongoClient;
        }

        [Function("Help")]
        public HttpResponseData Help(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "help")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString("This API is using MongoDB, as such all the Ids are ObjectIds" +
                "\n [ENDPOINTS]: /api/book , /api/books" +
                "\n [POST] " +
                "\n PostBook: /api/book + body. string Title + string Description" +
                "\n [GET] " +
                "\n GetAllBooks: /api/books" +
                "\n GetBook: /api/book/{id}" +
                "\n [PUT] " +
                "\n UpdateBook: /api/book/{id} + body. string Title + string Description" +
                "\n [DELETE] " +
                "\n DeleteBook: /api/book/{id}");

            return response;
        }

        //-------------------------------------------------- WORKS -------------------------------------
        [Function("GetAllBooks")]
        public async Task<HttpResponseData> GetAllBooks(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "books")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var database = _mongoClient.GetDatabase("LiberaBookDB");
            var collection = database.GetCollection<BsonDocument>("Books");
            var responseGet = await collection.Find(new BsonDocument()).ToListAsync();
            if (responseGet != null && responseGet.Count > 0)
            {
                var jsonBooks = responseGet.ToJson();
                response.WriteString(jsonBooks);
                return response;
            }
            else
            {
                var jsonMessage = new { Message = "No books found" };
                response.WriteString(Newtonsoft.Json.JsonConvert.SerializeObject(jsonMessage));
                return response;
            }
        }

        //-------------------------------------------------- WORKS -------------------------------------
        [Function("GetBook")]
        public async Task<HttpResponseData> GetBook(
                [HttpTrigger(AuthorizationLevel.Function, "get", Route = "book/{id}")] HttpRequestData req,
                string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Bad Id");
                return response;
            }

            var database = _mongoClient.GetDatabase("LiberaBookDB");
            var collection = database.GetCollection<Book>("Books");
            var filter = Builders<Book>.Filter.Eq(b => b._id, objectId);

            var book = await collection.Find(filter).FirstOrDefaultAsync();

            if (book != null)
            {
                var jsonResponse = Newtonsoft.Json.JsonConvert.SerializeObject(book);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString(jsonResponse);
                return response;
            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Book not found.");
                return response;
            }
            
        }

        //-------------------------------------------------- WORKS -------------------------------------
        [Function("PostBook")]
        public async Task<HttpResponseData> PostBook(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "book")] HttpRequestData req
            )
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Book book = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(requestBody);

            var database = _mongoClient.GetDatabase("LiberaBookDB");
            var collection = database.GetCollection<Book>("Books");
            await collection.InsertOneAsync(book);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString($"Added book {book.Title} to the database");
            return response;
        }

        //-------------------------------------------------- WORKS -------------------------------------
        [Function("UpdateBook")]
        public async Task<HttpResponseData> PutBook(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "book/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Bad Id");
                return response;
            }

            var database = _mongoClient.GetDatabase("LiberaBookDB");
            var collection = database.GetCollection<Book>("Books");

            var filter = Builders<Book>.Filter.Eq(b => b._id, objectId);
            var bookToBeUpdated = await collection.Find(filter).FirstOrDefaultAsync();

            if (bookToBeUpdated != null)
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Book book = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(requestBody);

                if (book != null)
                {
                    book._id = bookToBeUpdated._id;
                    collection.ReplaceOne(filter, book);
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json");
                    response.WriteString($"Book {bookToBeUpdated.Title} has been updated");
                    return response;
                } else
                {
                    var response = req.CreateResponse(HttpStatusCode.BadRequest);
                    response.Headers.Add("Content-Type", "application/json");
                    response.WriteString("No message body found");
                    return response;

                }
            } else
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("No book found");
                return response;
            }
        }

        //-------------------------------------------------- WORKS -------------------------------------
        [Function("DeleteBook")]
        public async Task<HttpResponseData> DeleteBook(
                [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "book/{id}")] HttpRequestData req,
                string id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Bad Id");
                return response;
            }

            var database = _mongoClient.GetDatabase("LiberaBookDB");
            var collection = database.GetCollection<Book>("Books");
            var filter = Builders<Book>.Filter.Eq(b => b._id, objectId);

            var book = await collection.Find(filter).FirstOrDefaultAsync();

            if (book != null)
            {
                collection.DeleteOne(filter);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString($"{book.Title} removed from database");
                return response;
            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Book not found.");
                return response;
            }
        }
    }
}
