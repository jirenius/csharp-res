using ResgateIO.Service;
using System;
using System.Collections.Generic;

namespace BookCollection
{
    static class BookStore
    {
        private static readonly Dictionary<string, Book> bookModels = new Dictionary<string, Book>
        {
            { "library.book.1", new Book(1, "Animal Farm", "George Orwell") },
            { "library.book.2", new Book(2, "Brave New World", "Aldous Huxley") },
            { "library.book.3", new Book(3, "Coraline", "Neil Gaiman") }
        };

        private static readonly List<Ref> bookList = new List<Ref>
        {
            new Ref("library.book.1"),
            new Ref("library.book.2"),
            new Ref("library.book.3")
        };

        private static int nextBookID = 4;

        private static object storeLock = new object();

        /// <summary>
        /// Gets a book based on its resource ID.
        /// Returns null if the book wasn't found.
        /// </summary>
        public static Book GetBook(string rid)
        {
            lock (storeLock)
            {
                bookModels.TryGetValue(rid, out Book book);
                return book;
            }
        }

        /// <summary>
        /// Gets the list of book references
        /// </summary>
        public static List<Ref> GetBookList()
        {
            return bookList;
        }

        /// <summary>
        /// Creates a new book and adds it to the list.
        /// </summary>
        /// <returns>Index in the list where it was added, and the added book reference.</returns>
        public static (int Idx, Ref Ref) AddBook(string title, string author)
        {
            lock (storeLock)
            {
                // Create a new book
                Book book = new Book(nextBookID, title, author);
                nextBookID++;

                // Add it to the bookModels dictonary
                bookModels[book.ResourceID] = book;

                // Add it last to the bookList
                Ref bookRef = new Ref(book.ResourceID);
                bookList.Add(bookRef);
                return (bookList.Count - 1, bookRef);
            }
        }

        /// <summary>
        /// Delete a book from the store.
        /// </summary>
        /// <returns>Index in the list where it was before deletion, or -1 if the book wasn't found</returns>
        public static int DeleteBook(string rid)
        {
            lock (storeLock)
            {
                // Find the book in the list
                int idx = bookList.FindIndex((bookRef) => bookRef.ResourceID == rid);
                
                if (idx > -1)
                {
                    // Delete the book
                    bookModels.Remove(rid);
                    bookList.RemoveAt(idx);
                }

                return idx;
            }
        }
    }
}