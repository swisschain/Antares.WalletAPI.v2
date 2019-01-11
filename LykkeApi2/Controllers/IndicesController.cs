﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Service.CryptoIndex.Client.Models;
using Lykke.Service.IndicesFacade.Client;
using Lykke.Service.IndicesFacade.Client.Models;
using LykkeApi2.Strings;

namespace LykkeApi2.Controllers
{
    [Route("api/indices")]
    [ApiController]
    public class IndicesController : Controller
    {
        private readonly IIndicesFacadeClient _indicesFacadeClient;

        public IndicesController(IIndicesFacadeClient indicesFacadeClient)
        {
            _indicesFacadeClient = indicesFacadeClient;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(IList<Index>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _indicesFacadeClient.Api.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{assetId}")]
        [ProducesResponseType(typeof(Index), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IActionResult> GetAsync(string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest(string.Format(Phrases.FieldShouldNotBeEmptyFormat, nameof(assetId)));

            Index result;
            try
            {
                result = await _indicesFacadeClient.Api.GetAsync(assetId);
            }
            catch (ClientApiException)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("{assetId}/history/{timeInterval}")]
        [ProducesResponseType(typeof(IList<HistoryElement>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ResponseCache(Duration = 10, VaryByQueryKeys = new[] { "*" })]
        public async Task<IActionResult> GetHistoryAsync(string assetId, TimeInterval timeInterval)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest(string.Format(Phrases.FieldShouldNotBeEmptyFormat, nameof(assetId)));

            if (timeInterval == TimeInterval.Unspecified)
                return BadRequest(string.Format(Phrases.FieldShouldNotBeEmptyFormat, nameof(timeInterval)));

            IList<HistoryElement> result;
            try
            {
                result = await _indicesFacadeClient.Api.GetHistoryAsync(assetId, timeInterval);
            }
            catch (ClientApiException)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
