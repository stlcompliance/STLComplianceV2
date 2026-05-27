using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Data;

public sealed class MaintainArrDbContext(DbContextOptions<MaintainArrDbContext> options) : PlatformDbContext(options);
