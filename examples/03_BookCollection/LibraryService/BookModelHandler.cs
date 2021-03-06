﻿using ResgateIO.Service;
using System.Collections.Generic;

namespace LibraryService
{
    class BookModelHandler : BaseHandler
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

        [CallMethod("set")]
        public void Set(ICallRequest request)
        {
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
        }

        public void ApplyChange(IResourceContext resource, ChangeEventArgs  ev)
        {
            var book = resource.RequireValue<Book>();
            var revert = new Dictionary<string, object>(ev.ChangedProperties.Count);

            if (ev.ChangedProperties.TryGetValue("title", out object titleObject))
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

            if (ev.ChangedProperties.TryGetValue("author", out object authorObject))
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

            ev.SetRevert(revert);
        }
    }
}
