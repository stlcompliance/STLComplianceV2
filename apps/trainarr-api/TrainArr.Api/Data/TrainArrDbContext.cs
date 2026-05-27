using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace TrainArr.Api.Data;

public sealed class TrainArrDbContext(DbContextOptions<TrainArrDbContext> options) : PlatformDbContext(options);
