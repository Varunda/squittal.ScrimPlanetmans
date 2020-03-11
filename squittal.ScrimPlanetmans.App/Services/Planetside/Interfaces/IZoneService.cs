﻿using squittal.ScrimPlanetmans.Shared.Models.Planetside;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.ScrimPlanetmans.Services.Planetside
{
    public interface IZoneService
    {
        IEnumerable<Zone> GetAllZones();
        Task<IEnumerable<Zone>> GetAllZonesAsync();
        Task<Zone> GetZoneAsync(int zoneId);
        Task RefreshStore();
        Task SetupZonesList();
    }
}
