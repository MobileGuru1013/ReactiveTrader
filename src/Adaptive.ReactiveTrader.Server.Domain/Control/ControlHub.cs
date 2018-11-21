using System.Collections.Generic;
using System.Linq;
using Adaptive.ReactiveTrader.Server.Pricing;
using Adaptive.ReactiveTrader.Server.ReferenceData;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO;
using Adaptive.ReactiveTrader.Shared.DTO.Control;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Adaptive.ReactiveTrader.Server.Control
{
    [HubName(ServiceConstants.Server.ControlHub)]
    public class ControlHub : Hub
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ControlHub));
        private readonly ICurrencyPairRepository _currencyPairRepository;
        private readonly ICurrencyPairUpdatePublisher _currencyPairUpdatePublisher;
        private readonly IPriceFeed _priceFeed;

        public ControlHub(ICurrencyPairRepository currencyPairRepository,
            ICurrencyPairUpdatePublisher currencyPairUpdatePublisher,
            IPriceFeed priceFeed)
        {
            _currencyPairRepository = currencyPairRepository;
            _currencyPairUpdatePublisher = currencyPairUpdatePublisher;
            _priceFeed = priceFeed;
        }

        [ControlAuth]
        [HubMethodName(ServiceConstants.Server.SetPriceFeedThroughput)]
        public UnitDto SetPriceFeedThroughput(FeedThroughputDto throughputDto)
        {
            Log.InfoFormat("Received set price feed throughput command to {0} from {1}.", throughputDto.Throughput, ContextUtil.GetUserName(Context));
            _priceFeed.SetUpdateFrequency(throughputDto.Throughput);
            return new UnitDto();
        }
        
        [ControlAuth]
        [HubMethodName(ServiceConstants.Server.GetPriceFeedThroughput)]
        public FeedThroughputDto SetPriceFeedThroughput()
        {
            var throughput = _priceFeed.GetUpdateFrequency();
            Log.InfoFormat("Returning price feed throughput of {0} to {1}.", throughput, ContextUtil.GetUserName(Context));
            return new FeedThroughputDto
            {
                Throughput = throughput
            };
        }

        [ControlAuth]
        [HubMethodName(ServiceConstants.Server.GetCurrencyPairStates)]
        public IEnumerable<CurrencyPairStateDto> GetCurrencyPairStates()
        {
            Log.InfoFormat("Returning all currency pair states to {0}.", ContextUtil.GetUserName(Context));

            return _currencyPairRepository.GetAllCurrencyPairInfos()
                .Select(cpi => new CurrencyPairStateDto
                {
                    Symbol = cpi.CurrencyPair.Symbol,
                    Enabled = cpi.Enabled,
                    Stale = cpi.Stale
                })
                .ToList();
        }

        [ControlAuth]
        [HubMethodName(ServiceConstants.Server.SetCurrencyPairState)]
        public UnitDto SetCurrencyPairState(CurrencyPairStateDto request)
        {
            var currencyPair = _currencyPairRepository.GetAllCurrencyPairInfos()
                .FirstOrDefault(cpi => cpi.CurrencyPair.Symbol == request.Symbol);

            if (currencyPair != null)
            {
                Log.InfoFormat("Received set currency pair state [{0}] from {1}.", request.Symbol, ContextUtil.GetUserName(Context));
                if (currencyPair.Enabled != request.Enabled)
                {
                    if (request.Enabled)
                    {
                        Log.InfoFormat("Enabling currency pair {0}.", request.Symbol);
                        _currencyPairUpdatePublisher.AddCurrencyPair(currencyPair.CurrencyPair);
                    }
                    else
                    {
                        Log.InfoFormat("Disabling currency pair {0}", request.Symbol);
                        _currencyPairUpdatePublisher.RemoveCurrencyPair(currencyPair.CurrencyPair);
                    }
                    currencyPair.Enabled = request.Enabled;
                }

                if (currencyPair.Stale != request.Stale)
                {
                    if (currencyPair.Stale)
                    {
                        Log.InfoFormat("Making currency pair {0} go stale.", request.Symbol);
                    }
                    else
                    {
                        Log.InfoFormat("Making currency pair {0} no longer stale.", request.Symbol);
                    }
                    currencyPair.Stale = request.Stale;
                }
            }
            else
            {
                Log.WarnFormat("Received set currency pair state for unknown currency pair {0} from {1}.", request.Symbol, ContextUtil.GetUserName(Context));
            }

            return new UnitDto();
        }
    }
}