﻿using performance_monitor_winui3.Core.Models;

namespace performance_monitor_winui3.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}
