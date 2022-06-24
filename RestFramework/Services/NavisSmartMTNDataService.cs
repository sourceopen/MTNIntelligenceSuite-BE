namespace NavisSmartRestGateway
{
    public interface IMTNDataService
    {
/*        HttpClient Client { get; }

        Task<MTNDataResponse<CustomerIdentificationTags>> GetComplexInfo();

        Task<MTNDataResponse<SystemInfoMTNResponse>> GetSystemInfo();

        Task<MTNDataResponse<SystemDataMTNResponse>> GetSystemData();

        Task<MTNDataResponse<TerminalDataMTNResponse>> GetTerminalData(string terminalUid);

        Task<MTNDataResponse<ProducerStatusMTNResponse>> UpdateProducerStatus(EMtnProducerStatus status, string statusMessage, Nullable<bool> wasSnapshotDone,
                                                        string lastSequence, string lastConnectTimeIso);
*/
    }

    public class NavisSmartMTNDataService : IMTNDataService
    {
  /*      public HttpClient Client { get; }
        private readonly ILogger<NavisSmartMTNDataService> _logger;
        private readonly IConfiguration _configuration;
        private static string _microserviceToken = string.Empty;
        private RestGatewaySevice _restGatewayService;
        private SequenceTracker _sequenceTracker;
        private int _sequenceNumberUpdateTimerInterval;
        private static bool _sequenceNumberUpdaterJobStarted = false;

        public NavisSmartMTNDataService(IConfiguration configuration, ILogger<NavisSmartMTNDataService> logger, HttpClient client, RestGatewaySevice inRestGatewayService,
                                        SequenceTracker insequenceTracker)
        {
            Client = client;
            _logger = logger;
            _configuration = configuration;

            _restGatewayService = inRestGatewayService;
            _sequenceTracker = insequenceTracker;

            _restGatewayService.WebserviceName = _configuration[StringConstants.SETTING_RESTSERVICE_NAME];
            if (_restGatewayService.WebserviceName == null)
            {
                _logger.LogCritical("Throwing missing key exception and shutting down service and since key " + StringConstants.SETTING_RESTSERVICE_NAME + " is missing!");
                throw new RequiredKeyMissingException(StringConstants.SETTING_RESTSERVICE_NAME);
            }

            _restGatewayService.WebservicePath = _configuration[StringConstants.SETTING_REST_ENDPOINT];
            if (_restGatewayService.WebservicePath == null)
            {
                _logger.LogCritical("Throwing missing key exception and shutting down service and since key " + StringConstants.SETTING_REST_ENDPOINT + " is missing!");
                throw new RequiredKeyMissingException(StringConstants.SETTING_REST_ENDPOINT);
            }

            _sequenceNumberUpdateTimerInterval = Convert.ToInt16(_configuration[StringConstants.SETTING_SEQ_UPDATE_INTERVAL] ?? StringConstants.SETTING_SEQ_UPDATE_INTERVAL_DEFAULT);

            if (!_sequenceNumberUpdaterJobStarted)
            {
                _sequenceNumberUpdaterJobStarted = true;
                startTimerToSendSequenceUpdates();
            }
        }

        #region Interface implementation

        /// <summary>
        /// Gets the secret token required to communicate with the Navis Smart Data Service
        /// </summary>
        private void GetMicroserviceToken()
        {
            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Get, MethodNames.MicroserviceToken, null, GetAuthToken());

            DoSendAsyncAndGetResponseBody<MTNCommunicationTokenMTNResponse>(request).ContinueWith(t =>
            {
                if (t.Result == null && t.Exception != null)
                {
                    throw t.Exception;
                }
                _microserviceToken = t.Result.Data.Token;
            }).Wait();
        }

        public Task<MTNDataResponse<SystemInfoMTNResponse>> GetSystemInfo()
        {
            if (string.IsNullOrEmpty(_microserviceToken))
            {
                GetMicroserviceToken();
            }

            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Get, MethodNames.SystemInfo, null, _microserviceToken);

            return DoSendAsyncAndGetResponseBody<SystemInfoMTNResponse>(request);
        }
        public Task<MTNDataResponse<CustomerIdentificationTags>> GetComplexInfo()
        {
            if (string.IsNullOrEmpty(_microserviceToken))
            {
                GetMicroserviceToken();
            }

            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Get, MethodNames.ComplexInfo, null, _microserviceToken);

            return DoSendAsyncAndGetResponseBody<CustomerIdentificationTags>(request);
        }
        public Task<MTNDataResponse<SystemDataMTNResponse>> GetSystemData()
        {
            if (string.IsNullOrEmpty(_microserviceToken))
            {
                GetMicroserviceToken();
            }

            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Get, MethodNames.SystemData, null, _microserviceToken);

            return DoSendAsyncAndGetResponseBody<SystemDataMTNResponse>(request);
        }

        public Task<MTNDataResponse<TerminalDataMTNResponse>> GetTerminalData(string terminalUid)
        {
            if (string.IsNullOrEmpty(_microserviceToken))
            {
                GetMicroserviceToken();
            }

            var queryParams = new Dictionary<string, string>
            {
                { StringConstants.TERM_OID, terminalUid }
            };

            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Get, MethodNames.TerminalData, queryParams, _microserviceToken);

            return DoSendAsyncAndGetResponseBody<TerminalDataMTNResponse>(request);
        }

        public Task<MTNDataResponse<ProducerStatusMTNResponse>> UpdateProducerStatus(EMtnProducerStatus status, string statusMessage, Nullable<bool> wasSnapshotDone,
                                                                            string lastSuccessfulSequence, string lastSuccesfulDateTime)
        {
            _logger.LogInformation("***UpdateProducerStatus - OpsView producer is now {0}***", status);

            if (string.IsNullOrEmpty(_microserviceToken))
            {
                GetMicroserviceToken();
            }

            ProducerStatusMTNRequest producerStatusRequest = new ProducerStatusMTNRequest(status, statusMessage, wasSnapshotDone, lastSuccessfulSequence, lastSuccesfulDateTime);

            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Post, MethodNames.ProducerStatus, null, _microserviceToken);
            request.Content = _restGatewayService.ConvertToJson(producerStatusRequest);

            return DoSendAsyncAndGetResponseBody<ProducerStatusMTNResponse>(request);
        }

        #endregion Interface implementation

        #region private methods

        private void startTimerToSendSequenceUpdates()
        {
            System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(_sequenceNumberUpdateTimerInterval).TotalMilliseconds);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.UpdateSequenceToMTN);
            timer.Start();
            _logger.LogInformation("The sequence updater job started to run at intervals of {0} minute(s)", _sequenceNumberUpdateTimerInterval);
        }

        private void UpdateSequenceToMTN(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (string.IsNullOrEmpty(_microserviceToken))
            {
                GetMicroserviceToken();
            }

            Tuple<ESeqFileErrorCodes,LatestSequenceDetailsMTNRequest> latestSequenceDetails = _sequenceTracker.readLatestSequenceDetails();

            var request = _restGatewayService.BuildRequestMessage(HttpMethod.Post, MethodNames.UpdateLatestSequenceDetail, null, _microserviceToken);
            request.Content = _restGatewayService.ConvertToJson(latestSequenceDetails.Item2);

            DoSendAsyncAndGetResponse(request).ContinueWith(reply =>
            {
                if (reply.Status.Equals(TaskStatus.RanToCompletion))
                {
                    /// this change for unit testing
                    /// During the unit test there is no ElapsedEventArgs so that aragument "e" will be null
                    /// To avoid the exception (of null) the conditional statement has included below in the logger methods parameter
                    if (reply.Result.IsSuccessStatusCode)
                        _logger.LogDebug("Updated MTN with the last successful sequence - successful sequence number sent is - {0} - , event happened at - {1} -. Update sent to MTN at - {2} -",
                                                                                                               latestSequenceDetails.Item2.lastSuccessfulSequence,
                                                                                                               latestSequenceDetails.Item2.lastSuccessfulDateTime,
                                                                                                               e == null ? DateTime.Now : e.SignalTime);
                    else
                        _logger.LogError("Could not update MTN with last successful sequence at {0}, reply status code was {1}. Check MTN running state!", (e == null ? DateTime.Now : e.SignalTime), reply.Result.StatusCode);
                }
                else
                {
                    _logger.LogError("MTN Update task could not run properly, could not update MTN with last successful sequence at {0}, Check MTN running state!", e.SignalTime);
                }
            });
        }



        private Task<MTNDataResponse<T>> DoSendAsyncAndGetResponseBody<T>(HttpRequestMessage httpRequest)
        {
            _logger.LogDebug("Attempting to asyncronously send request to URI : {0}\n with body : {1}", httpRequest.RequestUri, httpRequest.Content);

            return Client.SendAsync(httpRequest).ContinueWith((t) =>
            {
                MTNDataResponse<T> response = new MTNDataResponse<T>();
                t.Result.Content.ReadAsStringAsync().ContinueWith(x =>
                {
                    _logger.LogDebug("Received response body as - {0}, in response to request sent to URI - {1} - with body - {2}, ", x.Result, httpRequest.RequestUri, httpRequest.Content);
                    response.Source = x.Result;
                }).Wait();

                HandleResponse<T>(t.Result, response);
                return response;
            });
        }

        private Task<HttpResponseMessage> DoSendAsyncAndGetResponse(HttpRequestMessage httpRequest)
        {
            return Client.SendAsync(httpRequest);
        }

        /// <summary>
        /// Handle json reponse with errors (if any) before sending response to the caller
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponse"></param>
        /// <param name="response"></param>
        private void HandleResponse<T>(HttpResponseMessage httpResponse, MTNDataResponse<T> response)
        {
            try
            {
                JObject jsonResponse = null;

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(response.Source))
                        jsonResponse = JObject.Parse(response.Source);
                    else
                        _logger.LogError("response.Source was null or empty in method HandleResponse<{0}>! Cannot be parsed!", typeof(T).Name);

                    if(jsonResponse != null && jsonResponse[StringConstants.CONTENT] != null)
                        response.Data = JsonConvert.DeserializeObject<T>(Convert.ToString(jsonResponse[StringConstants.CONTENT]));
                }

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    response.Error = new ErrorResponse() { ErrorCode = httpResponse.StatusCode.ToString(), ErrorMessage = httpResponse.ReasonPhrase };
                    _logger.LogError("NavisSmartMTNDataService error  - {0}, code = {1}, message = {2}", response.Source, response.Error.ErrorCode, response.Error.ErrorMessage);

                    throw new NavisSmartMTNDataServiceException(string.Format("Error code : {0}, Error reason : {1}",
                            httpResponse.StatusCode.ToString(), httpResponse.ReasonPhrase));
                }
            }
            catch (JsonReaderException jEx)
            {
                _logger.LogError("NavisSmartMTNDataService error  - {0}", response.Source);
                _logger.LogError("NavisSmartMTNDataService exception - {0}", jEx.ToString());

                throw new NavisSmartMTNDataServiceException(string.Format("Error code : {0}, Error reason : {1}",
                    httpResponse.StatusCode.ToString(), httpResponse.ReasonPhrase), inner: jEx);
            }
        }

        private string GetAuthToken()
        {
            var listeningUri = _configuration[StringConstants.SETTING_REMOTE_LISTENING_URI] ?? _configuration[StringConstants.SETTING_LISTENING_URI];
            listeningUri += "OpsView/";
            return Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ServiceConstants.MicroserviceUserName}:{listeningUri}"));
        }

        private static class StringConstants
        {
            public const string SETTING_REMOTE_LISTENING_URI = "AppSettings:RemoteListeningUri";
            public const string SETTING_LISTENING_URI = "AppSettings:ListeningUri";
            public const string CONTENT = "content";
            public const string TYPE= "__type";
            public const string TERM_OID = "TermOid";
            public const string SETTING_RESTSERVICE_NAME = "AppSettings:RestServiceName";
            public const string SETTING_REST_ENDPOINT = "AppSettings:RestEndPoint";
            public const string SETTING_SEQ_UPDATE_INTERVAL = "AppSettings:SequenceUpdateInterval";
            public const string SETTING_SEQ_UPDATE_INTERVAL_DEFAULT = "30";
        }

        #endregion private methods
  */
    }
}