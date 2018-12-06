using Newtonsoft.Json;
using System;

namespace BookCollection
{
    class Book
    {
        [JsonProperty(PropertyName = "id")]
        public int ID;

        [JsonProperty(PropertyName = "title")]
        public string Title;

        [JsonProperty(PropertyName = "author")]
        public string Author;

        [JsonIgnore]
        public string ResourceID { get { return String.Format("library.book.{0}", ID); } }

        public Book(int id, string title, string author)
        {
            ID = id;
            Title = title;
            Author = author;
        }
    }
}