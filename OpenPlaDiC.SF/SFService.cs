using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenPlaDiC.Framework;
using Salesforce.Common;
using Salesforce.Force;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OpenPlaDiC.SF
{

    public interface ISFService
    {
        Task<Response<SFLoginResultModel>> AuthSFAsync(
            string consumerkey,
            string consumersecret,
            string username,
            string password,
            string sfUrl
            );

        Task<Response<DataTable>> GetSOQLAsync(string soqlCommand, SFLoginResultModel loginResult, string nextUrl = "");

    }
    public class SFService : ISFService
    {

        public async Task<Response<SFLoginResultModel>> AuthSFAsync(
            string consumerkey,
            string consumersecret,
            string username,
            string password,
            string sfUrl
            )
        {


            try
            {


                //create auth client to retrieve token
                var auth = new AuthenticationClient();

                //get back URL and token

                await auth.UsernamePasswordAsync(consumerkey, consumersecret, username, password, sfUrl).ConfigureAwait(false);


                var instanceUrl = auth.InstanceUrl;
                var accessToken = auth.AccessToken;
                var apiVersion = auth.ApiVersion;

                return new Response<SFLoginResultModel>() { IsSuccess = true, Data = new SFLoginResultModel() { InstanceUrl = auth.InstanceUrl, AccessToken = auth.AccessToken, ApiVersion = auth.ApiVersion } };


            }
            catch (Salesforce.Common.ForceAuthException ex)
            {
                return new Response<SFLoginResultModel>()
                {
                    IsException = true,
                    Message = ex.Message,
                    InnerException = ex.InnerException != null ? ex.InnerException.Message : "",
                    Value = consumerkey + " | " + consumersecret + " | " + username + " | " + password + " | " + sfUrl
                };
            }

        }


        public async Task<Response<DataTable>> GetSOQLAsync(string soqlCommand, SFLoginResultModel loginResult, string nextUrl = "")
        {
            try
            {

                Response<object> result = new Response<object>();

                result.IsSuccess = true;

                string strJSON = "";
                string res = "";
                DataTable tbRes = null;


                var client = new ForceClient(loginResult.InstanceUrl, loginResult.AccessToken, loginResult.ApiVersion);

                if (string.IsNullOrEmpty(nextUrl))
                {

                    //Toolkit handles all serialization
                    var resp = await client.QueryAllAsync<dynamic>(soqlCommand).ConfigureAwait(continueOnCapturedContext: false);

                    result.Data = resp;
                }
                else
                {
                    var respNext = await client.QueryContinuationAsync<dynamic>(nextUrl);
                    result.Data = respNext;

                }


                string str = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data);

                strJSON = str;

                SFData r = Newtonsoft.Json.JsonConvert.DeserializeObject<SFData>(str);

                res = Newtonsoft.Json.JsonConvert.SerializeObject(r);

                tbRes = Tabulate(res);

                return new Response<DataTable>() { IsSuccess = true, Flag = r.done, ExRef = (r.done ? "" : r.nextRecordsUrl.ToString()), Data = tbRes};


            }
            catch (Exception ex)
            {

                return new Response<DataTable>()
                {
                    IsException = true,
                    Message = ex.Message,
                    InnerException = ex.InnerException != null ? ex.InnerException.Message : "",
                    Value = soqlCommand + " | " + Newtonsoft.Json.JsonConvert.SerializeObject(loginResult) + " | " + nextUrl
                };
            }


        }

        private System.Data.DataTable Tabulate(string json)
        {
            var jsonLinq = JObject.Parse(json);

            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                    else
                    {

                        var r = GetExtraData(column.Name, column.Value);
                        foreach (var item in r)
                        {
                            cleanRow.Add(item.Name, item.Value);
                        }

                    }

                }

                trgArray.Add(cleanRow);
            }

            return JsonConvert.DeserializeObject<DataTable>(trgArray.ToString());
        }

        private List<SFExtraData> GetExtraData(string name, JToken data)
        {
            var list = new List<SFExtraData>();


            try
            {
                JObject d = data.Value<JObject>();

                foreach (JProperty column in d.Properties())
                {
                    if (column.Value is JValue)
                    {
                        if (column.Name != "url" && column.Name != "type")
                        {

                            list.Add(new SFExtraData(name + "_" + column.Name, column.Value));
                        }
                    }
                    else
                    {
                        list.AddRange(GetExtraData(column.Name, column.Value));
                    }

                }
            }
            catch (Exception ex)
            {

            }

            return list;

        }



    }
}
