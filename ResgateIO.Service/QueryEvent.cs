using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    internal class QueryEvent
    {
        public IResourceContext Resource { get; }
        private QueryCallBack callback;
        private IAsyncSubscription subscription;

        private ILogger Log { get { return Resource.Service.Log; } }

        public QueryEvent(IResourceContext resource, QueryCallBack callback)
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
                Log.Error(String.Format("Failed to subscribe to query event for {0}: {1}", Resource.ResourceName, ex.Message));
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
            Resource.Service.RunWith(Resource.ResourceName, () =>
            {
                closeCallback();
            });
        }

        private void closeCallback()
        {
            if (callback == null)
            {
                return;
            }
            callback(null);
            callback = null;
        }

        private void onMessage(object sender, MsgHandlerEventArgs e)
        {
            Msg msg = e.Message;
            String subj = msg.Subject;

            Log.Trace(String.Format("Q=> {0}: {1}", subj, Encoding.UTF8.GetString(msg.Data)));

            Resource.Service.RunWith(Resource.ResourceName, () =>
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
                        Log.Trace(String.Format("Missing query in query request {0}", qr.ResourceName));
                        qr.RawResponse(ResService.ResponseMissingQuery);
                        return;
                    }
                    qr.SetQuery(reqInput.Query);
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Error deserializing query request {0}: {1}", qr.ResourceName, ex.Message));
                    qr.RawResponse(ResService.ResponseBadRequest);
                    return;
                }
               
                callback(qr);
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
