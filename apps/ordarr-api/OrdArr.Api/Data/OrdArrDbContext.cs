using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace OrdArr.Api.Data;

public sealed class OrdArrDbContext(DbContextOptions<OrdArrDbContext> options) : PlatformDbContext(options);
