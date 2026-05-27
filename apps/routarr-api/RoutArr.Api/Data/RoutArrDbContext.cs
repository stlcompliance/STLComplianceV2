using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace RoutArr.Api.Data;

public sealed class RoutArrDbContext(DbContextOptions<RoutArrDbContext> options) : PlatformDbContext(options);
