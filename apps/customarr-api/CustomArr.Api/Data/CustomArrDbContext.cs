using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace CustomArr.Api.Data;

public sealed class CustomArrDbContext(DbContextOptions<CustomArrDbContext> options) : PlatformDbContext(options)
{
}
