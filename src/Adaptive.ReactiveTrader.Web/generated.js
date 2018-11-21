var _this = this;
$(document).ready(function () {
    // 15 minutes session, we disconnect users so they don't eat up too many websocket connections on Azure for too long
    var sessionExpirationSeconds = 15 * 60;

    // generate random username
    var username = "Web-" + Math.floor((Math.random() * 1000) + 1);

    var reactiveTrader = new ReactiveTrader();

    // If you don't specify a server, by default SignalR connects to origin.
    var servers = location.search.indexOf("?server=local") == -1 ? [""] : ["http://localhost:8080"];
    reactiveTrader.initialize(username, servers);

    var pricingViewModelFactory = new PricingViewModelFactory(reactiveTrader.priceLatencyRecorder);
    var spotTilesViewModel = new SpotTilesViewModel(reactiveTrader.referenceDataRepository, pricingViewModelFactory);
    var blotterViewModel = new BlotterViewModel(reactiveTrader.tradeRepository);
    var connectivityStatusViewModel = new ConnectivityStatusViewModel(reactiveTrader, reactiveTrader.priceLatencyRecorder);
    var sessionExpirationService = new SessionExpirationService(sessionExpirationSeconds);
    var shellViewModel = new ShellViewModel(spotTilesViewModel, blotterViewModel, connectivityStatusViewModel, sessionExpirationService, reactiveTrader);

    // TODO this should be moved somewhere else
    _this.fadeTrade = function (element, index, data) {
        $(element).animate({ backgroundColor: '#7A9EFF' }, 200).animate({ backgroundColor: 'white' }, 800);
    };

    ko.applyBindings(shellViewModel);
});
var CurrencyPairDto = (function () {
    function CurrencyPairDto() {
    }
    return CurrencyPairDto;
})();
var CurrencyPairUpdateDto = (function () {
    function CurrencyPairUpdateDto() {
    }
    return CurrencyPairUpdateDto;
})();
var DirectionDto;
(function (DirectionDto) {
    DirectionDto[DirectionDto["Buy"] = 0] = "Buy";
    DirectionDto[DirectionDto["Sell"] = 1] = "Sell";
})(DirectionDto || (DirectionDto = {}));
var PriceDto = (function () {
    function PriceDto() {
    }
    return PriceDto;
})();
var PriceSubscriptionRequestDto = (function () {
    function PriceSubscriptionRequestDto() {
    }
    return PriceSubscriptionRequestDto;
})();
var TradeDto = (function () {
    function TradeDto() {
    }
    return TradeDto;
})();
var TradeRequestDto = (function () {
    function TradeRequestDto() {
    }
    return TradeRequestDto;
})();
var TradeStatusDto;
(function (TradeStatusDto) {
    TradeStatusDto[TradeStatusDto["Done"] = 0] = "Done";
    TradeStatusDto[TradeStatusDto["Rejected"] = 1] = "Rejected";
})(TradeStatusDto || (TradeStatusDto = {}));
var UpdateTypeDto;
(function (UpdateTypeDto) {
    UpdateTypeDto[UpdateTypeDto["Added"] = 0] = "Added";
    UpdateTypeDto[UpdateTypeDto["Removed"] = 1] = "Removed";
})(UpdateTypeDto || (UpdateTypeDto = {}));
var DateUtils = (function () {
    function DateUtils() {
    }
    DateUtils.formatDateDayMonth = function (date) {
        // example date: Fri Apr 18 2014 01:00:00 GMT+0100 (GMT Summer Time)
        var tokens = date.toString().split(" ");
        return tokens[2] + " " + tokens[1];
    };

    DateUtils.formatDateDayMonthYear = function (date) {
        var tokens = date.toString().split(" ");
        return tokens[2] + " " + tokens[1] + " " + tokens[3].substr(2, 2);
    };

    DateUtils.formatDateDayMonthYearHour = function (date) {
        var tokens = date.toString().split(" ");
        return tokens[2] + " " + tokens[1] + " " + tokens[3].substr(2, 2) + " " + tokens[4].substr(0, 5);
    };
    return DateUtils;
})();
var NumberFormatter = (function () {
    function NumberFormatter() {
    }
    NumberFormatter.format = function (value) {
        return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    };
    return NumberFormatter;
})();
Rx.ConnectableObservable.prototype.lazyConnect = function (futureDisposable) {
    var _this = this;
    var connected = false;
    return Rx.Observable.create(function (observer) {
        var subscription = _this.subscribe(observer);
        if (!connected) {
            connected = true;
            if (!futureDisposable.isDisposed) {
                futureDisposable.setDisposable(_this.connect());
            }
        }
        return subscription;
    }).asObservable();
};

Rx.Observable.prototype.cacheFirstResult = function () {
    return this.take(1).publishLast().lazyConnect(new Rx.SingleAssignmentDisposable());
};

// TODO take unit inclusive
// TODO ObserveLatestOn
Rx.Observable.prototype.conflate = function (minimumUpdatePeriodMs, scheduler) {
    var _this = this;
    return Rx.Observable.create(function (observer) {
        // indicate when the last update was published
        var lastUpdateTime = 0;

        // indicate if an update is currently scheduled
        var updateScheduled = new Rx.SerialDisposable();
        updateScheduled.setDisposable(null);

        // indicate if completion has been requested (we can't complete immediatly if an update is in flight)
        var completionRequested = false;

        var subscription = _this.observeOn(scheduler).subscribe(function (x) {
            var currentUpdateTime = scheduler.now();
            var scheduleRequired = currentUpdateTime - lastUpdateTime < minimumUpdatePeriodMs;

            if (scheduleRequired) {
                updateScheduled.setDisposable(scheduler.scheduleWithRelative(lastUpdateTime + minimumUpdatePeriodMs, function () {
                    observer.onNext(x);

                    lastUpdateTime = scheduler.now();
                    updateScheduled.setDisposable(null);

                    if (completionRequested) {
                        observer.onCompleted();
                    }
                }));
            } else {
                observer.onNext(x);
                lastUpdateTime = scheduler.now();
            }
        }, function (ex) {
            return observer.onError(ex);
        }, function () {
            // if we have scheduled an update we need to complete once the update has been published
            if (updateScheduled.getDisposable() != null) {
                completionRequested = true;
            } else {
                observer.onCompleted();
            }
        });

        return new Rx.CompositeDisposable(subscription, updateScheduled);
    });
};

