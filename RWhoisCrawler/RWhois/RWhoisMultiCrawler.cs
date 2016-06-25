﻿// -----------------------------------------------------------------------
// <copyright file="RWhoisMultiCrawler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Geolocation.RWhois.Crawler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NetTools;
    using NLog;

    public class RWhoisMultiCrawler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Crawl(Dictionary<string, string> organizationsToRefServers, Dictionary<string, HashSet<IPAddressRange>> organizationsToRefRanges)
        {
            foreach (var entry in organizationsToRefRanges)
            {
                var organizationId = entry.Key;
                var ranges = entry.Value;

                if (organizationId == null || ranges == null || ranges.Count == 0)
                {
                    continue;
                }

                string refServerUrl;
                Uri refServerUri;

                if (organizationsToRefServers.TryGetValue(organizationId, out refServerUrl) && Uri.TryCreate(refServerUrl, UriKind.Absolute, out refServerUri))
                {
                    var hostname = refServerUri.Host;
                    var port = refServerUri.Port;

                    try
                    {
                        logger.Info(string.Format(CultureInfo.InvariantCulture, "Starting crawler for organizationId: {0}, hostname: {1}, port: {2}", organizationId, hostname, port));

                        var crawler = new RWhoisCrawler(hostname, port);
                        var consumer = new RWhoisConsumer(string.Format(CultureInfo.InvariantCulture, "{0}.txt", organizationId));
                        crawler.Subscribe(consumer);
                        crawler.CrawlRanges(ranges);

                        logger.Info(string.Format(CultureInfo.InvariantCulture, "Done with crawler for organizationId: {0}, hostname: {1}, port: {2}", organizationId, hostname, port));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
        }
    }
}