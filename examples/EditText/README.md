# Edit Text Example

**Tags:** *Models*, *Call methods*, *Client subscriptions*

## Description
A simple text field that can be edited by multiple clients simultaneously.

## Prerequisite

* [Install](https://resgate.io/docs/get-started/installation/) *NATS Server* and *Resgate* (done with 3 docker commands).

## Install and run

```text
git clone https://github.com/jirenius/csharp-res
```
Open the solution, `examples\EditText\EditText.sln`, in Visual Studio 2017.

Set *multiple startup projects* with the projects below ([how to](https://docs.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2019)). Press F5 to build and run.

  * *TextService*
  * *WebClient*

Open the client
```text
http://localhost:8081
```

## Things to try out

**Realtime updates**  
Run the client in two separate tabs, edit the message in one tab, and observe realtime updates in both.

**System reset**  
Stop the service. Edit the default text in *MyModelHandler.cs*. Restart the service to observe resetting of the message in all clients.

## API

Request | Resource | Description
--- | --- | ---
*get* | `text.mymodel` | Simple model.
*call* | `text.mymodel.set` | Sets the model's *message* property.

## REST API

Resources can be retrieved using ordinary HTTP GET requests, and methods can be called using HTTP POST requests.

### Get model
```
GET http://localhost:8080/api/text/mymodel
```

### Update model
```
POST http://localhost:8080/api/text/mymodel/set
```
*Body*  
```
{ "message": "Updated through HTTP" }
```