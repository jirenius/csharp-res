using ResgateIO.Service;
using System.Collections.Generic;

namespace BookCollection
{
    class BookModelHandler : ModelHandler
    {
        public override void Access(IAccessRequest request)
        {
            // Allow everone to access this resource
            request.AccessGranted();
        }

        public override void Get(IModelRequest request)
        {
            Book book = BookStore.GetBook(request.ResourceName);
            if (book == null)
            {
                request.NotFound();
            }
            else
            {
                request.Model(book);
            }
        }

        public override void Call(ICallRequest request)
        {
            switch (request.Method)
            {
                // Handle setting of the book properties
                case "set":
                    Book bookParams = request.ParseParams<Book>();
                    var changed = new Dictionary<string, object>(2);

                    // Check if the title property was provided
                    if (bookParams.Title != null)
                    {
                        changed["title"] = bookParams.Title.Trim();
                    }

                    // Check if the author property was provided
                    if (bookParams.Author != null)
                    {
                        changed["author"] = bookParams.Author.Trim();
                    }

                    // Send a change event with updated fields
                    request.ChangeEvent(changed);

                    // Send success response
                    request.Ok();
                    break;

                default:
                    request.MethodNotFound();
                    break;
            }
        }

        public override Dictionary<string, object> ApplyChange(ResourceContext resource, Dictionary<string, object> changes)
        {
            var book = resource.RequireValue<Book>();
            var revert = new Dictionary<string, object>(changes.Count);

            if (changes.TryGetValue("title", out object titleObject))
            {
                string title = (string)titleObject;
                if (title == "")
                {
                    throw new ResException("Title must not be empty.");
                }
                if (title != book.Title)
                {
                    revert["title"] = book.Title;
                    book.Title = title;
                }
            }

            if (changes.TryGetValue("author", out object authorObject))
            {
                string author = (string)authorObject;
                if (author == "")
                {
                    throw new ResException("Author must not be empty.");
                }
                if (author != book.Author)
                {
                    revert["author"] = book.Author;
                    book.Author = author;
                }
            }

            return revert;
        }
    }
}
