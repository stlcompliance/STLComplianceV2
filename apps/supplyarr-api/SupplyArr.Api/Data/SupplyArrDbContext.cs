using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Data;

public sealed class SupplyArrDbContext(DbContextOptions<SupplyArrDbContext> options) : PlatformDbContext(options);
