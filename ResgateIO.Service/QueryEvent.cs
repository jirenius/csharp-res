using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    internal class QueryEvent
    {
        public IResourceContext Resource { get; }
        private QueryCallback callback;
        private IAsyncSubscription subscription;

        private ResService Service { get { return Resource.Service; } }

        public QueryEvent(IResourceContext resource, QueryCallback callback)
        {
            Resource = resource;
            this.callback = callback;
        }

        public bool Start()
        {
            var conn = Resource.Service.Connection;
            string qsubj = conn.NewInbox();

            try
            {
                subscription = conn.SubscribeAsync(qsubj, onMessage);
            }
            catch (Exception ex)
            {
                Service.OnError("Failed to subscribe to query event for {0}: {1}", Resource.ResourceName, ex.Message);
                closeCallback();
                return false;
            }

            Resource.Service.Send("event." + Resource.ResourceName + ".query", new QueryEventDto(qsubj));
            return true;
        }

        public void Stop()
        {
            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
            }
            Resource.Service.With(Resource, closeCallback);
        }

        private void closeCallback()
        {
            if (callback == null)
            {
                return;
            }
            try
            {
                callback(null);
            }
            catch (Exception ex)
            {
                Service.OnError("Failed calling query request callback with null for {0}: {1}", Resource.ResourceName, ex.Message);
            }
            callback = null;
        }

        private void onMessage(object sender, MsgHandlerEventArgs e)
        {
            Msg msg = e.Message;
            String subj = msg.Subject;

            Service.Log.Trace("Q=> {0}: {1}", subj, Encoding.UTF8.GetString(msg.Data));

            Resource.Service.With(Resource, () =>
            {
                if (callback == null)
                {
                    return;
                }

                var qr = new QueryRequest(Resource, msg);
                try
                {
                    QueryRequestDto reqInput = JsonUtils.Deserialize<QueryRequestDto>(msg.Data);
                    if (String.IsNullOrEmpty(reqInput.Query))
                    {
                        Service.OnError("Missing query in query request {0}", qr.ResourceName);
                        qr.RawResponse(ResService.ResponseMissingQuery);
                        return;
                    }
                    qr.SetQuery(reqInput.Query);
                }
                catch (Exception ex)
                {
                    Service.OnError("Error deserializing query request {0}: {1}", qr.ResourceName, ex.Message);
                    qr.RawResponse(ResService.ResponseBadRequest);
                    return;
                }

                try
                {
                    callback(qr);
                }
                catch(ResException ex)
                {
                    if (!qr.Replied)
                    {
                        // If a reply isn't sent yet, send an error response,
                        // as throwing a ResException within a query callback
                        // is considered valid behaviour.
                        qr.Error(new ResError(ex));
                        return;
                    }

                    Service.OnError("Error handling query request {0}: {1} - {2}", qr.ResourceName, ex.Code, ex.Message);
                }
                catch (Exception ex)
                {
                    if (!qr.Replied)
                    {
                        qr.Error(new ResError(ex));
                    }

                    // Write to log as only ResExceptions are considered valid behaviour.
                    Service.OnError("Error handling query request {0}: {1}", qr.ResourceName, ex.Message);
                    return;
                }
                if (qr.Replied)
                {
                    return;
                }

                if (qr.Events.Count == 0)
                {
                    qr.RawResponse(ResService.ResponseNoQueryEvents);
                }
                else
                {
                    qr.RawResponse(JsonUtils.Serialize(new SuccessDto(new QueryResponseDto(qr.Events))));
                }
            });
            
        }
    }
}
