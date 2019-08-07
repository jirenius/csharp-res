using ResgateIO.Service;
using System.Collections.Generic;

namespace BookCollection
{
    class BookModelHandler : IModelHandler, IAccessHandler, ICallHandler
    {
        public void Access(IAccessRequest request)
        {
            // Allow everone to access this resource
            request.AccessGranted();
        }

        public void Get(IModelRequest request)
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

        public void Call(ICallRequest request)
        {
            Book book = BookStore.GetBook(request.ResourceName);
            if (book == null)
            {
                request.NotFound();
            }

            switch (request.Method)
            {
                // Handle setting of the book properties
                case "set":
                    Book bookParams = request.ParseParams<Book>();
                    var changed = new Dictionary<string, object>(2);

                    // Check if the title property was changed
                    if (bookParams.Title != null)
                    {
                        // Verify it is not empty
                        string title = bookParams.Title.Trim();
                        if (title == "")
                        {
                            request.InvalidParams("Title must not be empty");
                            return;
                        }

                        if (title != book.Title)
                        {
                            // Update the model.
                            book.Title = title;
                            changed["title"] = title;
                        }
                    }

                    // Check if the author property was changed
                    if (bookParams.Author != null)
                    {
                        // Verify it is not empty
                        string author = bookParams.Author.Trim();
                        if (author == "")
                        {
                            request.InvalidParams("Author must not be empty");
                            return;
                        }

                        if (author != book.Author)
                        {
                            // Update the model.
                            book.Author = author;
                            changed["author"] = author;
                        }
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
    }
}
