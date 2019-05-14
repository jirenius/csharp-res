# Hello World Example

This is an example of a simple text field that can be edited by multiple clients.

* It exposes a single resource: `example.mymodel`.
* It allows setting the resource's `message` property through the `set` method.
* It resets the model on server restart.
* It serves a web client at http://localhost:8081

## Prerequisite

* Have [NATS Server](https://nats.io/download/nats-io/gnatsd/) and [Resgate](https://github.com/resgateio/resgate) running

## Install and run

Clone csharp-res repository:
```text
git clone https://github.com/jirenius/csharp-res
```
Open the solution, `ResgateIO.Service.sln`, in Visual Studio 2017.

In the *Solution Explorer*, right click *HelloWorld* and choose *Set as StartUp Project*. Press F5 to build and run.

Open the client
```text
http://localhost:8081
```

## Things to try out

**Realtime updates**  
Run the client in two separate tabs, edit the message in one tab, and observe realtime updates in both.

## Web resources

Resources can be retrieved using ordinary HTTP GET requests, and methods can be called using HTTP POST requests.

### Get model
```
GET http://localhost:8080/api/exampleService/myModel
```

### Update model
```
POST http://localhost:8080/api/exampleService/myModel/set
```
*Body*  
```
{ "message": "Updated through HTTP" }
```