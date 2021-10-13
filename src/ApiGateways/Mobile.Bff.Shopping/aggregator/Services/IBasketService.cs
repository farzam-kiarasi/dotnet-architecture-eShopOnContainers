﻿namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Services;

public interface IBasketService
{
    Task<BasketData> GetById(string id);

    Task UpdateAsync(BasketData currentBasket);

}
