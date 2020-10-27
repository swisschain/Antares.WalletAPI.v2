using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Blockchain;
using Lykke.Common.Log;
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

        public SiriusWalletsService(
            long brokerAccountId,
            IApiClient siriusApiClient,
            ILog log)
        {
            _brokerAccountId = brokerAccountId;
            _siriusApiClient = siriusApiClient;
            _log = log;
        }

        public async Task CreateWalletsAsync(string clientId)
        {
            var accountResponse = await _siriusApiClient.Accounts.SearchAsync(new AccountSearchRequest
            {
                ReferenceId = clientId,
                Pagination = new PaginationInt64{Limit = 100}
            });

            if (accountResponse.ResultCase == AccountSearchResponse.ResultOneofCase.Error)
            {
                _log.Warning("Error getting wallets from sirius", context: $"Error: {accountResponse.Error.ToJson()}");
            }

            if (accountResponse.ResultCase == AccountSearchResponse.ResultOneofCase.Body &&
                accountResponse.Body.Items.Count == 0)
            {
                _log.Info("Creating wallets in sirius", context: clientId);

                var createResponse = await _siriusApiClient.Accounts.CreateAsync(new AccountCreateRequest
                {
                    BrokerAccountId = _brokerAccountId, ReferenceId = clientId
                });

                if (createResponse.ResultCase == AccountCreateResponse.ResultOneofCase.Error)
                {
                    _log.Warning("Error creating wallets in sirius", context: $"Error: {createResponse.Error.ToJson()}");
                }
                else
                {
                    _log.Info("Wallets created in siruis", context: $"Result: {createResponse.Body.Account.ToJson()}");
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
                _log.Warning("Error getting wallet from sirius", context: $"Error: {searchResponse.Error.ToJson()}, clientId: {clientId}, assetId: {assetId}");
            }

            return searchResponse.ResultCase == AccountDetailsSearchResponse.ResultOneofCase.Body
                ? searchResponse.Body.Items.FirstOrDefault()
                : null;
        }
    }
}
