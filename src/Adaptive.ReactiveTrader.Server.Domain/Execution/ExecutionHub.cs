﻿using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Adaptive.ReactiveTrader.Server.Execution
{
    [HubName(ServiceConstants.Server.ExecutionHub)]
    public class ExecutionHub : Hub
    {
        private readonly IExecutionService _executionService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExecutionHub));

        public ExecutionHub(IExecutionService executionService)
        {
            _executionService = executionService;
        }

        [HubMethodName(ServiceConstants.Server.Execute)]
        public Task<TradeDto> Execute(TradeRequestDto tradeRequest)
        {
            var user = ContextUtil.GetUserName(Context);
            Log.InfoFormat("Received trade request {0} from user {1}", tradeRequest, user);

            var trade = _executionService.Execute(tradeRequest, user);
            Log.InfoFormat("Trade executed: {0}", trade);

            return trade;
        }
    }
}