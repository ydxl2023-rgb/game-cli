#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Config;

/// <summary>Isolated regression checks for template entity ownership.</summary>
public static class EntityContractProbe
{
    /// <summary>Verify validation, snapshot isolation and notification aggregation.</summary>
    /// <returns>The number of successful contract checks.</returns>
    public static int Check()
    {
        int passed = 0;
        var red = new RedDotEntity();
        red.Initialize();
        int mail = Excel.RED_DOT_HOME_MAILTIPS;
        red.Refresh(red.GetIds());
        Check(!red.UpdateSignal(mail, -1) && !red.UpdateSignal(int.MinValue, 1), ref passed);
        Check(red.UpdateSignal(mail, 7), ref passed);
        var changes = red.Refresh(new[] { mail });
        Check(red.GetCount(mail) == 7 && changes[mail] == 7, ref passed);
        Check(red.GetCount(Excel.RED_DOT_HOME) >= 7, ref passed);
        changes[mail] = 99;
        red.GetSignals()[mail] = 99;
        red.GetCounts()[mail] = 99;
        Check(red.Refresh(red.GetIds()).Count == 0 && red.GetCount(mail) == 7, ref passed);
        Check(red.UpdateSignal(mail, 0), ref passed);
        red.Refresh(new[] { mail });
        Check(red.GetCount(mail) == 0 && red.GetCount(Excel.RED_DOT_HOME) == 0, ref passed);

        var product = new ShopProduct { id = 1, category = ShopCategory.Recharge, price = 10, reward = 2, remaining = 1 };
        var shop = new ShopEntity();
        Check(shop.InitializeDemo(new[] { product }, 100), ref passed);
        product.price = 99;
        var products = shop.GetProducts();
        products[0].price = 50;
        products.Clear();
        Check(shop.GetProducts()[0].price == 10 && shop.productCount == 1, ref passed);
        Check(shop.SelectProduct(1) && shop.ConfirmPurchase() == ShopPurchaseResult.Succeeded, ref passed);
        Check(shop.demoBalance == 90 && shop.demoTokens == 2 && shop.GetProducts()[0].remaining == 0, ref passed);
        Check(shop.ConfirmPurchase() == ShopPurchaseResult.NoSelection && shop.demoBalance == 90, ref passed);

        var server = new ServerEntry { id = 1, name = "Local", state = ServerState.Normal };
        var login = new LoginEntity();
        Check(login.SetServers(new[] { server }), ref passed);
        server.state = ServerState.Maintenance;
        var servers = login.GetServers();
        servers[0].state = ServerState.Maintenance;
        servers.Clear();
        Check(login.serverCount == 1 && login.GetServers()[0].state == ServerState.Normal, ref passed);
        Check(!login.SetServers(new[] { server, server }) && login.serverCount == 1, ref passed);
        Check(!login.SelectServer(int.MinValue), ref passed);

        var home = new HomeEntity();
        Check(home.SetIndicator(HomeIndicator.AdsNode, false) && !home.IsIndicatorVisible(HomeIndicator.AdsNode), ref passed);
        Check(!home.SetIndicator((HomeIndicator)int.MinValue, true), ref passed);

        return passed;
    }

    /// <summary>Fail immediately when an entity contract changes unexpectedly.</summary>
    /// <param name="condition">Expected invariant.</param>
    /// <param name="passed">Successful check counter.</param>
    private static void Check(bool condition, ref int passed)
    {
        if (!condition)
            throw new InvalidOperationException($"Entity contract failed after {passed} checks.");

        passed++;
    }
}
#endif
