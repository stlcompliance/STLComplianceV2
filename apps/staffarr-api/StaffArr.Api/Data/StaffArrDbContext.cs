using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace StaffArr.Api.Data;

public sealed class StaffArrDbContext(DbContextOptions<StaffArrDbContext> options) : PlatformDbContext(options);
