using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
//using Xamarin.Android.Net;
using static SparePartsApp.Helper;

namespace SparePartsApp.Services
{
    public abstract class BaseRestService<T> where T : class
    {
        protected string baseAddress { get { return Helper.Server; } }

        public string BaseUrl { get { return $"http://{baseAddress}/{Controller}"; } }

        public virtual string Controller { get; } = typeof(T).Name.ToString();

        public void ShowLoading()
        {
            if (!Helper.HasWifi)
            {
                throw new Exception("Rede wifi indisponível!");
            }

            var ip = baseAddress.Split('/')[0].Split(':')[0];

            //if (new Ping().Send(ip).Status != IPStatus.Success)
            //{
            //    throw new Exception("Servidor web indisponível: " + ip);
            //}

            if (!Helper.HasWebServerAvaiable($"http://{baseAddress}"))
            {
                throw new Exception("Servidor web indisponível: " + ip);
            }

            Xamarin.Forms.MessagingCenter.Send((App)Xamarin.Forms.Application.Current, "ShowLoading");
        }

        public void HideLoading()
        {
            Xamarin.Forms.MessagingCenter.Send((App)Xamarin.Forms.Application.Current, "HideLoading");
        }

        private bool ServerIsAvailable()
        {
            try
            {
                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) })
                {
                    var response = httpClient.GetStringAsync(baseAddress);

                    return response.Result.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public virtual async Task<string> Post(T obj, string action = "")
        {
            try
            {
                ShowLoading();

                var handler = new Xamarin.Android.Net.AndroidClientHandler();
                handler.ConnectTimeout = TimeSpan.FromMinutes(10);
                handler.ReadTimeout = TimeSpan.FromMinutes(10);

                using (var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) })
                {
                    var body = JsonConvert.SerializeObject(obj);
                    var stringContent = new StringContent(body, Encoding.UTF8, "application/json");

                    var requestUri = Path.Combine(BaseUrl, action).Replace("\\", "/");

                    var response = await httpClient.PostAsync(requestUri, stringContent);
                    var message = string.Empty;

                    using (HttpContent content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        message = result;
                    }

                    return message.Trim('"', '\\').Replace("\\r", "\r").Replace("\\n", "\n");
                }
            }
            catch (Exception ex)
            {
                return ex.GetInnerExceptionMessage();
            }
            finally
            {
                HideLoading();
            }
        }

        public virtual async Task<string> Put(T obj, uint id, string action = "")
        {
            try
            {
                ShowLoading();

                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(15) })
                {
                    var body = JsonConvert.SerializeObject(obj);
                    var stringContent = new StringContent(body, Encoding.UTF8, "application/json");

                    var requestUri = Path.Combine(BaseUrl, action, id.ToString()).Replace("\\", "/");

                    var response = await httpClient.PutAsync(requestUri, stringContent);
                    var message = string.Empty;

                    using (HttpContent content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        message = result;
                    }

                    return message.Trim('"', '\\').Replace("\\r", "\r").Replace("\\n", "\n");
                }
            }
            catch (Exception ex)
            {
                return ex.GetInnerExceptionMessage();
            }
            finally
            {
                HideLoading();
            }
        }

        public virtual async Task<string> Delete(uint id, string action = "")
        {
            try
            {
                ShowLoading();

                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(15) })
                {
                    var requestUri = Path.Combine(BaseUrl, action, id.ToString()).Replace("\\", "/");

                    var response = await httpClient.DeleteAsync(requestUri);
                    var message = string.Empty;

                    using (HttpContent content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        message = result;
                    }

                    return message.Trim('"', '\\').Replace("\\r", "\r").Replace("\\n", "\n");
                }
            }
            catch (Exception ex)
            {
                return ex.GetInnerExceptionMessage();
            }
            finally
            {
                HideLoading();
            }
        }

        public virtual async Task<T> Get(string action = "")
        {
            try
            {
                ShowLoading();

                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(15) })
                {
                    var requestUri = Path.Combine(BaseUrl, action).Replace("\\", "/");

                    var response = await httpClient.GetAsync(requestUri);

                    var contentResult = string.Empty;

                    using (HttpContent content = response.Content)
                    {
                        contentResult = await content.ReadAsStringAsync();
                    }

                    var result = JsonConvert.DeserializeObject<T>(contentResult);

                    return result;
                }
            }
            catch (JsonReaderException)
            {
                throw new Exception($"Servidor web indisponível: {Helper.Server}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetInnerExceptionMessage());
            }
            finally
            {
                HideLoading();
            }
        }

        public virtual async Task<IEnumerable<T>> GetList(string action = "")
        {
            try
            {
                ShowLoading();

                using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(15) })
                {
                    var requestUri = Path.Combine(BaseUrl, action).Replace("\\", "/");

                    var json = await httpClient.GetStringAsync(requestUri);

                    var result = JsonConvert.DeserializeObject<IEnumerable<T>>(json);

                    return result;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetInnerExceptionMessage());
            }
            finally
            {
                HideLoading();
            }
        }

        public void FileUpload(string action, string filePath)
        {
            try
            {
                ShowLoading();

                using (HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(15) })
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(filePath))
                using (StreamContent fileContent = new StreamContent(fileStream))
                {
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        FileName = filePath
                    };

                    var requestUri = Path.Combine(BaseUrl, action).Replace("\\", "/");

                    fileContent.Headers.Add("name", Path.GetFileName(filePath));
                    content.Add(fileContent);

                    var result = httpClient.PostAsync(requestUri, content).Result;

                    result.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetInnerExceptionMessage());
            }
            finally
            {
                HideLoading();
            }
        }

    }

}
