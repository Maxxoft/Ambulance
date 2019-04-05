using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using ModernHttpClient;
using Xamarin.Forms;
using Ambulance.ObjectModel;
using Ambulance.Dependency;
using Ambulance.Data;
using Ambulance.Extensions;
using System.Globalization;

namespace Ambulance.WebService
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ApiCommandAttribute : Attribute
    {
        public ApiCommandAttribute(string formatString, int paramCount = 0)
        {
            FormatString = formatString;
            ParamCount = paramCount;
        }

        public string FormatString { get; set; }
        public int ParamCount { get; set; }
    }

    public class ApiOrder
    {
        public int orderID { get; set; }                // номер заказа, целое число
        public string orderAddressFrom { get; set; }    // адрес откуда забирать пациента, текст
        public float orderFromLat { get; set; }         // координаты широты расположения пациента, дробное число
        public float orderFromLng { get; set; }         // координаты долготы расположения пациента, дробное число
        public string orderAddressTo { get; set; }      // адрес куда везти пациента(медучреждение), текст
        public float orderToLat { get; set; }           // координаты широты медучреждения, дробное число 
        public float orderToLng { get; set; }           // координаты долготы медучреждения, дробное число
        public string orderArrivalDate { get; set; }  // дата и время во сколько нужно быть у пациента, дата - формат : день-месяц-год часы-минуты 
	    public string orderSickName { get; set; }       // ФИО пациента, текст
	    public string orderSickPhone { get; set; }      // Телефон пациента, текст
	    public string orderAutoData { get; set; }       // информация по автомобилю, текст
	    public string orderComment { get; set; }        // комментарий к заявке, текст
        public int orderStatus { get; set; }
    }

    public class ApiCrew
    {
        public string CrewName { get; set; }            // название медбригады, текст
        public string CrewPhone { get; set; }           // телефон медбригады - текст
        public int CrewAcriveOrder { get; set; }        // идентификато активной заявки, число целое
    }

    public class ApiCrewType
    {
        public int crewTypeID { get; set; }            
        public string crewTypeName { get; set; }          
    }

    public class ApiResponse
    {
        public string Result { get; set; }
        public string Descriptor { get; set; }
        public string Error { get; set; }

        public string CrewName { get; set; }
        public string CrewPhone { get; set; }
        public int CrewAcriveOrder { get; set; }
        public List<ApiOrder> Order { get; set; }
        public List<List<ApiOrder>> Orders { get; set; }
        public List<List<ApiCrewType>> CrewTypes { get; set; }
        public int OrderNumber { get; set; }
        public int CrewState { get; set; }

        public bool BoolResult { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ErrorMessage { get; set; }
        public bool RequestDone { get; set; }
    }

    public enum ApiCommand
    {
        [ApiCommand("vCMD=Auth&CrewPhone={0}&CrewPin={1}", 2)]
        Auth,
        [ApiCommand("vCMD=OrderDetails&OrderID={0}", 1)]
        OrderDetails,
        [ApiCommand("vCMD=vAllOrdersWithState0", 0)]
        NewOrders,
        [ApiCommand("vCMD=ActiveOrder", 0)]
        ActiveOrder,
        [ApiCommand("vCMD=AutoPositionUpdate&AutoLat={0}&AutoLng={1}", 2)]
        UpdateLocation,
        [ApiCommand("vCMD=OrderStatusUpdate&OrderID={0}&OrderStatus={1}", 2)]
        UpdateOrderStatus,
        [ApiCommand("vCMD=getCrewTypes", 0)]
        GetCrewTypes,
        [ApiCommand("vCMD=setCrewTypeSelect&crewTypeID={0}", 1)]
        SetCrewType,
        [ApiCommand("vCMD=OrderCancel&OrderID={0}&OrderCancelComment={1}", 2)]
        CancelOrder,
        [ApiCommand("vCMD=setCrewState&StateValue={0}", 1)]
        SetCrewState
    }

    public static class ApiService
    {
        const string API_URL = "http://med03.com/api/API.php?";
        const string MSG_ERROR_INCORRECT_RESPONSE = "Ошибка разбора ответа сервера. Обратитесь в службу поддержки или попробуйте повторить запрос позднее";

        static NativeMessageHandler httpMessageHandler;
        static NativeCookieHandler httpCookieHandler;
        public static HttpClient HttpClient { get; private set; }

        public static readonly List<ApiResponse> ReqLog = new List<ApiResponse>();
        public static bool TestMode { get; set; }

        static string fcmToken;
        static bool fcmTokenChanged;
        public static string FCMToken
        {
            get { return fcmToken; }
            set { if (fcmToken == value) return; fcmToken = value; fcmTokenChanged = true; }
        }

        public static async void CloseSession()
        {
            while (isBusy)
                await Task.Delay(50);
            try
            {
                isBusy = true;
                HttpClient = null;
                httpCookieHandler = null;
                httpMessageHandler = null;
                DependencyService.Get<IAPIHelper>().ClearCookies();
                AppData.Crew = null;
            }
            finally
            {
                isBusy = false;
            }
        }

        static bool isBusy;
        static readonly object reqLock = new object();
        public static async Task<ApiResponse> ExecuteCommand(ApiCommand command, string[] pars)
        {
            var response = new ApiResponse();
            ReqLog.Add(response);

            // Get command string from commsnd's attribute
            var apiAttr = command.GetAttribute<ApiCommandAttribute>();
            if (apiAttr.ParamCount != pars.Length)
            {
                response.ErrorMessage = "Ошибка формирования запроса к серверу.";
                return response;
            }

            string url = API_URL;
            
            response.Request = url + string.Format(apiAttr.FormatString, pars);
            
            while (isBusy)
                await Task.Delay(200);

            try
            {
                isBusy = true;

                if (HttpClient == null)
                {
                    httpCookieHandler = new NativeCookieHandler();
                    httpMessageHandler = new NativeMessageHandler(true, false, httpCookieHandler);
                    httpMessageHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                    HttpClient = new HttpClient(httpMessageHandler, false);
                }

                try
                {
                    Debug.WriteLine("Request: {0}", response.Request);
                    var res = await HttpClient.GetAsync(response.Request).ConfigureAwait(false); 
                    Debug.WriteLine("Read content");
                    response.Response = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //response.response = await httpClient.GetStringAsync(response.request);
                    Debug.WriteLine("Response: {0}", response.Response);
                    // Обработка напильником, блять!
                    //response.response = response.response.Replace("\"vUser\":[],", string.Empty);

                    if (string.IsNullOrEmpty(response.Response))
                    {
                        response.ErrorMessage = "Сервер не отвечает, попробуйте повторить позже";
                        return response;
                    }
                    response.RequestDone = true;
                }
                catch (Exception e)
                {
                    response.BoolResult = false;
                    response.ErrorMessage = "Ошибка отправки запроса на сервер " + Environment.NewLine + e.Message;
                    //GoogleAnalyticsHelper.SendException("MFService:ExecuteCommand " + e.Message, false);
                    return response;
                }

                try
                {
                    long x = 0;
                    JsonConvert.PopulateObject(response.Response, response);
                    response.BoolResult = response?.Result?.ToUpper() == "TRUE" || response?.Result?.ToUpper() == "OK";
                    if (!response.BoolResult)
                    {
                        if (!string.IsNullOrEmpty(response.Error))
                            response.ErrorMessage = response.Error;
                        else if (string.IsNullOrEmpty(response.ErrorMessage))
                            response.ErrorMessage = "Запрос к серверу вернул отрицательный результат";
                    }
                    return response;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    response.BoolResult = false;
                    response.ErrorMessage = "Ошибка при разборе ответа от сервера";
                    //GoogleAnalyticsHelper.SendException("MFService:ExecuteCommand " + e.Message, false);
                    return response;
                }
            }
            finally
            {
                isBusy = false;
            }
        }

        public static async Task<string> Auth(string phone, string pin)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pin)) return "Некорректные данные";
            
            var res = await ExecuteCommand(ApiCommand.Auth, new string[] { phone, pin });
            if (!res.BoolResult) return res.ErrorMessage;

            var crew = new Crew();
            if (string.IsNullOrEmpty(res.CrewName) || string.IsNullOrEmpty(res.CrewPhone))
                return MSG_ERROR_INCORRECT_RESPONSE;
            crew.Name = res.CrewName;
            crew.Phone = res.CrewPhone;
            crew.Online = res.CrewState > 0;
            if (res.CrewAcriveOrder > 0)
            {
                crew.ActiveOrder = await GetOrderDetails(res.CrewAcriveOrder);
            }

            AppData.Crew = crew;
            AppData.Save();
            return null;
        }

        public static async Task<Order> GetOrderDetails(int orderId)
        {
            var res = await ExecuteCommand(ApiCommand.OrderDetails, new string[] { orderId.ToString() });
            if (!res.BoolResult) return null;
            try
            {
                if (res.Order?[0] == null) return null;
                return new Order
                {
                    OrderId = res.Order[0].orderID,
                    AddressFrom = res.Order[0].orderAddressFrom,
                    AddressFromLat = res.Order[0].orderFromLat,
                    AddressFromLng = res.Order[0].orderFromLng,
                    AddressTo = res.Order[0].orderAddressTo,
                    AddressToLat = res.Order[0].orderToLat,
                    AddressToLng = res.Order[0].orderToLng,
                    AutoData = res.Order[0].orderAutoData,
                    Comment = res.Order[0].orderComment,
                    SickName = res.Order[0].orderSickName,
                    SickPhone = res.Order[0].orderSickPhone,
                    Status = (OrderStatus)res.Order[0].orderStatus,
                    ArrivalDate = DateTime.ParseExact(res.Order[0].orderArrivalDate, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None)
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        public static async Task<string> GetNewOrders()
        {
            var res = await ExecuteCommand(ApiCommand.NewOrders, new string[] { });
            if (!res.BoolResult) return res.ErrorMessage;

            var orders = new List<Order>();
            try
            {
                if ((res.Orders?.Count ?? 0) > 0 && (res.Orders[0]?.Count ?? 0) > 0)
                {
                    foreach (var apiOrder in res.Orders[0])
                    {
                        var order = new Order
                        {
                            OrderId = apiOrder.orderID,
                            AddressFrom = apiOrder.orderAddressFrom,
                            AddressFromLat = apiOrder.orderFromLat,
                            AddressFromLng = apiOrder.orderFromLng,
                            AddressTo = apiOrder.orderAddressTo,
                            AddressToLat = apiOrder.orderToLat,
                            AddressToLng = apiOrder.orderToLng,
                            AutoData = apiOrder.orderAutoData,
                            Comment = apiOrder.orderComment,
                            SickName = apiOrder.orderSickName,
                            SickPhone = apiOrder.orderSickPhone,
                            Status = OrderStatus.New,
                            ArrivalDate = DateTime.ParseExact(apiOrder.orderArrivalDate, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None)
                        };
                        orders.Add(order);
                    }
                }
                AppData.Orders = orders;
                return null;
            }
            catch
            {
                return MSG_ERROR_INCORRECT_RESPONSE;
            }
        }

        public static async Task<string> GetActiveOrder()
        {
            var res = await ExecuteCommand(ApiCommand.ActiveOrder, new string[] { });
            if (!res.BoolResult) return res.ErrorMessage;
            if (res.OrderNumber > 0)
                AppData.Crew.ActiveOrder = await GetOrderDetails(res.OrderNumber);
            else
                AppData.Crew.ActiveOrder = null;
            return null;
        }

        public static async Task<bool> UpdateLocation(double lat, double lng)
        {
            var res = await ExecuteCommand(ApiCommand.UpdateLocation, new string[] { lat.ToString().Replace(",", "."), lng.ToString().Replace(",", ".") });
            return res.BoolResult;
        }

        public static async Task<string> SetCrewState(bool state)
        {
            var res = await ExecuteCommand(ApiCommand.SetCrewState, new string[] { (state ? 1 :0).ToString() });
            return res.BoolResult ? null : res.ErrorMessage;
        }

        public static async Task<string> UpdateOrder(Order order, int newStatus)
        {
            var res = await ExecuteCommand(ApiCommand.UpdateOrderStatus, new string[] { order.OrderId.ToString(), newStatus.ToString() });
            if (res.BoolResult) order.Status = (OrderStatus)newStatus;
            return res.BoolResult ? null : res.ErrorMessage;
        }

        public static async Task<string> CancelOrder(Order order, string reason)
        {
            var res = await ExecuteCommand(ApiCommand.CancelOrder, new string[] { order.OrderId.ToString(), reason });
            if (res.BoolResult) order.Status = OrderStatus.Cancelled;
            return res.BoolResult ? null : res.ErrorMessage;
        }

        public static async Task<string> GetCrewTypes()
        {
            var res = await ExecuteCommand(ApiCommand.GetCrewTypes, new string[] { });
            if (!res.BoolResult) return res.ErrorMessage;

            try
            {
                AppData.CrewTypes = new List<CrewType>();
                if ((res.CrewTypes?.Count ?? 0) > 0 && (res.CrewTypes[0]?.Count ?? 0) > 0)
                {
                    foreach (var apiCrewType in res.CrewTypes[0])
                    {
                        var crewType = new CrewType
                        {
                            Id = apiCrewType.crewTypeID,
                            Name = apiCrewType.crewTypeName
                        };
                        AppData.CrewTypes.Add(crewType);
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return MSG_ERROR_INCORRECT_RESPONSE;
            }
        }

        public static async Task<string> SetCrewType(CrewType crewType)
        {
            var res = await ExecuteCommand(ApiCommand.SetCrewType, new string[] {crewType.Id.ToString() });
            if (!res.BoolResult) return res.ErrorMessage;
            return null;
        }
    }
}

