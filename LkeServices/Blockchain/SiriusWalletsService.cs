using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Blockchain;
using Polly;
using Polly.Retry;
using Swisschain.Sirius.Api.ApiClient;
using Swisschain.Sirius.Api.ApiContract.Account;
using Swisschain.Sirius.Api.ApiContract.Common;

namespace LkeServices.Blockchain
{
    public class SiriusWalletsService : ISiriusWalletsService
    {
        private readonly long _brokerAccountId;
        private readonly IApiClient _siriusApiClient;
        private readonly ILog _log;

        private readonly RetryPolicy<AccountDetailsSearchResponse> _waitAccountCreationPolicy = Policy
            .HandleResult<AccountDetailsSearchResponse>(res =>
            {
                var hasWallets = res != null && res.Body.Items.Count > 0;
                Console.WriteLine(!hasWallets ? "No response yet..." : "Wallets created!");
                return !hasWallets;
            })
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(3));

        public SiriusWalletsService(
            long brokerAccountId,
            IApiClient siriusApiClient,
            ILog log)
        {
            _brokerAccountId = brokerAccountId;
            _siriusApiClient = siriusApiClient;
            _log = log;
        }

        public async Task CreateWalletsAsync(string clientId, bool waitForCreation)
        {
            var accountResponse = await _siriusApiClient.Accounts.SearchAsync(new AccountSearchRequest
            {
                ReferenceId = clientId,
                Pagination = new PaginationInt64{Limit = 100}
            });

            if (accountResponse.ResultCase == AccountSearchResponse.ResultOneofCase.Error)
            {
                _log.WriteWarning(nameof(CreateWalletsAsync), info: "Error getting wallets from sirius", context: $"Error: {accountResponse.Error.ToJson()}");
            }

            if (accountResponse.ResultCase == AccountSearchResponse.ResultOneofCase.Body &&
                accountResponse.Body.Items.Count == 0)
            {
                _log.WriteInfo(nameof(CreateWalletsAsync), info: "Creating wallets in sirius", context: clientId);

                var createResponse = await _siriusApiClient.Accounts.CreateAsync(new AccountCreateRequest
                {
                    RequestId = $"{_brokerAccountId}{clientId}",
                    BrokerAccountId = _brokerAccountId,
                    ReferenceId = clientId
                });

                if (createResponse.ResultCase == AccountCreateResponse.ResultOneofCase.Error)
                {
                    _log.WriteWarning(nameof(CreateWalletsAsync), info: "Error creating wallets in sirius", context: $"Error: {createResponse.Error.ToJson()}");
                }
                else
                {
                    _log.WriteInfo(nameof(CreateWalletsAsync), info: "Wallets created in siruis", context: $"Result: {createResponse.Body.Account.ToJson()}");
                }
                if (waitForCreation)
                {
                    var result = await _waitAccountCreationPolicy.ExecuteAsync(async () =>
                        await _siriusApiClient.Accounts.SearchDetailsAsync(new AccountDetailsSearchRequest
                        {
                            BrokerAccountId = _brokerAccountId,
                            ReferenceId = clientId
                        }));

                    _log.WriteInfo(nameof(CreateWalletsAsync), info: "Wallets created!", context: $"clientId: {clientId}");
                }
            }
        }

        public async Task<AccountDetailsResponse> GetWalletAdderssAsync(string clientId, long assetId)
        {
            var searchResponse = await _siriusApiClient.Accounts.SearchDetailsAsync(new AccountDetailsSearchRequest
            {
                BrokerAccountId = _brokerAccountId,
                ReferenceId = clientId,
                AssetId = assetId
            });

            if (searchResponse.ResultCase == AccountDetailsSearchResponse.ResultOneofCase.Error)
            {
                _log.WriteWarning(nameof(GetWalletAdderssAsync), info: "Error getting wallet from sirius", context: $"Error: {searchResponse.Error.ToJson()}, clientId: {clientId}, assetId: {assetId}");
            }

            return searchResponse.ResultCase == AccountDetailsSearchResponse.ResultOneofCase.Body
                ? searchResponse.Body.Items.FirstOrDefault()
                : null;
        }
    }
}
