using Microsoft.AspNetCore.Mvc;
using FormatSoapAPI;
using Azymut.Helpers;
using DataAccessLibrary.Models.Shoper;
using DataAccessLibrary.Data.Shoper;
using DataAccessLibrary.Data.Azymut;
using DataAccessLibrary.Models.Azymut;
using Azymut.Models;
using Azymut.Models.Shoper;
using DataAccessLibrary.Models.Hangfire;
using Azymut.Models.Azymut;
using DataAccessLibrary.Data.Hangfire;
using System.Net.Mail;
using RequestHelpersLibrary;


namespace AzymutShoper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AzymutShoperOrdersController : ControllerBase
    {
        private readonly IShoperDataService _shoperDataService;
        private readonly IShoperService _shoperService;
        private readonly IHttpClientFactory _httpClient;
        private readonly IAzymutDataService _azymutDataService;
        private readonly IAzymutService _azymutService;
        private readonly IHangfireDataService _hangfireDataService;
        private readonly IRequestValidator _validator;
        private readonly IRequestAliveConnectionKeeper _aliveConnectionKeeper;
        private readonly IConfiguration _configuration;
        public iFormatSoapAPIClient clientAzymut { get; set; }

        public AzymutShoperOrdersController(IConfiguration configuration, IHangfireDataService hangfireDataService, IAzymutService azymutService, IHttpClientFactory httpClient, IAzymutDataService azymutDataService, IShoperService shoperService, IShoperDataService shoperDataService, IRequestValidator validator, IRequestAliveConnectionKeeper aliveConnectionKeeper)
        {
            _shoperDataService = shoperDataService;
            _shoperService = shoperService;
            _httpClient = httpClient;
            _azymutDataService = azymutDataService;
            _azymutService = azymutService;
            _hangfireDataService = hangfireDataService;
            _validator = validator;
            _aliveConnectionKeeper = aliveConnectionKeeper;
            _configuration = configuration;
            clientAzymut = new iFormatSoapAPIClient();
        }
        
        // Getting orders from Shoper, sending orders to Azymut, getting links from Azymut, sending links to Shoper.

        [HttpGet("Run")]
        public async Task<IActionResult> Run(string synchronizationGuid)
        {
            // Validate call
            if (_validator.ValidateRequest(Request.Headers["Authorization"]) == false)
            {
                return Unauthorized();
            }

            // Since Tim can run longer than Azure's supported limit, spawning a connection keeper
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task.Run(() => _aliveConnectionKeeper.SpawnKeepConnectionAliveTask(this.Response, cancellationToken));

            try
            {
                // counters
                int newOrdersFromShoper = 0;
                int ordersSentToAzymut = 0;
                int linksGotFromAzymut = 0;
                int linksSentToUser = 0;

                // Get Azymut api login and pass
                AzymutApiUser apiAzymutUserData = await _azymutDataService.GetAzymutApiUserAsync(synchronizationGuid);

                // Get Azymut SynchronizationId
                AzymutSynchronizationProperties azymutSynchronizationProperties = await _azymutDataService.GetAzymutSynchronizationProperties(synchronizationGuid);
                int synchronizationId = azymutSynchronizationProperties.Id;

                // Get Shoper token
                ApiUserShoper apiShoperUserData = await _shoperDataService.GetShoperApiUserAsync(synchronizationGuid);
                string token = await _shoperService.GetTokenForApiAsync(apiShoperUserData.BaseUrl, apiShoperUserData.ApiUser, apiShoperUserData.ApiPassword);

                // Get Shoper parameters - order statuses
                Synchronization synchronizationProperties = await _hangfireDataService.GetSynchronizationPropertiesAsync(synchronizationGuid);
                int shoperSynchronizationId = synchronizationProperties.Id;
                Azymut.Models.Shoper.ShoperParameters shoperParameters = await _shoperService.GetShoperParametersAsync(shoperSynchronizationId);

                // get books' typeid and codes and issueid
                List<BookCodeIssueType> booksCodesIssuesTypes = await _azymutService.GetBooksCodesIssuesAsync();

                // get all paid orders from Shoper 
                List<OrderSQL> ordersToSQLNotFiltered = await _shoperService.CreateListOfPaidOrdersAsync(apiShoperUserData.BaseUrl, token, synchronizationId, booksCodesIssuesTypes);

                // check orders in SQL Database and save only new ones (order paid, change status of order in Shoper too)
                foreach (var order in ordersToSQLNotFiltered)
                {
                    int orderExists = await _azymutService.CheckOrderExistenceAsync(order.OrderId, order.Code, order.SynchronizationId);
                    if (orderExists == 0)
                    {
                        await _azymutService.SaveNewOrderAsync(order);
                        await _shoperService.UpdateOrderStatusAsync(apiShoperUserData.BaseUrl, token, order.OrderId, shoperParameters.OrderStatus1);
                        newOrdersFromShoper += 1;
                    }
                }

                // get session Id string
                string sessionID = await _azymutService.GetAzymutSessionStringAsync(clientAzymut, apiAzymutUserData.ApiUser, apiAzymutUserData.ApiPassword);

                // get list of orders to send - with status = 1 in Orders table
                List<OrderSQL> newOrders = await _azymutService.GetOrdersFromSQLAsync(synchronizationId, 1);

                foreach (var newOrderItem in newOrders)
                {
                    // create item for Azymut
                    iFSCreateOrderReq orderToAzymut = _azymutService.ConvertSQLOrderToAzymut(newOrderItem);

                    createOrder newOrder = new createOrder();
                    newOrder.sessionID = sessionID;
                    newOrder.items = new iFSCreateOrderReq[] { orderToAzymut };
                    newOrder.orderID = $"ShoperOrder-Id-{newOrderItem.OrderId}-{newOrderItem.SynchronizationId}-{newOrderItem.Code}";

                    // sent item to Azymut
                    var newOrderResponse = await clientAzymut.createOrderAsync(newOrder);

                    // check response and set status of order in SQL to 2, when orderResponse is success
                    if (newOrderResponse.createOrderResponse.createOrderResult.msg == "OK")
                    {
                        await _azymutService.ChangeOrderStatusAsync(newOrderItem, 2);
                        await _shoperService.UpdateOrderStatusAsync(apiShoperUserData.BaseUrl, token, newOrderItem.OrderId, shoperParameters.OrderStatus2);
                        ordersSentToAzymut += 1;
                    }
                }

                // waiting for Azymut link creation for last sent orders - from configuration file in seconds
                // waiting only if there are any new orders
                if(newOrders.Count > 0)
                {
                    await Task.Delay(_configuration.GetValue<int>("AzymutParameters:OrderTimeout") * 1000);
                }

                // get orders with status = 2 (order sent to Azymut, waiting for links)
                List<OrderSQL> sentOrders = await _azymutService.GetOrdersFromSQLAsync(synchronizationId, 2);

                // read mail template file - first line mail sender name, second line - subject, third line - body
                string mailFrom = "";
                string subject = "";
                string bodyTemplate = "";
                using (StreamReader reader = new StreamReader($"Files/MailTemplate-{synchronizationGuid}.txt"))
                {
                    mailFrom = reader.ReadLine();
                    subject = reader.ReadLine();
                    bodyTemplate = reader.ReadLine();
                }

                // get multiformat links for ebooks, if available, and set order status to 3, when mail with link has been successfully sent
                foreach (var sentOrder in sentOrders)
                {
                    getOrderMultiformatInfo orderInfo = new getOrderMultiformatInfo();

                    orderInfo.sessionID = sessionID;
                    orderInfo.orderID = $"ShoperOrder-Id-{sentOrder.OrderId}-{sentOrder.SynchronizationId}-{sentOrder.Code}";

                    var orderInfoResponse = await clientAzymut.getOrderMultiformatInfoAsync(orderInfo);
                    if (orderInfoResponse != null)
                    {
                        if (orderInfoResponse.getOrderMultiformatInfoResponse.getOrderMultiformatInfoResult.msg == "OK" &&
                             orderInfoResponse.getOrderMultiformatInfoResponse.getOrderMultiformatInfoResult.result.items != null)
                        {
                            List<string> urlLinks = new List<string>();
                            foreach (var item in orderInfoResponse.getOrderMultiformatInfoResponse.getOrderMultiformatInfoResult.result.items[0].content)
                            {
                                urlLinks.Add(item.urls[0]);
                            }

                            // increase counter of links read from Azymut
                            linksGotFromAzymut += 1;

                            // define email parameters
                            var fromAddress = new MailAddress(apiShoperUserData.MailSenderAddress, mailFrom);
                            var toAddress = new MailAddress(sentOrder.Mail, sentOrder.Mail);
                            string fromPassword = apiShoperUserData.MailSenderPassword;

                            // create links (for multiformat with HTML formatting for new lines) 
                            string urlLink = string.Empty;
                            foreach (string oneLink in urlLinks)
                            {
                                urlLink += oneLink + "<br>";
                            }

                            // change {urlLink} and {orderId} from mail template with parameters
                            string body = bodyTemplate.Replace("{urlLink}", urlLink);
                            body = body.Replace("{orderId}", sentOrder.OrderId.ToString());

                            // create smtp client and send mail to customer
                            var smtp = _shoperService.CreateSmtpClient(apiShoperUserData.MailSMTP, apiShoperUserData.MailPort, fromAddress.Address, fromPassword);

                            var message = new MailMessage(fromAddress, toAddress)
                            {
                                Subject = subject,
                                Body = body,
                                IsBodyHtml = true
                            };

                            try
                            {
                                // send email to user
                                smtp.Send(message);

                                // increase counter of links send to users
                                linksSentToUser += 1;
                                await _azymutService.ChangeOrderStatusAsync(sentOrder, 3);
                                await _shoperService.UpdateOrderStatusAsync(apiShoperUserData.BaseUrl, token, sentOrder.OrderId, shoperParameters.OrderStatus3);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error during sending mail to user: {sentOrder.Mail}, error {ex.Message}", ex);
                            }

                        }
                    }
                }

                _aliveConnectionKeeper.CancelKeepAliveToken(cancellationToken);

                // For long running processes we need to write to the response
                await this.Response.WriteAsync($"{newOrdersFromShoper} new paid orders has been found, {ordersSentToAzymut} orders sent to Azymut, {linksGotFromAzymut} links to ebooks got from Azymut, {linksSentToUser} links sent to users.");
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _aliveConnectionKeeper.CancelKeepAliveToken(cancellationToken);
                return BadRequest(ex.Message);
            }
        }
    }
}
 