Rx.Observable.prototype.detectStale = function (stalenessPeriodMs, scheduler) {
    var _this = this;
    return Rx.Observable.create(function (observer) {
        var timerSubscription = new Rx.SerialDisposable();

        var scheduleStale = function () {
            timerSubscription.setDisposable(Rx.Observable.timer(stalenessPeriodMs, scheduler).subscribe(function (_) {
                observer.onNext(new Stale(true, null));
            }));
        };

        var sourceSubscription = _this.subscribe(function (x) {
            // cancel any scheduled stale update
            timerSubscription.getDisposable().dispose();

            observer.onNext(new Stale(false, x));

            scheduleStale();
        }, function (ex) {
            return observer.onError(ex);
        }, function () {
            return observer.onCompleted();
        });

        scheduleStale();

        return new Rx.CompositeDisposable(sourceSubscription, timerSubscription);
    });
};
var Stale = (function () {
    function Stale(stale, update) {
        this.isStale = stale;
        this.update = update;
    }
    return Stale;
})();
var Statistics = (function () {
    function Statistics(uiLatency, receivedCount, renderedCount) {
        this.receivedCount = receivedCount;
        this.uiLatencyMax = uiLatency;
        this.renderedCount = renderedCount;
    }
    return Statistics;
})();
var PriceLatencyRecorder = (function () {
    function PriceLatencyRecorder() {
    }
    PriceLatencyRecorder.prototype.onRendered = function (price) {
        var priceLatency = price;
        if (priceLatency != null) {
            priceLatency.displayedOnUi();

            this._renderedCount++;
            if (this._maxLatency == null || priceLatency.uiProcessingTimeMs > this._maxLatency.uiProcessingTimeMs) {
                this._maxLatency = priceLatency;
            }
        }
    };

    PriceLatencyRecorder.prototype.onReceived = function (price) {
        var priceLatency = price;
        if (priceLatency != null) {
            priceLatency.receivedInGuiProcess();
            this._receivedCount++;
        }
    };

    PriceLatencyRecorder.prototype.calculateAndReset = function () {
        if (!this._maxLatency)
            return null;

        var result = new Statistics(this._maxLatency.uiProcessingTimeMs, this._receivedCount, this._renderedCount);
        this._renderedCount = 0;
        this._receivedCount = 0;
        this._maxLatency = null;
        return result;
    };
    return PriceLatencyRecorder;
})();
var Profiler = (function () {
    function Profiler() {
        this._userPerformanceApi = typeof window.performance != "undefined";
    }
    Profiler.prototype.now = function () {
        if (this._userPerformanceApi) {
            return window.performance.now();
        } else {
            return new Date().getTime();
        }
    };
    return Profiler;
})();
var Direction;
(function (Direction) {
    Direction[Direction["Buy"] = 0] = "Buy";
    Direction[Direction["Sell"] = 1] = "Sell";
})(Direction || (Direction = {}));
var Trade = (function () {
    function Trade(currencyPair, direction, notional, spotRate, tradeStatus, tradeDate, tradeId, traderName, valueDate, dealtCurrency) {
        this.currencyPair = currencyPair;
        this.direction = direction;
        this.notional = notional;
        this.spotRate = spotRate;
        this.tradeStatus = tradeStatus;
        this.tradeDate = new Date(tradeDate);
        this.tradeId = tradeId;
        this.traderName = traderName;
        this.valueDate = new Date(valueDate);
        this.dealtCurrency = dealtCurrency;
    }
    return Trade;
})();
var TradeFactory = (function () {
    function TradeFactory() {
    }
    TradeFactory.prototype.create = function (trade) {
        return new Trade(trade.CurrencyPair, trade.Direction == 0 /* Buy */ ? 0 /* Buy */ : 1 /* Sell */, trade.Notional, trade.SpotRate, trade.Status == 0 /* Done */ ? 0 /* Done */ : 1 /* Rejected */, trade.TradeDate, trade.TradeId, trade.TraderName, trade.ValueDate, trade.DealtCurrency);
    };
    return TradeFactory;
})();
var TradeStatus;
(function (TradeStatus) {
    TradeStatus[TradeStatus["Done"] = 0] = "Done";
    TradeStatus[TradeStatus["Rejected"] = 1] = "Rejected";
})(TradeStatus || (TradeStatus = {}));
var ExecutablePrice = (function () {
    function ExecutablePrice(direction, rate, executionRepository) {
        this._executionRepository = executionRepository;
        this.direction = direction;
        this.rate = rate;
    }
    ExecutablePrice.prototype.execute = function (notional, dealtCurrency) {
        return this._executionRepository.executeRequest(this, notional, dealtCurrency);
    };
    return ExecutablePrice;
})();
var Price = (function () {
    function Price(bid, ask, valueDate, currencyPair, profiler) {
        this.bid = bid;
        this.ask = ask;
        this.valueDate = valueDate;
        this.currencyPair = currencyPair;
        this.isStale = false;
        this._profiler = profiler;

        bid.parent = this;
        ask.parent = this;

        this.spread = (ask.rate - bid.rate) * Math.pow(10, currencyPair.pipsPosition);
    }
    Object.defineProperty(Price.prototype, "mid", {
        get: function () {
            return (this.bid.rate + this.ask.rate) / 2;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Price.prototype, "uiProcessingTimeMs", {
        get: function () {
            return this._renderTimestamp - this._receivedTimestamp;
        },
        enumerable: true,
        configurable: true
    });

    Price.prototype.displayedOnUi = function () {
        this._renderTimestamp = this._profiler.now();
    };

    Price.prototype.receivedInGuiProcess = function () {
        this._receivedTimestamp = this._profiler.now();
    };
    return Price;
})();
var PriceFactory = (function () {
    function PriceFactory(executionRepository, priceLatencyRecored, profiler) {
        this._executionRepository = executionRepository;
        this._priceLatencyRecored = priceLatencyRecored;
        this._profiler = profiler;
    }
    PriceFactory.prototype.create = function (priceDto, currencyPair) {
        var bid = new ExecutablePrice(1 /* Sell */, priceDto.b, this._executionRepository);
        var ask = new ExecutablePrice(0 /* Buy */, priceDto.a, this._executionRepository);
        var valueDate = new Date(priceDto.d);
        var price = new Price(bid, ask, valueDate, currencyPair, this._profiler);

        this._priceLatencyRecored.onReceived(price);

        return price;
    };
    return PriceFactory;
})();
var StalePrice = (function () {
    function StalePrice(currencyPair) {
        this.currencyPair = currencyPair;
        this.isStale = true;
    }
    return StalePrice;
})();
var CurrencyPair = (function () {
    function CurrencyPair(symbol, ratePrecision, pipsPosition, priceRespository) {
        var _this = this;
        this.symbol = symbol;
        this.ratePrecision = ratePrecision;
        this.pipsPosition = pipsPosition;
        this.baseCurrency = symbol.substring(0, 3);
        this.counterCurrency = symbol.substring(3, 6);

        this.prices = Rx.Observable.defer(function () {
            return priceRespository.getPriceStream(_this);
        }).publish().refCount();
    }
    return CurrencyPair;
})();
var CurrencyPairUpdate = (function () {
    function CurrencyPairUpdate(updateType, currencyPair) {
        this._currencyPair = currencyPair;
        this._updateType = updateType;
    }
    Object.defineProperty(CurrencyPairUpdate.prototype, "updateType", {
        get: function () {
            return this._updateType;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(CurrencyPairUpdate.prototype, "currencyPair", {
        get: function () {
            return this._currencyPair;
        },
        enumerable: true,
        configurable: true
    });
    return CurrencyPairUpdate;
})();
var CurrencyPairUpdateFactory = (function () {
    function CurrencyPairUpdateFactory(priceRepository) {
        this._priceRepository = priceRepository;
    }
    CurrencyPairUpdateFactory.prototype.create = function (currencyPairUpdate) {
        var cp = new CurrencyPair(currencyPairUpdate.CurrencyPair.Symbol, currencyPairUpdate.CurrencyPair.RatePrecision, currencyPairUpdate.CurrencyPair.PipsPosition, this._priceRepository);

        var update = new CurrencyPairUpdate(currencyPairUpdate.UpdateType == 0 /* Added */ ? 0 /* Add */ : 1 /* Remove */, cp);

        return update;
    };
    return CurrencyPairUpdateFactory;
})();
var UpdateType;
(function (UpdateType) {
    UpdateType[UpdateType["Add"] = 0] = "Add";
    UpdateType[UpdateType["Remove"] = 1] = "Remove";
})(UpdateType || (UpdateType = {}));
var ExecutionRepository = (function () {
    function ExecutionRepository(executionServiceClient, tradeFactory) {
        this._tradeFactory = tradeFactory;
        this._executionServiceClient = executionServiceClient;
    }
    ExecutionRepository.prototype.executeRequest = function (executablePrice, notional, dealtCurrency) {
        var _this = this;
        var price = executablePrice.parent;

        var request = new TradeRequestDto();
        request.Direction = executablePrice.direction == 0 /* Buy */ ? 0 /* Buy */ : 1 /* Sell */;
        request.Notional = notional;
        request.SpotRate = executablePrice.rate;
        request.Symbol = price.currencyPair.symbol;
        request.ValueDate = price.valueDate.toISOString();
        request.DealtCurrency = dealtCurrency;

        return this._executionServiceClient.executeRequest(request).select(function (tradeDto) {
            return _this._tradeFactory.create(tradeDto);
        }).detectStale(2000, Rx.Scheduler.timeout);
    };
    return ExecutionRepository;
})();
var PriceRepository = (function () {
    function PriceRepository(pricingServiceClient, priceFactory) {
        this._priceFactory = priceFactory;
        this._pricingServiceClient = pricingServiceClient;
    }
    PriceRepository.prototype.getPriceStream = function (currencyPair) {
        var _this = this;
        return Rx.Observable.defer(function () {
            return _this._pricingServiceClient.getSpotStream(currencyPair.symbol);
        }).select(function (p) {
            return _this._priceFactory.create(p, currencyPair);
        }).catch(function (ex) {
            console.error("Error thrown in stream " + currencyPair.symbol + ": " + ex);

            // if the stream errors (server disconnected), we push a stale price
            return Rx.Observable.return(new StalePrice(currencyPair)).concat(Rx.Observable.timer(3000, Rx.Scheduler.timeout).ignoreElements().select(function (_) {
                return new StalePrice(currencyPair);
            }));
        }).repeat().detectStale(4000, Rx.Scheduler.timeout).select(function (s) {
            return (s.isStale ? new StalePrice(currencyPair) : s.update);
        }).publish().refCount();
    };
    return PriceRepository;
})();
var ReferenceDataRepository = (function () {
    function ReferenceDataRepository(referenceDataServiceClient, currencyPairUpdateFactory) {
        this._currencyPairUpdateFactory = currencyPairUpdateFactory;
        this._referenceDataServiceClient = referenceDataServiceClient;
    }
    ReferenceDataRepository.prototype.getCurrencyPairsStream = function () {
        var _this = this;
        return Rx.Observable.defer(function () {
            return _this._referenceDataServiceClient.getCurrencyPairUpdatesStream();
        }).where(function (updates) {
            return updates.length > 0;
        }).select(function (updates) {
            return _this.createCurrencyPairUpdates(updates);
        }).catch(Rx.Observable.return([])).repeat().publish().refCount();
    };

    ReferenceDataRepository.prototype.createCurrencyPairUpdates = function (updates) {
        var result = [];

        for (var i = 0; i < updates.length; i++) {
            result[i] = this._currencyPairUpdateFactory.create(updates[i]);
        }

        return result;
    };
    return ReferenceDataRepository;
})();
var TradeRepository = (function () {
    function TradeRepository(blotterServiceClient, tradeFactory) {
        this._tradeFactory = tradeFactory;
        this._blotterServiceClient = blotterServiceClient;
    }
    TradeRepository.prototype.getTradesStream = function () {
        var _this = this;
        return Rx.Observable.defer(function () {
            return _this._blotterServiceClient.getTradesStream();
        }).select(function (trades) {
            return _this.createTrades(trades);
        }).catch(function () {
            return Rx.Observable.return([]);
        }).repeat().publish().refCount();
    };

    TradeRepository.prototype.createTrades = function (trades) {
        var result = [];

        for (var i = 0; i < trades.length; i++) {
            result[i] = this._tradeFactory.create(trades[i]);
        }

        return result;
    };
    return TradeRepository;
})();
var ServiceClientBase = (function () {
    function ServiceClientBase(connectionProvider) {
        this._connectionProvider = connectionProvider;
    }
    ServiceClientBase.prototype.getResilientStream = function (streamFactory, connectionTimeoutMs) {
        var activeConnections = this._connectionProvider.getActiveConnection().selectMany(function (connection) {
            return connection.status;
        }, function (connection, status) {
            return { c: connection, s: status };
        }).where(function (t) {
            return t.s.connectionStatus == 1 /* Connected */ || t.s.connectionStatus == 4 /* Reconnected */;
        }).select(function (t) {
            return t.c;
        }).publish().refCount();

        // get the first connection
        var firstConnection = activeConnections.take(1).timeout(connectionTimeoutMs);

        // 1 - notifies when the first connection gets disconnected
        var firstDisconnection = firstConnection.selectMany(function (connection) {
            return connection.status;
        }, function (connection, status) {
            return { c: connection, s: status };
        }).where(function (t) {
            return t.s.connectionStatus == 3 /* Reconnecting */ || t.s.connectionStatus == 5 /* Closed */;
        }).select(function (t) {
        });

        // 2- connection provider created a new connection it means the active one has droped
        var subsequentConnection = activeConnections.skip(1).select(function (_) {
        }).take(1);

        // OnError when we get 1 or 2
        var disconnected = firstDisconnection.merge(subsequentConnection).select(function (_) {
            return Rx.Notification.createOnError(new TransportDisconnectedException("Connection was closed."));
        }).dematerialize();

        // create a stream which will OnError as soon as the connection drops
        return firstConnection.selectMany(function (connection) {
            return streamFactory(connection);
        }).merge(disconnected).publish().refCount();
    };

    ServiceClientBase.prototype.requestUponConnection = function (factory, timeoutMs) {
        return this._connectionProvider.getActiveConnection().take(1).timeout(timeoutMs).selectMany(function (c) {
            return factory(c);
        }).cacheFirstResult();
    };
    return ServiceClientBase;
})();
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var BlotterServiceClient = (function (_super) {
    __extends(BlotterServiceClient, _super);
    function BlotterServiceClient(connectionProvider) {
        _super.call(this, connectionProvider);
    }
    BlotterServiceClient.prototype.getTradesStream = function () {
        var _this = this;
        return _super.prototype.getResilientStream.call(this, function (connection) {
            return _this.getTradesStreamFromConnection(connection);
        }, 5000);
    };

    BlotterServiceClient.prototype.getTradesStreamFromConnection = function (connection) {
        return Rx.Observable.create(function (observer) {
            var tradesSubscription = connection.allTrades.subscribe(observer);

            console.log("Sending blotter subscription...");
            connection.blotterHubProxy.invoke("SubscribeTrades").done(function (_) {
                return console.log("Subscribed to blotter");
            }).fail(function (ex) {
                return observer.onError(ex);
            });

            var unsubscriptionDisposable = Rx.Disposable.create(function () {
                console.log("Sending blotter unsubscription...");

                connection.blotterHubProxy.invoke("UnsubscribeTrades").done(function (_) {
                    return console.log("Unsubscribed from blotter stream.");
                }).fail(function (ex) {
                    return console.error("An error occured while unsubscribing from blotter:" + ex);
                });
            });

            return new Rx.CompositeDisposable([tradesSubscription, unsubscriptionDisposable]);
        }).publish().refCount();
    };
    return BlotterServiceClient;
})(ServiceClientBase);
var ExecutionServiceClient = (function (_super) {
    __extends(ExecutionServiceClient, _super);
    function ExecutionServiceClient(connectionProvider) {
        _super.call(this, connectionProvider);
    }
    ExecutionServiceClient.prototype.executeRequest = function (tradeRequest) {
        var _this = this;
        return this.requestUponConnection(function (connection) {
            return _this.executeForConnection(tradeRequest, connection.executionHubProxy);
        }, 500);
    };

    ExecutionServiceClient.prototype.executeForConnection = function (tradeRequest, executionHub) {
        return Rx.Observable.create(function (observer) {
            executionHub.invoke("Execute", tradeRequest).done(function (trade) {
                return observer.onNext(trade);
            }).fail(function (error) {
                return observer.onError(error);
            });

            return Rx.Disposable.empty;
        });
    };
    return ExecutionServiceClient;
})(ServiceClientBase);
var PricingServiceClient = (function (_super) {
    __extends(PricingServiceClient, _super);
    function PricingServiceClient(connectionProvider) {
        _super.call(this, connectionProvider);
    }
    PricingServiceClient.prototype.getSpotStream = function (currencyPair) {
        var _this = this;
        return _super.prototype.getResilientStream.call(this, function (connection) {
            return _this.getSpotStreamForConnection(currencyPair, connection);
        }, 5000);
    };

    PricingServiceClient.prototype.getSpotStreamForConnection = function (currencyPair, connection) {
        return Rx.Observable.create(function (observer) {
            var pricesSubscription = connection.allPrices.subscribe(function (price) {
                if (price.s == currencyPair) {
                    observer.onNext(price);
                }
            });

            console.log("Sending price subscription for currency pair " + currencyPair);

            var subscriptionRequest = new PriceSubscriptionRequestDto();
            subscriptionRequest.CurrencyPair = currencyPair;

            connection.pricingHubProxy.invoke("SubscribePriceStream", subscriptionRequest).done(function (_) {
                return console.log("Subscribed to " + currencyPair);
            }).fail(function (ex) {
                return observer.onError(ex);
            });

            var unsubsciptionDisposable = Rx.Disposable.create(function () {
                connection.pricingHubProxy.invoke("UnsubscribePriceStream", subscriptionRequest).done(function (_) {
                    return console.log("Unsubscribed from currency pair '" + currencyPair + "' stream.");
                }).fail(function (error) {
                    return console.log("An error occured while sending unsubscription request for " + currencyPair + ":" + error);
                });
            });

            return new Rx.CompositeDisposable([pricesSubscription, unsubsciptionDisposable]);
        }).publish().refCount();
    };
    return PricingServiceClient;
})(ServiceClientBase);
var ReferenceDataServiceClient = (function (_super) {
    __extends(ReferenceDataServiceClient, _super);
    function ReferenceDataServiceClient(connectionProvider) {
        _super.call(this, connectionProvider);
    }
    ReferenceDataServiceClient.prototype.getCurrencyPairUpdatesStream = function () {
        var _this = this;
        return this.getResilientStream(function (connection) {
            return _this.getCurrencyPairUpdatesForConnection(connection);
        }, 5000);
    };

    ReferenceDataServiceClient.prototype.getCurrencyPairUpdatesForConnection = function (connection) {
        return Rx.Observable.create(function (observer) {
            var currencyPairUpdateSubscription = connection.currencyPairUpdates.subscribe(function (currencyPairUpdate) {
                return observer.onNext([currencyPairUpdate]);
            });

            console.log("Sending currency pair subscription...");

            connection.referenceDataHubProxy.invoke("GetCurrencyPairs").done(function (currencyPairs) {
                console.log("Subscribed to currency pairs and received " + currencyPairs.length + " currency pairs.");
                observer.onNext(currencyPairs);
            }).fail(function (ex) {
                return observer.onError(ex);
            });

            var unsubsciptionDisposable = Rx.Disposable.create(function () {
                console.log("Unsubscribed from currency pairs stream.");
            });

            return new Rx.CompositeDisposable(currencyPairUpdateSubscription, unsubsciptionDisposable);
        }).publish().refCount();
    };
    return ReferenceDataServiceClient;
})(ServiceClientBase);
var TransportDisconnectedException = (function () {
    function TransportDisconnectedException(message) {
        this.message = message;
        this.name = "TransportDisconnectedException";
    }
    return TransportDisconnectedException;
})();
var SessionExpirationService = (function () {
    function SessionExpirationService(sessionDurationSeconds) {
        this._sessionDurationSeconds = sessionDurationSeconds;
    }
    SessionExpirationService.prototype.getSessionExpiredStream = function () {
        return Rx.Observable.timer(this._sessionDurationSeconds * 1000, Rx.Scheduler.timeout).publish().refCount();
    };
    return SessionExpirationService;
})();
var Connection = (function () {
    function Connection(address, username) {
        var _this = this;
        this._status = new Rx.BehaviorSubject(new ConnectionInfo(6 /* Uninitialized */, address, 4 /* None */));
        this._address = address;

        if (address != "") {
            this._hubConnection = $.hubConnection(address);
        } else {
            this._hubConnection = $.hubConnection();
            this._address = window.location.protocol + "//" + window.location.host;
        }

        this._hubConnection.qs = { "User": username };

        this._hubConnection.disconnected(function () {
            return _this.changeStatus(5 /* Closed */, 4 /* None */);
        }).connectionSlow(function () {
            return _this.changeStatus(2 /* ConnectionSlow */, _this.getConnectionType());
        }).reconnected(function () {
            return _this.changeStatus(4 /* Reconnected */, _this.getConnectionType());
        }).reconnecting(function () {
            return _this.changeStatus(3 /* Reconnecting */, 4 /* None */);
        }).error(function (error) {
            return console.log(error);
        });

        this._referenceDataHubProxy = this._hubConnection.createHubProxy("ReferenceDataHub");
        this._blotterHubProxy = this._hubConnection.createHubProxy("BlotterHub");
        this._executionHubProxy = this._hubConnection.createHubProxy("ExecutionHub");
        this._pricingHubProxy = this._hubConnection.createHubProxy("PricingHub");

        this.installListeners();
    }
    Connection.prototype.initialize = function () {
        var _this = this;
        return Rx.Observable.create(function (observer) {
            _this.changeStatus(0 /* Connecting */, 4 /* None */);

            console.log("Connecting to " + _this._address + "...");
            _this._hubConnection.start().done(function () {
                _this.changeStatus(1 /* Connected */, _this.getConnectionType());
                observer.onNext(true);
                console.log("Connected to " + _this._address + ".");
            }).fail(function () {
                _this.changeStatus(5 /* Closed */, 4 /* None */);
                var error = "An error occured when starting SignalR connection.";
                console.log(error);
                observer.onError(error);
            });

            return Rx.Disposable.create(function () {
                console.log("Stopping connection...");
                _this._hubConnection.stop();
                console.log("Connection stopped.");
            });
        }).publish().refCount();
    };

    Connection.prototype.getConnectionType = function () {
        switch (this._hubConnection.transport.name) {
            case "webSockets":
                return 0 /* WebScokets */;
            case "foreverFrame":
                return 1 /* ForeverFrame */;
            case "serverSentEvents":
                return 2 /* ServerSentEvents */;
            case "longPolling":
                return 3 /* LongPolling */;
            default:
                return 4 /* None */;
        }
    };

    Connection.prototype.changeStatus = function (newStatus, connectionType) {
        this._status.onNext(new ConnectionInfo(newStatus, this.address, connectionType));
    };

    Object.defineProperty(Connection.prototype, "status", {
        get: function () {
            return this._status;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "address", {
        get: function () {
            return this._address;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "referenceDataHubProxy", {
        get: function () {
            return this._referenceDataHubProxy;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "pricingHubProxy", {
        get: function () {
            return this._pricingHubProxy;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "executionHubProxy", {
        get: function () {
            return this._executionHubProxy;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "blotterHubProxy", {
        get: function () {
            return this._blotterHubProxy;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "allPrices", {
        get: function () {
            return this._allPrices;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "currencyPairUpdates", {
        get: function () {
            return this._currencyPairUpdates;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(Connection.prototype, "allTrades", {
        get: function () {
            return this._allTrades;
        },
        enumerable: true,
        configurable: true
    });

    Connection.prototype.installListeners = function () {
        var _this = this;
        this._allPrices = new Rx.Subject();
        this._currencyPairUpdates = new Rx.Subject();
        this._allTrades = new Rx.Subject();

        this._pricingHubProxy.on("OnNewPrice", function (price) {
            return _this._allPrices.onNext(price);
        });
        this._referenceDataHubProxy.on("OnCurrencyPairUpdate", function (currencyPairs) {
            return _this._currencyPairUpdates.onNext(currencyPairs);
        });
        this._blotterHubProxy.on("OnNewTrade", function (trades) {
            return _this._allTrades.onNext(trades);
        });
    };
    return Connection;
})();
var ConnectionInfo = (function () {
    function ConnectionInfo(connectionStatus, server, connectionType) {
        this.connectionStatus = connectionStatus;
        this.server = server;
        this.connectionType = connectionType;
    }
    ConnectionInfo.prototype.toString = function () {
        return "ConnectionStatus: " + this.connectionStatus + ", Server: " + this.server + ", Type: " + this.connectionType;
    };
    return ConnectionInfo;
})();
var ConnectionProvider = (function () {
    function ConnectionProvider(username, servers) {
        this._disposable = new Rx.SingleAssignmentDisposable();
        this._username = username;
        this._servers = servers;
        this._currentIndex = 0;

        // TODO shuffle server list
        this._connectionSequence = this.createConnectionSequence();
    }
    ConnectionProvider.prototype.getActiveConnection = function () {
        return this._connectionSequence;
    };

    ConnectionProvider.prototype.dispose = function () {
        this._disposable.dispose();
    };

    ConnectionProvider.prototype.createConnectionSequence = function () {
        var _this = this;
        return Rx.Observable.create(function (o) {
            console.info("Creating new connection...");

            var connection = _this.getNextConnection();

            var statusSubscription = connection.status.subscribe(function (_) {
            }, function (ex) {
                console.error(ex);
                o.onCompleted();
            }, function () {
                console.info("Status subscription completed");
                o.onCompleted();
            });

            // TODO if we fail to connect we should not retry straight away to connect to same server, we need some back off
            var connectionSubscription = connection.initialize().subscribe(function (_) {
                return o.onNext(connection);
            }, function (ex) {
                console.error("Active connection errored:" + ex);
                o.onCompleted();
            }, function () {
                console.warn("Active connection completed.");
                o.onCompleted();
            });

            return new Rx.CompositeDisposable(statusSubscription, connectionSubscription);
        }).repeat().replay(null, 1).lazyConnect(this._disposable);
    };

    ConnectionProvider.prototype.getNextConnection = function () {
        var connection = new Connection(this._servers[this._currentIndex], this._username);
        this._currentIndex++;
        if (this._currentIndex == this._servers.length) {
            this._currentIndex = 0;
        }
        return connection;
    };
    return ConnectionProvider;
})();
var ConnectionStatus;
(function (ConnectionStatus) {
    ConnectionStatus[ConnectionStatus["Connecting"] = 0] = "Connecting";
    ConnectionStatus[ConnectionStatus["Connected"] = 1] = "Connected";
    ConnectionStatus[ConnectionStatus["ConnectionSlow"] = 2] = "ConnectionSlow";
    ConnectionStatus[ConnectionStatus["Reconnecting"] = 3] = "Reconnecting";
    ConnectionStatus[ConnectionStatus["Reconnected"] = 4] = "Reconnected";
    ConnectionStatus[ConnectionStatus["Closed"] = 5] = "Closed";
    ConnectionStatus[ConnectionStatus["Uninitialized"] = 6] = "Uninitialized";
})(ConnectionStatus || (ConnectionStatus = {}));
var ConnectionType;
(function (ConnectionType) {
    ConnectionType[ConnectionType["WebScokets"] = 0] = "WebScokets";
    ConnectionType[ConnectionType["ForeverFrame"] = 1] = "ForeverFrame";
    ConnectionType[ConnectionType["ServerSentEvents"] = 2] = "ServerSentEvents";
    ConnectionType[ConnectionType["LongPolling"] = 3] = "LongPolling";
    ConnectionType[ConnectionType["None"] = 4] = "None";
})(ConnectionType || (ConnectionType = {}));
var BlotterViewModel = (function () {
    function BlotterViewModel(tradeRepository) {
        this._tradeRepository = tradeRepository;
        this.trades = ko.observableArray([]);

        this.loadTrades();
    }
    BlotterViewModel.prototype.loadTrades = function () {
        var _this = this;
        this._tradesSubscription = this._tradeRepository.getTradesStream().subscribe(function (trades) {
            return _this.addTrades(trades);
        }, function (ex) {
            return console.error("an error occured within the trade stream", ex);
        });
    };

    BlotterViewModel.prototype.addTrades = function (trades) {
        var _this = this;
        if (trades.length == 0) {
            // empty list of trades means we are disconnected
            this._stale = true;
        } else {
            if (this._stale) {
                this.trades.removeAll();
                this._stale = false;
            }
        }

        trades.forEach(function (t) {
            var tradeViewModel = new TradeViewModel(t);
            _this.trades.unshift(tradeViewModel);
        });
    };

    BlotterViewModel.prototype.disconnect = function () {
        this._tradesSubscription.dispose();
    };
    return BlotterViewModel;
})();
var TradeViewModel = (function () {
    function TradeViewModel(trade) {
        this.spotRate = trade.spotRate;
        this.notional = NumberFormatter.format(trade.notional) + " " + trade.dealtCurrency;
        this.direction = trade.direction == 0 /* Buy */ ? "Buy" : "Sell";
        this.currencyPair = trade.currencyPair.substring(0, 3) + " / " + trade.currencyPair.substring(3, 6);
        this.tradeId = trade.tradeId.toFixed(0);
        this.tradeDate = DateUtils.formatDateDayMonthYearHour(trade.tradeDate);
        this.tradeStatus = trade.tradeStatus == 0 /* Done */ ? "Done" : "REJECTED";
        this.traderName = trade.traderName;
        this.valueDate = "SP. " + DateUtils.formatDateDayMonthYear(trade.tradeDate);
        this.dealtCurrency = trade.dealtCurrency;
    }
    return TradeViewModel;
})();
var ConnectivityStatusViewModel = (function () {
    function ConnectivityStatusViewModel(reactiveTrader, priceLatencyRecorder) {
        var _this = this;
        this._priceLatencyRecorder = priceLatencyRecorder;

        this.uiLatency = ko.observable("-");
        this.uiUpdates = ko.observable(0);
        this.ticksReceived = ko.observable(0);
        this.disconnected = ko.observable(false);
        this.status = ko.observable("Disconnected");

        this._connectionStatusSubscription = reactiveTrader.connectionStatusStream.subscribe(function (status) {
            return _this.onStatusChanged(status);
        }, function (ex) {
            return console.error("An error occured within the connection status stream", ex);
        });

        this._timerSubscription = Rx.Observable.timer(1000, Rx.Scheduler.timeout).repeat().subscribe(function (_) {
            return _this.onTimerTick();
        });
    }
    ConnectivityStatusViewModel.prototype.onTimerTick = function () {
        var stats = this._priceLatencyRecorder.calculateAndReset();
        if (stats == null)
            return;

        this.uiLatency(stats.uiLatencyMax.toFixed(2));
        this.uiUpdates(stats.renderedCount);
        this.ticksReceived(stats.receivedCount);
    };

    ConnectivityStatusViewModel.prototype.onStatusChanged = function (connectionInfo) {
        switch (connectionInfo.connectionStatus) {
            case 6 /* Uninitialized */:
            case 0 /* Connecting */:
                this.status("Connecting to " + connectionInfo.server + "...");
                this.disconnected(true);
                break;
            case 4 /* Reconnected */:
            case 1 /* Connected */:
                this.status("Connected to " + connectionInfo.server + " (" + this.formatConnectionType(connectionInfo.connectionType) + ")");
                this.disconnected(false);
                break;
            case 2 /* ConnectionSlow */:
                this.status("Slow connection detected with " + connectionInfo.server + " (" + this.formatConnectionType(connectionInfo.connectionType) + ")");
                this.disconnected(false);
                break;
            case 3 /* Reconnecting */:
                this.status("Reconnecting to " + connectionInfo.server + "...");
                this.disconnected(true);
                this.clearStatistics();
                break;
            case 5 /* Closed */:
                this.status("Disconnected from " + connectionInfo.server);
                this.disconnected(true);
                this.clearStatistics();
                break;
        }
    };

    ConnectivityStatusViewModel.prototype.clearStatistics = function () {
        this.uiLatency("-");
        this.uiUpdates(0);
        this.ticksReceived(0);
    };

    ConnectivityStatusViewModel.prototype.disconnect = function () {
        this._connectionStatusSubscription.dispose();
        this._timerSubscription.dispose();

        this.status("Session expired, disconnected.");
        this.disconnected(true);
        this.clearStatistics();
    };

    ConnectivityStatusViewModel.prototype.formatConnectionType = function (connectionType) {
        switch (connectionType) {
            case 0 /* WebScokets */:
                return "ws";
            case 2 /* ServerSentEvents */:
                return "sse";
            case 3 /* LongPolling */:
                return "lp";
            case 1 /* ForeverFrame */:
                return "ff";
            case 4 /* None */:
                return "?";
        }
    };
    return ConnectivityStatusViewModel;
})();
var ShellViewModel = (function () {
    function ShellViewModel(spotTiles, blotter, connectivityStatus, sessionExpirationService, reactiveTrader) {
        var _this = this;
        this.spotTiles = spotTiles;
        this.blotter = blotter;
        this.connectivityStatus = connectivityStatus;
        this.sessionExpired = ko.observable(false);
        this._reactiveTrader = reactiveTrader;

        sessionExpirationService.getSessionExpiredStream().subscribe(function () {
            return _this.onSessionExpired();
        });
    }
    ShellViewModel.prototype.onSessionExpired = function () {
        console.info("Expiring session...");

        this.spotTiles.disconnect();
        this.blotter.disconnect();
        this.connectivityStatus.disconnect();
        this.sessionExpired(true);
        this._reactiveTrader.dispose();

        console.info("session expired");
    };

    ShellViewModel.prototype.reconnect = function () {
        location.reload();
    };
    return ShellViewModel;
})();
var AffirmationViewModel = (function () {
    function AffirmationViewModel(trade, parent) {
        this._trade = trade;
        this._parent = parent;
        this._baseCurrency = trade.currencyPair.substring(0, 3);
        this._counterCurrency = trade.currencyPair.substring(3, 6);
        this._otherCurrency = trade.dealtCurrency = this._baseCurrency ? this._counterCurrency : this._baseCurrency;
    }
    Object.defineProperty(AffirmationViewModel.prototype, "currencyPair", {
        get: function () {
            return this._baseCurrency + " / " + this._counterCurrency;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "direction", {
        get: function () {
            return this._trade.direction == 0 /* Buy */ ? "Bought" : "Sold";
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "notional", {
        get: function () {
            return NumberFormatter.format(this._trade.notional);
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "spotRate", {
        get: function () {
            return this._trade.spotRate.toString();
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "tradeDate", {
        get: function () {
            return this._trade.tradeDate.toString();
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "tradeId", {
        get: function () {
            return this._trade.tradeId.toString();
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "traderName", {
        get: function () {
            return this._trade.traderName;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "valueDate", {
        get: function () {
            return DateUtils.formatDateDayMonth(this._trade.valueDate);
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "dealtCurrency", {
        get: function () {
            return this._trade.dealtCurrency;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "rejected", {
        get: function () {
            return this._trade.tradeStatus == 0 /* Done */ ? "" : "REJECTED";
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(AffirmationViewModel.prototype, "otherCurrency", {
        get: function () {
            return this._otherCurrency;
        },
        enumerable: true,
        configurable: true
    });

    AffirmationViewModel.prototype.dismiss = function () {
        this._parent.dismissAffirmation();
    };
    return AffirmationViewModel;
})();
var ConfigViewModel = (function () {
    function ConfigViewModel() {
        this.subscriptionMode = ko.observable(0 /* OnDispatcher */);
        this.executionMode = ko.observable(0 /* Async */);
    }
    ConfigViewModel.prototype.standard = function () {
        this.subscriptionMode(0 /* OnDispatcher */);
    };

    ConfigViewModel.prototype.dropFrame = function () {
        this.subscriptionMode(1 /* ObserveLatestOnDispatcher */);
    };

    ConfigViewModel.prototype.conflate = function () {
        this.subscriptionMode(2 /* Conflate */);
    };

    ConfigViewModel.prototype.constantRate = function () {
        this.subscriptionMode(3 /* ConstantRate */);
    };

    ConfigViewModel.prototype.async = function () {
        this.executionMode(0 /* Async */);
    };

    ConfigViewModel.prototype.sync = function () {
        this.executionMode(1 /* Sync */);
    };
    return ConfigViewModel;
})();
var ExecutionMode;
(function (ExecutionMode) {
    ExecutionMode[ExecutionMode["Async"] = 0] = "Async";
    ExecutionMode[ExecutionMode["Sync"] = 1] = "Sync";
})(ExecutionMode || (ExecutionMode = {}));
var SubscriptionMode;
(function (SubscriptionMode) {
    SubscriptionMode[SubscriptionMode["OnDispatcher"] = 0] = "OnDispatcher";
    SubscriptionMode[SubscriptionMode["ObserveLatestOnDispatcher"] = 1] = "ObserveLatestOnDispatcher";
    SubscriptionMode[SubscriptionMode["Conflate"] = 2] = "Conflate";
    SubscriptionMode[SubscriptionMode["ConstantRate"] = 3] = "ConstantRate";
})(SubscriptionMode || (SubscriptionMode = {}));
var ErrorViewModel = (function () {
    function ErrorViewModel(parent, message) {
        this._parent = parent;
        this.errorMessage = ko.observable(message);
    }
    ErrorViewModel.prototype.dismiss = function () {
        this._parent.dismissError();
    };
    return ErrorViewModel;
})();
var FormattedPrice = (function () {
    function FormattedPrice(bigFigures, pips, tenthOfPips) {
        this.bigFigures = bigFigures;
        this.pips = pips;
        this.tenthOfPips = tenthOfPips;
    }
    return FormattedPrice;
})();
var OneWayPriceViewModel = (function () {
    function OneWayPriceViewModel(parent, direction) {
        this._parent = parent;
        this.direction = direction == 0 /* Buy */ ? "BUY" : "SELL";

        this.bigFigures = ko.observable("");
        this.pips = ko.observable("");
        this.tenthOfPips = ko.observable("");
    }
    OneWayPriceViewModel.prototype.onExecute = function () {
        var _this = this;
        this._parent.executing(true);

        this._executablePrice.execute(this._parent.notional(), this._parent.dealtCurrency).subscribe(function (trade) {
            return _this.onExecuted(trade);
        }, function (ex) {
            return _this.onExecutionError(ex);
        });
    };

    OneWayPriceViewModel.prototype.onPrice = function (executablePrice) {
        this._executablePrice = executablePrice;

        var formattedPrice = PriceFormatter.getFormattedPrice(executablePrice.rate, executablePrice.parent.currencyPair.ratePrecision, executablePrice.parent.currencyPair.pipsPosition);

        this.bigFigures(formattedPrice.bigFigures);
        this.pips(formattedPrice.pips);
        this.tenthOfPips(formattedPrice.tenthOfPips);
        this._parent.stale(false);
    };

    OneWayPriceViewModel.prototype.onStalePrice = function () {
        this._executablePrice = null;
        this.bigFigures("");
        this.pips("");
        this.tenthOfPips("");
        this._parent.stale(true);
    };

    OneWayPriceViewModel.prototype.onExecuted = function (trade) {
        if (trade.isStale) {
            this.onExecutionTimeout();
        } else {
            console.log("Trade executed.");
            this._parent.onTrade(trade.update);
        }
        this._parent.executing(false);
    };

    OneWayPriceViewModel.prototype.onExecutionTimeout = function () {
        console.error("Trade execution request timed out.");
        this._parent.onExecutionError("No response was received from the server, the execution status is unknown. Please contact your sales representative.");
    };

    OneWayPriceViewModel.prototype.onExecutionError = function (ex) {
        this._parent.onExecutionError(ex.message);
    };
    return OneWayPriceViewModel;
})();
var PriceFormatter = (function () {
    function PriceFormatter() {
    }
    PriceFormatter.getFormattedPrice = function (rate, precision, pipsPosition) {
        var rateAsString = rate.toFixed(precision);

        var dotIndex = rateAsString.indexOf(".");

        var bigFigures = rateAsString.substring(0, dotIndex + pipsPosition - 1);
        var pips = rateAsString.substring(dotIndex + pipsPosition - 1, dotIndex + pipsPosition + 1);

        var tenthOfPips = "";

        if (precision > pipsPosition) {
            tenthOfPips = rateAsString.substring(dotIndex + pipsPosition + 1, rateAsString.length);
        }

        return new FormattedPrice(bigFigures, pips, tenthOfPips);
    };

    PriceFormatter.getFormattedSpread = function (spread, precision, pipsPosition) {
        var delta = precision - pipsPosition;
        if (delta > 0) {
            return spread.toFixed(delta);
        }
        return spread.toString();
    };
    return PriceFormatter;
})();
var PriceMovement;
(function (PriceMovement) {
    PriceMovement[PriceMovement["None"] = 0] = "None";
    PriceMovement[PriceMovement["Down"] = 1] = "Down";
    PriceMovement[PriceMovement["Up"] = 2] = "Up";
})(PriceMovement || (PriceMovement = {}));
var PricingViewModel = (function () {
    function PricingViewModel(currencyPair, priceLatencyRecorder, parent) {
        this._priceLatencyRecorder = priceLatencyRecorder;
        this.symbol = currencyPair.baseCurrency + " / " + currencyPair.counterCurrency;
        this._priceSubscription = new Rx.SerialDisposable();
        this._currencyPair = currencyPair;
        this._parent = parent;
        this.bid = new OneWayPriceViewModel(this, 1 /* Sell */);
        this.ask = new OneWayPriceViewModel(this, 0 /* Buy */);
        this.notional = ko.observable(1000000);
        this.dealtCurrency = currencyPair.baseCurrency;
        this.spread = ko.observable("");
        this.movement = ko.observable(0 /* None */);
        this.spotDate = ko.observable("SP");
        this.isSubscribing = ko.observable(true);
        this.isStale = ko.observable(false);
        this.isExecuting = ko.observable(false);

        this.subscribeForPrices();
    }
    Object.defineProperty(PricingViewModel.prototype, "currencyPair", {
        get: function () {
            return this._currencyPair;
        },
        enumerable: true,
        configurable: true
    });

    PricingViewModel.prototype.dispose = function () {
        if (!this._disposed) {
            this._priceSubscription.dispose();
            this._disposed = true;
        }
    };

    PricingViewModel.prototype.onTrade = function (trade) {
        this._parent.onTrade(trade);
    };

    PricingViewModel.prototype.subscribeForPrices = function () {
        var _this = this;
        var subscription = this._currencyPair.prices.subscribe(function (price) {
            return _this.onPrice(price);
        }, function (ex) {
            return console.error(ex);
        });

        this._priceSubscription.setDisposable(subscription);
    };

    PricingViewModel.prototype.onPrice = function (price) {
        this.isSubscribing(false);

        if (price.isStale) {
            this.bid.onStalePrice();
            this.ask.onStalePrice();
            this.spread("");
            this._previousRate = null;
            this.movement(0 /* None */);
            this.spotDate("SP");
        } else {
            if (this._previousRate != null) {
                if (price.mid > this._previousRate) {
                    this.movement(2 /* Up */);
                } else if (price.mid < this._previousRate) {
                    this.movement(1 /* Down */);
                } else {
                    this.movement(0 /* None */);
                }
            }

            this._previousRate = price.mid;
            this.bid.onPrice(price.bid);
            this.ask.onPrice(price.ask);

            this.spread(PriceFormatter.getFormattedSpread(price.spread, this._currencyPair.ratePrecision, this._currencyPair.pipsPosition));
            this.spotDate("SP. " + DateUtils.formatDateDayMonth(price.valueDate));

            this._priceLatencyRecorder.onRendered(price);
        }
    };

    PricingViewModel.prototype.onExecutionError = function (message) {
        this._parent.onExecutionError(message);
    };

    PricingViewModel.prototype.stale = function (value) {
        this.isStale(value);
    };

    PricingViewModel.prototype.executing = function (value) {
        this.isExecuting(value);
    };
    return PricingViewModel;
})();
var PricingViewModelFactory = (function () {
    function PricingViewModelFactory(priceLatencyRecorder) {
        this._priceLatencyRecorder = priceLatencyRecorder;
    }
    PricingViewModelFactory.prototype.create = function (currencyPair, parent) {
        return new PricingViewModel(currencyPair, this._priceLatencyRecorder, parent);
    };
    return PricingViewModelFactory;
})();
var SpotTilesViewModel = (function () {
    function SpotTilesViewModel(referenceDataRepository, pricingViewModelFactory) {
        this._referenceDataRepository = referenceDataRepository;
        this._pricingViewModelFactory = pricingViewModelFactory;
        this.spotTiles = ko.observableArray([]);

        this._config = new SpotTileViewModel(null, 2 /* Conflate */, this._pricingViewModelFactory);
        this._config.toConfig();

        // TODO this.spotTiles.push(this._config);
        this._subscriptionModeDisposable = this._config.config().subscriptionMode.subscribe(function (subscriptionMode) {
            // TODO
        });

        this._executionModeDisposable = this._config.config().executionMode.subscribe(function (executionMode) {
            // TODO
        });

        this.loadSpotTiles();
    }
    SpotTilesViewModel.prototype.dispose = function () {
        this._executionModeDisposable.dispose();
        this._subscriptionModeDisposable.dispose();
    };

    SpotTilesViewModel.prototype.loadSpotTiles = function () {
        var _this = this;
        this._currencyPairsSubscription = this._referenceDataRepository.getCurrencyPairsStream().subscribe(function (currencyPairs) {
            return currencyPairs.forEach(function (cp) {
                return _this.handleCurrencyPairUpdate(cp);
            });
        }, function (ex) {
            return console.error("Failed to get currencies", ex);
        });
    };

    SpotTilesViewModel.prototype.handleCurrencyPairUpdate = function (update) {
        var spotTileViewModel = ko.utils.arrayFirst(this.spotTiles(), function (stvm) {
            return stvm.currencyPair == update.currencyPair.symbol;
        });

        if (update.updateType == 0 /* Add */) {
            if (spotTileViewModel != null) {
                // we already have a tile for this ccy pair
                return;
            }

            var spotTile = new SpotTileViewModel(update.currencyPair, this._config.config().subscriptionMode(), this._pricingViewModelFactory);
            this.spotTiles.push(spotTile);
        } else {
            if (spotTileViewModel != null) {
                this.spotTiles.remove(spotTileViewModel);
                spotTileViewModel.dispose();
            }
        }
    };

    SpotTilesViewModel.prototype.disconnect = function () {
        ko.utils.arrayForEach(this.spotTiles(), function (spotTile) {
            return spotTile.disconnect();
        });
        this._currencyPairsSubscription.dispose();
    };
    return SpotTilesViewModel;
})();
var SpotTileViewModel = (function () {
    function SpotTileViewModel(currencyPair, subscriptionMode, pricingFactory) {
        this.pricing = ko.observable(null);
        this.affirmation = ko.observable(null);
        this.error = ko.observable(null);
        this.config = ko.observable(null);
        this.state = ko.observable(0 /* Pricing */);
        this._disposed = false;
        if (currencyPair != null) {
            this.pricing(pricingFactory.create(currencyPair, this));
            this.currencyPair = currencyPair.symbol;
        }
    }
    SpotTileViewModel.prototype.dispose = function () {
        if (!this._disposed) {
            this.pricing().dispose();
            this._disposed = true;
        }
    };

    SpotTileViewModel.prototype.onTrade = function (trade) {
        var _this = this;
        this.affirmation(new AffirmationViewModel(trade, this));
        this.state(1 /* Affirmation */);
        this.error = null;

        Rx.Observable.timer(3000, Rx.Scheduler.timeout).subscribe(function (_) {
            return _this.dismissAffirmation();
        });
    };

    SpotTileViewModel.prototype.onExecutionError = function (message) {
        this.error(new ErrorViewModel(this, message));
        this.state(2 /* Error */);
    };

    SpotTileViewModel.prototype.dismissAffirmation = function () {
        this.state(0 /* Pricing */);
        this.affirmation(null);
    };

    SpotTileViewModel.prototype.dismissError = function () {
        this.state(0 /* Pricing */);
        this.error(null);
    };

    SpotTileViewModel.prototype.toConfig = function () {
        this.state(3 /* Config */);
        this.config(new ConfigViewModel());
    };

    SpotTileViewModel.prototype.disconnect = function () {
        this.pricing().dispose();
    };
    return SpotTileViewModel;
})();
var TileState;
(function (TileState) {
    TileState[TileState["Pricing"] = 0] = "Pricing";
    TileState[TileState["Affirmation"] = 1] = "Affirmation";
    TileState[TileState["Error"] = 2] = "Error";
    TileState[TileState["Config"] = 3] = "Config";
})(TileState || (TileState = {}));
var ReactiveTrader = (function () {
    function ReactiveTrader() {
    }
    ReactiveTrader.prototype.initialize = function (username, servers) {
        this._connectionProvider = new ConnectionProvider(username, servers);

        this.priceLatencyRecorder = new PriceLatencyRecorder();
        var referenceDataServiceClient = new ReferenceDataServiceClient(this._connectionProvider);
        var executionServiceClient = new ExecutionServiceClient(this._connectionProvider);
        var blotterServiceClient = new BlotterServiceClient(this._connectionProvider);
        var pricingServiceClient = new PricingServiceClient(this._connectionProvider);

        var tradeFactory = new TradeFactory();
        var executionRepository = new ExecutionRepository(executionServiceClient, tradeFactory);
        var profiler = new Profiler();
        var priceFactory = new PriceFactory(executionRepository, this.priceLatencyRecorder, profiler);
        var priceRepository = new PriceRepository(pricingServiceClient, priceFactory);
        var currencyPairUpdateFactory = new CurrencyPairUpdateFactory(priceRepository);

        this.tradeRepository = new TradeRepository(blotterServiceClient, tradeFactory);
        this.referenceDataRepository = new ReferenceDataRepository(referenceDataServiceClient, currencyPairUpdateFactory);
    };

    Object.defineProperty(ReactiveTrader.prototype, "connectionStatusStream", {
        get: function () {
            return this._connectionProvider.getActiveConnection().select(function (c) {
                return c.status;
            }).switchLatest().publish().refCount();
        },
        enumerable: true,
        configurable: true
    });

    ReactiveTrader.prototype.dispose = function () {
        this._connectionProvider.dispose();
    };
    return ReactiveTrader;
})();
//# sourceMappingURL=generated.js.map
