# Search Example

**Tags:** *Models*, *Collections*, *Linked resources*, *Queries*, *Pagination*, *Persistence*, *Call methods*, *Resource parameters*

## Description
A customer management system, where you can search and filter customers by name and country. The search results are live and updated as customers are edited, added, or deleted by multiple users simultaneously.

## Prerequisite

* [Install](https://resgate.io/docs/get-started/installation/) *NATS Server* and *Resgate* (done with 3 docker commands).

## Install and run

```text
git clone https://github.com/jirenius/csharp-res
```
Open the solution, `examples\Search\Search.sln`, in Visual Studio 2017.

Set *multiple startup projects* with the projects below ([how to](https://docs.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2019)). Press F5 to build and run.

  * *SearchService*
  * *WebClient*

Open the client
```text
http://localhost:8081
```

## Things to try out

**Live query**  
Make a query (eg. Set *Filter* to `B`, and *Country* to `Germany`) in one tab. In a separate tab, try to:
* create a *New customer* matching the query.
* edit a customer so that it starts to match the query.
* edit a customer so that it no longer matches the query.
* delete a customer that matches the query.

**Persistence**  
Run the client and make some changes. Restart the service and all changes should be persisted in the LiteDB database.

## API

Request | Resource | Description
--- | --- | ---
*get* | `search.customers?from=0&limit=5&name=A&country=Sweden` | Query collection of customer references. All query parameters are optional.
*call* | `search.customers.new` | Adds a new customer.
*get* | `search.customer.<ID>` | Models representing customers.
*call* | `search.customer.<ID>.set` | Sets the customers' *name*, *email*, and *country* properties.
*call* | `search.customer.<ID>.delete` | Deletes a customer.
*get* | `search.countries` | Collection of available countries.

## REST API

Resources can be retrieved using ordinary HTTP GET requests, and methods can be called using HTTP POST requests.

### Get customer query collection (all parameters are optional)
```
GET http://localhost:8080/api/search/customers?from=0&limit=5&name=A&country=Sweden
```

### Add new customer
```
POST http://localhost:8080/api/search/customers/new
```
*Body*  
```
{ "name": "John Doe", "email": "john.doe@example.com", "country": "France" }
```

### Get customer
```
GET http://localhost:8080/api/search/customer/<ID>
```

### Update customer properties
```
POST http://localhost:8080/api/search/customer/<ID>/set
```
*Body*  
```
{ "name": "Jane Doe", "country": "United Kingdom" }
```

### Delete customer
```
POST http://localhost:8080/api/search/customer/<ID>/delete
```
*No body*

### Get country collection
```
GET http://localhost:8080/api/search/countries
```