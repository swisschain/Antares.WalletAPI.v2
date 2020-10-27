using System.Linq;
using System.Threading.Tasks;
using Core.Blockchain;
using Swisschain.Sirius.Api.ApiClient;
using Swisschain.Sirius.Api.ApiContract.Account;
using Swisschain.Sirius.Api.ApiContract.Common;

namespace LkeServices.Blockchain
{
    public class SiriusWalletsService : ISiriusWalletsService
    {
        private readonly long _brokerAccountId;
        private readonly IApiClient _siriusApiClient;

        public SiriusWalletsService(
            long brokerAccountId,
            IApiClient siriusApiClient)
        {
            _brokerAccountId = brokerAccountId;
            _siriusApiClient = siriusApiClient;
        }

        public async Task CreateWalletsAsync(string clientId)
        {
            var accountResponse = await _siriusApiClient.Accounts.SearchAsync(new AccountSearchRequest
            {
                ReferenceId = clientId,
                Pagination = new PaginationInt64{Limit = 100}
            });

            if (accountResponse.ResultCase == AccountSearchResponse.ResultOneofCase.Body &&
                accountResponse.Body.Items.Count == 0)
            {
                var createResponse = await _siriusApiClient.Accounts.CreateAsync(new AccountCreateRequest
                {
                    BrokerAccountId = _brokerAccountId, ReferenceId = clientId
                });

                var x = createResponse.Body.Account;
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

            return searchResponse.ResultCase == AccountDetailsSearchResponse.ResultOneofCase.Body
                ? searchResponse.Body.Items.FirstOrDefault()
                : null;
        }
    }
}
