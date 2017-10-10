﻿using Common.Log;
using Lykke.Service.Balances.Client;
using Lykke.Service.Balances.AutorestClient.Models;
using LykkeApi2.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Lykke.WalletApiv2.Tests.ClientBalances
{
    public class ClientBalancesTest
    {
        private ClientBalancesController _controller;

        [Fact]
        public async Task GetClientBalancesByClientId_ReturnsOk()
        {
            var logs = new Mock<ILog>();
            var walletsClient = new Mock<IBalancesClient>();
            walletsClient.Setup(x => x.GetClientBalances(It.IsAny<string>()))
                .Returns(CreateMockedResponseForClientBalances.GetAllBalancesForClient);

            _controller = new ClientBalancesController(logs.Object, walletsClient.Object);

            var result = await _controller.Get();

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetClientBalancesByClientIdAndAssetId_ReturnsOk()
        {
            var logs = new Mock<ILog>();
            var walletsClient = new Mock<IBalancesClient>();
            walletsClient.Setup(x => x.GetClientBalanceByAssetId(It.IsAny<ClientBalanceByAssetIdModel>()))
                .Returns(CreateMockedResponseForClientBalances.GetAllBalancesForClientByAssetId);

            _controller = new ClientBalancesController(logs.Object, walletsClient.Object);

            var result = await _controller.GetClientBalanceByAssetId("USD");

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
