using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Shared.Entities
{
    public class Book
    {
        public ObjectId _id;
        public string Title { get; set; }
        public string Description { get; set; }

    }
}
