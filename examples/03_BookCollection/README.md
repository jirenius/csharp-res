# Book Collection Example

**Tags:** *Models*, *Collections*, *Linked resources*, *Call methods*, *Resource parameters*

## Description
A simple library management system, listing books by title and author. Books can be edited, added, or deleted by multiple users simultaneously.

## Prerequisite

* [Install](https://resgate.io/docs/get-started/installation/) *NATS Server* and *Resgate* (done with 3 docker commands).

## Install and run

```text
git clone https://github.com/jirenius/csharp-res
```
Open the solution, `examples\03_BookCollection\BookCollection.sln`, in Visual Studio 2017.

Set *multiple startup projects* with the projects below ([how to](https://docs.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2019)). Press F5 to build and run.

  * *LibraryService*
  * *WebClient*

Open the client
```text
http://localhost:8081
```


## Things to try out

### Realtime updates
* Open the client in two separate tabs.
* Add/Edit/Delete entries to observe realtime updates.

### System reset
* Open the client and make some changes.
* Restart the service to observe resetting of resources in all clients.

### Resynchronization
* Open the client on two separate devices.
* Disconnect one device.
* Make changes using the other device.
* Reconnect the first device to observe resynchronization.

## API

Request | Resource | Description
--- | --- | ---
*get* | `library.books` | Collection of book model references.
*call* | `library.books.new` | Creates a new book.
*get* | `library.book.<BOOK_ID>` | Models representing books.
*call* | `library.book.<BOOK_ID>.set` | Sets the books' *title* and *author* properties.
*call* | `library.book.<BOOK_ID>.delete` | Deletes a book.

## REST API

Resources can be retrieved using ordinary HTTP GET requests, and methods can be called using HTTP POST requests.

### Get book collection
```
GET http://localhost:8080/api/library/books
```

### Get book
```
GET http://localhost:8080/api/library/book/<BOOK_ID>
```

### Update book properties
```
POST http://localhost:8080/api/library/book/<BOOK_ID>/set
```
*Body*  
```
{ "title": "Animal Farming" }
```

### Add new book
```
POST http://localhost:8080/api/library/books/new
```
*Body*  
```
{ "title": "Dracula", "author": "Bram Stoker" }
```

### Delete book
```
POST http://localhost:8080/api/library/books/delete
```
*Body*  
```
{ "id": <BOOK_ID> }
```