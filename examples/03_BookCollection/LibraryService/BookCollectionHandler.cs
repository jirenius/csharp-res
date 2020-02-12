using ResgateIO.Service;
using System;

namespace LibraryService
{
    internal class BookCollectionHandler : BaseHandler
    {
        public void Access(IAccessRequest request)
        {
            // Allow everone to access this resource
            request.AccessGranted();
        }

        public void Get(ICollectionRequest request)
        {
            // Pass the entire list of book references
            request.Collection(BookStore.GetBookList());
        }

        public void New(ICallRequest request)
        {
            Book newParams = request.ParseParams<Book>();

            // Validate that we received both title and author
            if (String.IsNullOrEmpty(newParams.Title) || String.IsNullOrEmpty(newParams.Author))
            {
                request.InvalidParams("Must provide both title and author");
                return;
            }

            // Add a new book to the store
            var add = BookStore.AddBook(newParams.Title, newParams.Author);
            // Send add event
            request.AddEvent(add.Ref, add.Idx);

            // Respond with a resource reference to the newly created book model
            request.Resource(add.Ref.ResourceID);
        }

        [CallMethod("delete")]
        public void Delete(ICallRequest request)
        {
            // Unmarshal book ID params to a Book model to easily extract the resource ID.
            Book deleteParams = request.ParseParams<Book>();

            int idx = BookStore.DeleteBook(deleteParams.ResourceID);
            if (idx > -1)
            {
                request.RemoveEvent(idx);
            }

            // Send success response. It is up to the service to define if a delete
            // should be idempotent or not. In this case we send success regardless
            // if the book existed or not, making it idempotent.
            request.Ok();
        }
    }
}