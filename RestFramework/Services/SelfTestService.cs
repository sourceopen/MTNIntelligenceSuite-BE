using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Options;

namespace NavisSmartRestGateway.Service.SelfTest
{
    public class SelfTestService
    {
        /*
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SelfTestService> _logger;
        private readonly string _kafkaBrokerEndpoint;
        private readonly AdminClientBuilder _adminClientBuilder;
        private string SEQUENCE_FILE_NAME_WITH_PATH = string.Empty;
//        private readonly KafkaConfig _kafkaConfig;

        public SelfTestService(IConfiguration inConfiguration, IServiceProvider inserviceProvider, ILogger<SelfTestService> inLogger, AdminClientBuilder adminClientBuilder,
                                IOptions<KafkaConfig> inKafkaConfig)
        {
            _configuration = inConfiguration;
            _serviceProvider = inserviceProvider;
            _logger = inLogger;
            SEQUENCE_FILE_NAME_WITH_PATH = _configuration["AppSettings:SequenceFileNameWithPath"];
            _kafkaBrokerEndpoint = _configuration["KafkaConfig:ServerEndpoint"];
            _adminClientBuilder = adminClientBuilder;
            _kafkaConfig = inKafkaConfig.Value;

            _logger.LogTrace("Started SelfTestService!");
        }

        public List<Tuple<string, bool, string>> performSelfTest()
        {
            List<Tuple<string, bool, string>> tempResult = new List<Tuple<string, bool, string>>();

            _logger.LogTrace("Starting Self test..");

            tempResult.Add(TestEssentialKeys());
            tempResult.Add(TestKafkaConnectivityAndTopicsExistence());
            tempResult.Add(TestWritabilityForSeqFileDir());
            tempResult.Add(TestRESTToMTN());

            return tempResult;
        }

        private Tuple<string, bool, string> TestEssentialKeys()
        {
            _logger.LogTrace("Checking for essential keys..");

            string missingKeys = string.Empty;

            if (_configuration[StringConstants.APPSETTING_SEQ_FILE_NAME_PATH] == null)
                missingKeys += StringConstants.APPSETTING_SEQ_FILE_NAME_PATH + StringConstants.COMMA;
            if (_configuration[StringConstants.APPSETTING_LISTENING_URI] == null)
                missingKeys += StringConstants.APPSETTING_LISTENING_URI + StringConstants.COMMA;
            if (_configuration[StringConstants.APPSETTING_RESTENDPOINT] == null)
                missingKeys += StringConstants.APPSETTING_RESTENDPOINT + StringConstants.COMMA;
            if (_configuration[StringConstants.APPSETTING_RESTSERVICENAME] == null)
                missingKeys += StringConstants.APPSETTING_RESTSERVICENAME + StringConstants.COMMA;
            if (_configuration[StringConstants.APPSETTING_SERVICE_ENDPOINT] == null)
                missingKeys += StringConstants.APPSETTING_SERVICE_ENDPOINT + StringConstants.COMMA;

            Boolean resultBool = true;
            string resultString = StringConstants.MISSING_KEYS + StringConstants.NONE;

            if (!missingKeys.Equals(string.Empty))
            {
                resultBool = false;
                resultString = StringConstants.MISSING_KEYS + missingKeys;
            }

            _logger.LogTrace("------------> TestEssentialKeys -> {0}", resultBool == true ? "PASSED" : "FAILED");
            if(!resultBool)
                _logger.LogError("{0}", resultString);

            return Tuple.Create("TestEssentialKeys", resultBool, resultString);
        }

        private class StringConstants
        {
            public const string COMMA = ",";
            public const string APPSETTING_SEQ_FILE_NAME_PATH = "AppSettings:SequenceFileNameWithPath";
            public const string APPSETTING_LISTENING_URI = "AppSettings:ListeningUri";
            public const string APPSETTING_RESTENDPOINT = "AppSettings:RestEndPoint";
            public const string APPSETTING_RESTSERVICENAME = "AppSettings:RestServiceName";
            public const string APPSETTING_SERVICE_ENDPOINT = "KafkaConfig:ServerEndpoint";
            public const string APPSETTING_ENABLE_SNMP = "SnmpTrapConfig:EnableSnmpTrap";
            public const string APPSETTING_SNMP_HOST = "SnmpTrapConfig:SnmpHost";
            public const string APPSETTING_SNMP_PORT = "SnmpTrapConfig:SnmpPort";
            public const string MISSING_KEYS = "MISSING KEYS: ";
            public const string NONE = "NONE";
        }

        private Tuple<string, bool, string> TestKafkaConnectivityAndTopicsExistence()
        {
            _logger.LogTrace("Testing Kafka Connection..");

            Boolean resultBool = false;
            string resultString = string.Empty;
            Tuple<string, bool, string> result;

            try
            {
                //adminClientBuilder moved to class level
                using (var adminClient = _adminClientBuilder.Build())
                {
                    Metadata metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                    List<TopicMetadata> topicsMetadata = metadata.Topics;
                    List<string> topicNames = metadata.Topics.Select(a => a.Topic).ToList();
                    if (topicNames.Contains(_kafkaConfig.IncrementalTopic) && topicNames.Contains(_kafkaConfig.SnapshotTopic))
                        resultBool = true; //success
                    else
                        resultString += "Snapshot/Incremental topic(s) missing, Please create necessary topics.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("TestKafkaConnectivityAndTopicsExistence failed, Exception - {0}", ex.ToString());
                resultString += ex.ToString();
                resultBool = false;
            }

            _logger.LogTrace("------------> TestKafkaConnectivityAndTopicsExistence -> {0}", resultBool == true ? "PASSED" : "FAILED");
            if (!resultBool)
                _logger.LogTrace("{0}", resultString);

            result = Tuple.Create("TestKafkaConnectivityAndTopicsExistence", resultBool, resultString);
            return result;
        }

        private Tuple<string, bool, string> TestWritabilityForSeqFileDir()
        {
            _logger.LogTrace("Testing writability to seq file dir..");

            bool resultBool = false;
            string resultString = string.Empty;
            Tuple<string, bool, string> result;

            string path = Path.GetDirectoryName(SEQUENCE_FILE_NAME_WITH_PATH);

            //check if directory exists
            if (!System.IO.Directory.Exists(path))
            {
                resultBool = false;
                resultString = "The directory path " + SEQUENCE_FILE_NAME_WITH_PATH + " does not exist.";
                result = Tuple.Create("TestWritabilityForSeqFileDir", resultBool, resultString);

                _logger.LogDebug("------------> TestWritabilityForSeqFileDir -> FAILED -> {0}", resultString);

                return result;
            }

            //test writability to the path where seq file will exist
            //create a random file in the directory and check if it is writable
            try
            {
                FileStream fs = File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                fs.Close();
                //success
            }
            catch (Exception ex)
            {
                resultBool = false;
                resultString = "Exception thrown in TestWritabilityForSeqFileDir. Exception is " + ex.ToString();
                result = Tuple.Create("TestWritabilityForSeqFileDir", resultBool, resultString);

                _logger.LogDebug("------------> TestWritabilityForSeqFileDir -> FAILED -> {0}", resultString);

                return result;
            }

            _logger.LogTrace("------------> TestWritabilityForSeqFileDir -> PASSED");

            result = Tuple.Create("TestWritabilityForSeqFileDir", true, "SUCCESS");
            return result;
        }

        private Tuple<string, bool, string> TestRESTToMTN()
        {
            _logger.LogTrace("Testing REST to MTN..");

            Tuple<string, bool, string> result = Tuple.Create("TestRESTToMTN", true, string.Empty);
            bool resultBool = false;
            string resultString = string.Empty;

            using (var scope = _serviceProvider.CreateScope())
            {
                var mtnDataService = scope.ServiceProvider.GetRequiredService<IMTNDataService>();

                mtnDataService.GetSystemInfo().ContinueWith(t =>
                {
                    if (t.Result.Source != null)
                    {
                        try
                        {
                            JObject jsonResponse = JObject.Parse(t.Result.Source);

                            if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.ToString()))
                            {
                                SystemInfoMTNResponse response = jsonResponse.ToObject<SystemInfoMTNResponse>();
                               if(response != null && response.AboutDetails != null &&
                                    response.AboutDetails["jmtVersion"] != null &&
                                    response.AboutDetails["jmtVersion"].ToString().Length != 0)
                                {
                                    //success
                                    _logger.LogTrace("The version of MTN running is : {0}", response.AboutDetails["jmtVersion"].ToString());
                                    resultBool = true;
                                    result = Tuple.Create("TestRESTToMTN", true, "SUCCESS");
                                }
                                else
                                {
                                    _logger.LogError("response.Type was {0}", response.Type);
                                    resultBool = false;
                                    resultString = "response.Type was " + response.Type;
                                    result = Tuple.Create("TestRESTToMTN", resultBool, resultString);
                                }
                            }
                            else
                            {
                                _logger.LogError("jsonResponse was {0}", jsonResponse);
                                resultBool = false;
                                resultString = "jsonResponse was " + jsonResponse;
                                result = Tuple.Create("TestRESTToMTN", resultBool, resultString);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Response source was {0}, Exception thrown while trying to sent REST req to MTN -> {1}", t.Result.Source, ex.ToString());
                            resultBool = false;
                            resultString = "Response source was " + t.Result.Source + ", Exception thrown while trying to sent REST req to MTN Exception -> {0}" + ex.ToString();
                            result = Tuple.Create("TestRESTToMTN", resultBool, resultString);
                        }
                    }
                    else
                    {
                        _logger.LogError("Response source was null while trying to get Info in TestRESTToMTN.");
                        resultBool = false;
                        resultString = "Response source was null while trying to get Info in TestRESTToMTN.";
                        result = Tuple.Create("TestRESTToMTN", resultBool, resultString);
                    }
                }).Wait();
            }

            _logger.LogTrace("------------> TestRESTToMTN -> {0}", resultBool == true ? "PASSED" : "FAILED");
            if (!resultBool)
                _logger.LogTrace("{0}", resultString);

            return result;
        }

        */
    }
}